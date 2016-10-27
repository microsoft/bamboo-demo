#pragma once

#include "Encoder.h"

using namespace Windows::Devices::Pwm;
using namespace Windows::Foundation;
using namespace concurrency;

namespace Microsoft
{
namespace Maker
{
	// By using a scope to measure the actual output frequency we found
	// that the internal clock on the PCA9685 generated a PWM frequency
	// that was 6.3% too fast. Setting the frequency to 94 yields a 
	// 99.7Hz PWM signal
	#define PWM_FREQUENCY           94
	#define PWM_ACTUAL_FREQUENCY    99.7
	#define THROTTLE_RANGE          0.0005  // 0.5 milliseconds
	#define THROTTLE_MIDPOINT       0.0015  // 1.5 milliseconds
	#define THROTTLE_FULL_FORWARD   100.0
	#define THROTTLE_FULL_REVERSE   -100.0

	ref class Motor;
	public delegate void MotorRpmUpdatedEventHandler(Motor^ sender, int32_t newRpm);

	public ref class Motor sealed
	{
	private:
		int _motorPin;
		int _encoderPin;
		Encoder ^_encoder;
		PwmPin ^_motorPwm;
		double _throttle;
		long _runningPulseCount;
		void SetThrottle(double percent);

		static PwmController ^_pwmController;
		static IAsyncOperation<PwmController^>^ GetPwmController();

		void OnRpmUpdated(Microsoft::Maker::Encoder ^sender, int32_t newRpm);

	public:
		Motor(int motorPin, int encoderPin, double encoderPPR);
		IAsyncAction^ Initialize();
		event MotorRpmUpdatedEventHandler^ RpmUpdated;

		property double Throttle {
			double get() { return _throttle; }
			void set(double value)
			{
				SetThrottle(value);
			}
		}

		property int RPM {
			int get() { 
				if (_throttle >= 0)
					return _encoder->GetCurrentRPM();
				else
					return _encoder->GetCurrentRPM() * -1;
			}
		}

		property LONGLONG EncoderPulses {
			LONGLONG get() 
			{ 
				if(_throttle > 0)
					return _encoder->GetPulseCount(); 
				else
					return _encoder->GetPulseCount() * -1;
			}
		}
	};

	PwmController^ Motor::_pwmController = nullptr;
} // end Maker namespace
} // end Microsoft namespace
