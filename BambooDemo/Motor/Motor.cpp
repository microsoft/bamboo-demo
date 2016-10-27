#include "pch.h"
#include <pplawait.h>
#include <agents.h>
#include "Motor.h"

using namespace Microsoft::Maker;
using namespace Platform;
using namespace Windows::Devices::Pwm;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Concurrency;
using namespace PwmPCA9685;

Microsoft::Maker::Motor::Motor(int motorPin, int encoderPin, double encoderPPR) : 
	_motorPin(motorPin), 
	_encoderPin(encoderPin), 
	_throttle(0), 
	_encoder(nullptr), 
	_motorPwm(nullptr)
{
	_encoder = ref new Encoder(encoderPin, encoderPPR);
	_encoder->RpmUpdated += ref new Microsoft::Maker::RpmUpdatedEventHandler(this, &Microsoft::Maker::Motor::OnRpmUpdated);
}

IAsyncOperation<PwmController^>^ Microsoft::Maker::Motor::GetPwmController() 
{
	if (_pwmController == nullptr)
	{
		return create_async([]() -> PwmController^ {
			create_task(PwmController::GetControllersAsync(PwmPCA9685::PwmProviderPCA9685::GetPwmProvider())).then([](IVectorView < PwmController ^> ^ providers) {
				_pwmController = providers->GetAt(0);
				_pwmController->SetDesiredFrequency(PWM_FREQUENCY);
			}).get();
			return _pwmController;
		});
			
	}
	else
	{
		return create_async([]()->PwmController^ {
			return _pwmController;
		});
	}
}

IAsyncAction^ Microsoft::Maker::Motor::Initialize()
{
	return create_async([this] {
		create_task(Microsoft::Maker::Motor::GetPwmController()).then([this](PwmController^ controller) {
			_motorPwm = controller->OpenPin(_motorPin);
			SetThrottle(0);
			_motorPwm->Start();
		}).get();
	});
}

void Microsoft::Maker::Motor::SetThrottle(double percent)
{
	if (nullptr != _motorPwm)
	{
		_throttle = percent;
		if (_throttle > THROTTLE_FULL_FORWARD) _throttle = THROTTLE_FULL_FORWARD;
		if (_throttle < THROTTLE_FULL_REVERSE) _throttle = THROTTLE_FULL_REVERSE;

		// For our motor the speed is determined by a pulse width in the range of 1-2ms.
		// 1ms is full reverse, 1.5ms is stopped, and 2ms is full forward.
		// Map the throttle percentage into the motor range and adjust it based on
		// the actual PWM frequency
		double actualDutyCyclePercentage = THROTTLE_MIDPOINT + (THROTTLE_RANGE * (_throttle / 100.0));
		actualDutyCyclePercentage *= PWM_ACTUAL_FREQUENCY;
		_motorPwm->SetActiveDutyCyclePercentage(actualDutyCyclePercentage);
	}
}

void Microsoft::Maker::Motor::OnRpmUpdated(Microsoft::Maker::Encoder ^sender, int32_t newRpm)
{
	if (this->Throttle >= 0)
		RpmUpdated(this, newRpm);
	else
		RpmUpdated(this, newRpm * -1);
}
