using System;
using Windows.Foundation;
using Bamboo.Utilities;

namespace Bamboo.Models
{
    public sealed class Odometer
    {
        private static volatile Odometer instance;
        private static object syncRoot = new object();

        public OdometryData Data { get; set; }
        private Point positionThreshold { get; set; }
        private Double angleThreshold { get; set; }
        private double previousDelta = double.MaxValue;
        private double previousAngleDelta = double.MaxValue;

        public event EventHandler<OdometryThresholdReachedEventArgs> ThresholdPositionReached;
        public event EventHandler<OdometryThresholdReachedEventArgs> ThresholdAngleReached;
        public event EventHandler<OdometryThresholdReachedEventArgs> PositionChanged;

        private Odometer()
        {
            Data = new OdometryData();
            Data.X = 0d;
            Data.Y = 0d;
            Data.Theta = 0d;
        }

        public static Odometer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Odometer();
                    }
                }
                return instance;
            }
        }

        void OnOdometryThresholdPositionReached(OdometryThresholdReachedEventArgs args)
        {
            ThresholdPositionReached?.Invoke(this, args);
        }

        void OnOdometryThresholdAngleReached(OdometryThresholdReachedEventArgs args)
        {
            ThresholdAngleReached?.Invoke(this, args);
        }

        void OnPositionChanged(OdometryThresholdReachedEventArgs args)
        {
            PositionChanged?.Invoke(this, args);
        }

        private double CalculateDistance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        private double CalculateArcLength(double thetaA, double thetaB)
        {
            if (thetaA < 0) thetaA += 360;
            if (thetaB < 0) thetaB += 360;
            // thetaA and thetaB assumed to be in degrees
            double arcA = Math.PI * (Configuration.AXLE_LENGTH / 2) * thetaA / 180;
            double arcB = Math.PI * (Configuration.AXLE_LENGTH / 2) * thetaB / 180;

            return Math.Abs(arcA - arcB);
        }

        public void Reset()
        {
            Data.X = 0d;
            Data.Y = 0d;
            Data.Theta = 0d;
        }

        // The dead reckoning code for this method is a C# port of the code found at
        // http://www.seattlerobotics.org/encoder/200010/dead_reckoning_article.html
        // Used with permission - Implementing Dead Reckoning by Odometry on a Robot 
        // with R/C Servo Differential Drive Copyright © 2000, Dafydd Walters

        public void Update(int leftRPM, long leftPulses, int rightRPM, long rightPulses)
        {
            float dist_left;
            float dist_right;
            long left_ticks;
            long right_ticks;
            float expr1;
            float cos_current;
            float sin_current;
            float right_minus_left;
            double current_theta;

            // Update the local OdemetryData with the information from the drivetrain
            lock (syncRoot)
            {
                left_ticks = leftPulses;
                right_ticks = rightPulses;

                dist_left = left_ticks * Configuration.MUL_COUNT;
                dist_right = right_ticks * Configuration.MUL_COUNT;

                current_theta = Data.Theta * (Math.PI / 180);
                cos_current = (float)Math.Cos(current_theta);
                sin_current = (float)Math.Sin(current_theta);

                if(left_ticks == right_ticks)
                {
                    Data.X += dist_left * cos_current;
                    Data.Y += dist_left * sin_current;
                }
                else
                {
                    expr1 = (float)Configuration.AXLE_LENGTH * (dist_right + dist_left) / 2.0f / (dist_right - dist_left);

                    right_minus_left = dist_right - dist_left;

                    Data.X += expr1 * (Math.Sin(right_minus_left /
                                          Configuration.AXLE_LENGTH + current_theta) - sin_current);

                    Data.Y -= expr1 * (Math.Cos(right_minus_left /
                                          Configuration.AXLE_LENGTH + current_theta) - cos_current);

                    /* Calculate new orientation */
                    current_theta += right_minus_left / Configuration.AXLE_LENGTH;

                    /* Keep in the range -PI to +PI */
                    while (current_theta > Math.PI)
                        current_theta -= (2.0 * Math.PI);
                    while (current_theta < -Math.PI)
                        current_theta += (2.0 * Math.PI);

                    // Map to degrees
                    Data.Theta = current_theta * 180 / Math.PI;
                }

                double delta = CalculateDistance(positionThreshold, new Point { X = Data.X, Y = Data.Y });
                double angleDelta = CalculateArcLength(angleThreshold, Data.Theta);
                if (previousDelta != delta)
                    Logger.GetInstance().LogLine($"Position: x:{Data.X} y:{Data.Y} Angle:{Data.Theta}  Target: x:{positionThreshold.X} y:{positionThreshold.Y} {angleThreshold}  Distance: {delta} Arc: {angleDelta}");

                // Check to see if we've hit a threshold
                if (delta < 0.01 || (delta > previousDelta))
                {
                    OnOdometryThresholdPositionReached(new OdometryThresholdReachedEventArgs { Data = Data });
                }

                if (Math.Abs(angleThreshold - Data.Theta) < 5)
                {
                    OnOdometryThresholdAngleReached(new OdometryThresholdReachedEventArgs { Data = Data });
                }

                previousAngleDelta = angleDelta;
                previousDelta = delta;
            }

            OnPositionChanged(new OdometryThresholdReachedEventArgs { Data = Data });
        }

        public void SetTrackingThreshold(double x, double y, double angle)
        {
            previousDelta = double.MaxValue;
            previousAngleDelta = double.MaxValue;
            positionThreshold = new Point { X = x, Y = y };
            angleThreshold = angle;
        }
    }

    public class OdometryData
    {
        public OdometryData(){ }

        public OdometryData(OdometryData data)
        {
            X = data.X;
            Y = data.Y;
            Theta = data.Theta;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Theta { get; set; }
    }

    public class OdometryThresholdReachedEventArgs : EventArgs
    {
        public OdometryData Data { get; set; }
    }
}
