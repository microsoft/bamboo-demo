using Bamboo.Utilities;
using Microsoft.Maker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace Bamboo.Models
{
    public class Drivetrain
    {
        private Motor leftMotor;
        private Motor rightMotor;
        private AutoResetEvent updateReady = new AutoResetEvent(true);
        private ThreadPoolTimer updateTimer;

        public PidController.PidController leftMotorPid;
        public PidController.PidController rightMotorPid;

        public Drivetrain()
        {
            try
            {
                leftMotor = new Motor(Configuration.LEFT_MOTOR_PWM_CHANNEL, Configuration.LEFT_MOTOR_ENCODER_INPUT_PIN, Configuration.PULSES_PER_REVOLUTION);
                rightMotor = new Motor(Configuration.RIGHT_MOTOR_PWM_CHANNEL, Configuration.RIGHT_MOTOR_ENCODER_INPUT_PIN, Configuration.PULSES_PER_REVOLUTION);
                
                leftMotorPid = new PidController.PidController(Configuration.PROPORTIONAL_GAIN, Configuration.INTEGRAL_GAIN, Configuration.DERIVATIVE_GAIN, Configuration.MAX_FORWARD_THROTTLE, Configuration.MAX_REVERSE_THROTTLE);
                rightMotorPid = new PidController.PidController(Configuration.PROPORTIONAL_GAIN, Configuration.INTEGRAL_GAIN, Configuration.DERIVATIVE_GAIN, Configuration.MAX_FORWARD_THROTTLE, Configuration.MAX_REVERSE_THROTTLE);

                leftMotorPid.SetPoint = 0f;
                rightMotorPid.SetPoint = 0f;
            }
            catch (Exception ex)
            {
                Logger.GetInstance().LogLine(ex.Message);
                throw ex;
            }
        }

        public async Task Initialize()
        {
            await leftMotor.Initialize();
            await rightMotor.Initialize();

            // Setup PID update thread
            updateTimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                updateReady.WaitOne();
                UpdatePidAndOdometry(updateReady);
            }, TimeSpan.FromMilliseconds(Configuration.UPDATE_INTERVAL_MS));
        }

        // Travel forward a fixed distance in meters.
        public async Task Forward(double distanceInMeters)
        {
            await Stop();
            double theta = Odometer.Instance.Data.Theta;
            double targetY = Odometer.Instance.Data.Y + Math.Sin(Math.PI * theta / 180) * distanceInMeters;
            double targetX = Odometer.Instance.Data.X + Math.Cos(Math.PI * theta / 180) * distanceInMeters;
            Odometer.Instance.SetTrackingThreshold(targetX, targetY, theta);
            Odometer.Instance.ThresholdPositionReached += Instance_ThresholdPositionReached;

            // Move the wheels at the target RPM
            leftMotorPid.SetPoint = Configuration.TARGET_RPM;
            rightMotorPid.SetPoint = Configuration.TARGET_RPM;
        }

        public async Task Stop()
        {
            leftMotorPid.SetPoint = 0f;
            rightMotorPid.SetPoint = 0f;
            // Give the platform time to stop rolling before continuing
            await Task.Delay(250);
            Odometer.Instance.ThresholdPositionReached -= Instance_ThresholdPositionReached;
            Odometer.Instance.ThresholdAngleReached -= Instance_ThresholdAngleReached;
        }

        public async Task Reverse(double distanceInMeters)
        {
            await Stop();
            double theta = Odometer.Instance.Data.Theta;
            double targetY = Odometer.Instance.Data.Y - Math.Sin(Math.PI * theta / 180) * distanceInMeters;
            double targetX = Odometer.Instance.Data.X - Math.Cos(Math.PI * theta / 180) * distanceInMeters;
            Odometer.Instance.SetTrackingThreshold(targetX, targetY, theta);
            Odometer.Instance.ThresholdPositionReached += Instance_ThresholdPositionReached;
            leftMotorPid.SetPoint = Configuration.TARGET_RPM * -1f;
            rightMotorPid.SetPoint = Configuration.TARGET_RPM * -1f;
        }

        public async Task TurnLeft(double theta)
        {
            await Stop();

            double targetTheta = Odometer.Instance.Data.Theta + theta;
            if (targetTheta > 180)
                targetTheta -= 360;

            Odometer.Instance.SetTrackingThreshold(Odometer.Instance.Data.X, Odometer.Instance.Data.Y, targetTheta);
            Odometer.Instance.ThresholdAngleReached += Instance_ThresholdAngleReached;
            leftMotorPid.SetPoint = Configuration.TARGET_TURN_RPM * -1f;
            rightMotorPid.SetPoint = Configuration.TARGET_TURN_RPM;
        }

        public async Task TurnRight(double theta)
        {
            await Stop();

            double targetTheta = Odometer.Instance.Data.Theta - theta;
            if (targetTheta < -180)
                targetTheta += 360;

            Odometer.Instance.SetTrackingThreshold(Odometer.Instance.Data.X, Odometer.Instance.Data.Y, targetTheta);
            Odometer.Instance.ThresholdAngleReached += Instance_ThresholdAngleReached;
            leftMotorPid.SetPoint = Configuration.TARGET_TURN_RPM;
            rightMotorPid.SetPoint = Configuration.TARGET_TURN_RPM * -1f;
        }

        private void UpdatePID(int leftRPM, int rightRPM)
        {
            leftMotorPid.ProcessVariable = leftRPM;
            rightMotorPid.ProcessVariable = rightRPM;

            leftMotor.Throttle = leftMotorPid.ControlVariable;
            rightMotor.Throttle = rightMotorPid.ControlVariable;
        }

        private void UpdatePidAndOdometry(AutoResetEvent resetEvent)
        {
            int leftRPM = leftMotor.RPM;
            int rightRPM = rightMotor.RPM;

            // Update the PID while we are here. Have to do this first. Don't ask.
            UpdatePID(leftRPM, rightRPM);

            // Update the Odometry
            Odometer.Instance.Update(leftRPM, leftMotor.EncoderPulses, rightRPM, rightMotor.EncoderPulses);

            resetEvent.Set();
        }

        // Helper function for tuning the PID loop in realtime
        public void SetPIDValues(float p, float i, float d)
        {
            rightMotorPid.GainProportional = p;
            rightMotorPid.GainIntegral = i;
            rightMotorPid.GainDerivative = d;

            leftMotorPid.GainProportional = p;
            leftMotorPid.GainIntegral = i;
            leftMotorPid.GainDerivative = d;
        }

        private async void Instance_ThresholdPositionReached(object sender, OdometryThresholdReachedEventArgs e)
        {
            Logger.GetInstance().LogLine($"Arrived at position waypoint: ({e.Data.X},{e.Data.Y})");
            Odometer.Instance.ThresholdPositionReached -= Instance_ThresholdPositionReached;
            await Stop();
        }

        private async void Instance_ThresholdAngleReached(object sender, OdometryThresholdReachedEventArgs e)
        {
            Logger.GetInstance().LogLine($"Arrived at angle: {e.Data.Theta}");
            Odometer.Instance.ThresholdAngleReached -= Instance_ThresholdAngleReached;
            await Stop();
        }
    }
}
