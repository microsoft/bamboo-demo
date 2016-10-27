using System;

namespace Bamboo
{
    public static class Configuration
    {
        // Name of the robot. This will be the key word that causes the robot to start
        // listening for commands.
        public const String ROBOT_NAME = "Bamboo";

        // Mechanical constants for the robot platform
        public const double GEAR_RATIO = 46.85; // Gear ratio of the motor. Found in the datasheet.
        public const double ENCODER_PPR = 24.0; // Pulses per revolution of the encoder used
        public const double PULSES_PER_REVOLUTION = ENCODER_PPR * GEAR_RATIO;
        public const double WHEEL_DIAMETER = 0.090; // in meters
        public const double AXLE_LENGTH = 0.22; // in meters
        public const float MUL_COUNT = (float)(Math.PI * WHEEL_DIAMETER / PULSES_PER_REVOLUTION);

        // Only allow 25% power in forward and reverse.
        public const float MAX_FORWARD_THROTTLE = 25f;
        public const float MAX_REVERSE_THROTTLE = -25f;
        public const double MAX_RPM = 100.0;    // For safety limit the max RPM

        // The update interval for driving the PID loop. The more often the PID controller
        // is updated, the more accurate the drivetrain will track the desired RPM. Updating
        // it too frequently can cause performance degredation with little performance
        // benefit.
        public const double UPDATE_INTERVAL_MS = 100;

        // The PID gain constants were derived from trial and error tuning
        public const float PROPORTIONAL_GAIN = 0.1f;
        public const float INTEGRAL_GAIN = 0.15f;
        public const float DERIVATIVE_GAIN = 0f;

        // How fast the wheels should move when travelling fixed distances and turning
        public const float TARGET_RPM = 50f;
        public const float TARGET_TURN_RPM = 20f;

        // The pins to use on the carrier board for encoder input.
        public const int LEFT_MOTOR_ENCODER_INPUT_PIN = 8;
        public const int RIGHT_MOTOR_ENCODER_INPUT_PIN = 7;

        // The PWM output channels to use for motor control. These are referenced as an
        // index from 0 to 15 relating to the PWM outputs on the Adafruit PWM Shield.
        public const int LEFT_MOTOR_PWM_CHANNEL = 1;
        public const int RIGHT_MOTOR_PWM_CHANNEL = 0;
    }
}
