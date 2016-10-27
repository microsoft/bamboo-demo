#include "pch.h"
#include "Encoder.h"

using namespace Microsoft::Maker;
using namespace Platform;
using namespace std::placeholders;

Microsoft::Maker::Encoder::Encoder(int pin, double pulsesPerRevolution) : _currentRPM(0), _isFirstInterrupt(true), _pin(nullptr), _pulseCount(0), _runningPulseCount(0)
{
	_degreesPerPulse = DEGREES_PER_REVOLUTION / pulsesPerRevolution;

	// The high resolution clock frequency is fixed at system boot so we only
	// need to query this value once
	QueryPerformanceFrequency(&_hiResolutionClockFrequency);

	// Set up the interrupt pin
	GpioController^ controller = GpioController::GetDefault();
	_pin = controller->OpenPin(pin);
	_pin->SetDriveMode(Windows::Devices::Gpio::GpioPinDriveMode::Input);

	// Update RPM every 50ms
	Windows::Foundation::TimeSpan tickInterval;
	tickInterval.Duration = 10000 * UPDATE_INTERVAL_MS;
	updateTimer = ThreadPoolTimer::CreatePeriodicTimer(ref new TimerElapsedHandler(this, &Encoder::CalculateRPM), tickInterval);
}

Microsoft::Maker::Encoder::~Encoder()
{
	_pin->StopInterruptCount();
	updateTimer->Cancel();
}

uint32_t Microsoft::Maker::Encoder::GetCurrentRPM(void)
{
	return _currentRPM;
}

LONGLONG Microsoft::Maker::Encoder::GetPulseCount(void) 
{
	LONGLONG currentCount = _runningPulseCount;
	_runningPulseCount = 0;
	return currentCount;
}

void Encoder::CalculateRPM(Windows::System::Threading::ThreadPoolTimer^ timer)
{
	rpmMutex.lock();
	if (_isFirstInterrupt) {
		QueryPerformanceCounter(&_lastEventTime);
		_isFirstInterrupt = false;
		_pin->StartInterruptCount(); // Count rising and falling edges
	}
	else
	{
		LARGE_INTEGER currentEventTime;
		QueryPerformanceCounter(&currentEventTime);

		// How far has the shaft traveled since our last query
		_pulseCount = _pin->InterruptCount;
		_runningPulseCount += _pulseCount;
		double degreesSinceLastInterrupt = _degreesPerPulse * _pulseCount;
		
		// Start counting interrupts again
		_pin->StartInterruptCount();

		// Convert the elapsed time to microseconds. For more info on the high-resolution clock see
		// https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx
		LARGE_INTEGER elapsedTime;
		elapsedTime.QuadPart = currentEventTime.QuadPart - _lastEventTime.QuadPart;
		elapsedTime.QuadPart *= 1000000;
		elapsedTime.QuadPart /= _hiResolutionClockFrequency.QuadPart; // Get elapsed time in microseconds

		// Calculate the RPM
		// degreesSinceLastInterrupt      1 revolution       60000000 us
		// -------------------------  x  --------------  x  -------------  =  RPM Value
		//     elapsedTime in us           360 degrees          1 min
		uint32_t  newValue = static_cast<uint32_t>((degreesSinceLastInterrupt * 60000000.00) / (DEGREES_PER_REVOLUTION * elapsedTime.QuadPart));

		// Smooth the RPM output using an exponential moving average.
		// https://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average
		_currentRPM = static_cast<uint32_t>(EMA_ALPHA*newValue + (1.0 - EMA_ALPHA)*_currentRPM);
		RpmUpdated(this, _currentRPM);

		_lastEventTime.QuadPart = currentEventTime.QuadPart;
	}
	rpmMutex.unlock();
}
