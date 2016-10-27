#pragma once
#include <mutex>
#include <windows.devices.gpio.h>

using namespace Windows::Devices::Gpio;
using namespace Windows::System::Threading;

#define DEGREES_PER_REVOLUTION	360.00

// This value is the alpha variable for the exponential moving average
// calculation used for smoothing the RPM value.
#define EMA_ALPHA	0.8

#define UPDATE_INTERVAL_MS	50	// Update the RPM every 50ms

namespace Microsoft {
namespace Maker {
	ref class Encoder;
	public delegate void RpmUpdatedEventHandler(Encoder^ sender, int32_t newRpm);

	public ref class Encoder sealed
	{
	private:
		LARGE_INTEGER _lastEventTime;
		uint32_t _currentRPM;
		double _degreesPerPulse;
		LARGE_INTEGER _hiResolutionClockFrequency;
		bool _isFirstInterrupt;
		GpioPin^ _pin;
		std::mutex rpmMutex;
		LONGLONG _pulseCount;
		LONGLONG _runningPulseCount;
		ThreadPoolTimer^ updateTimer;
		void CalculateRPM(Windows::System::Threading::ThreadPoolTimer^ timer);

	public:
		Encoder(int pin, double pulsesPerRevolution);
		virtual ~Encoder();
		uint32_t GetCurrentRPM(void);
		LONGLONG GetPulseCount(void);
		event RpmUpdatedEventHandler^ RpmUpdated;
	};

} // end Maker namespace
} // end Microsoft namespace

