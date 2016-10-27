using System;
using System.Globalization;
using Windows.Devices.Gpio;
using Windows.UI;

namespace Bamboo.Models
{
    public class LEDStrip
    {
        private GpioPin _dataPin;
        private GpioPin _clockPin;
        private Color[] _ledData;
        private byte[] _latchBytes;

        public int Length { get; set; }

        public LEDStrip(int dataPin, int clockPin, int numLEDs)
        {
            Length = numLEDs;

            _ledData = new Color[numLEDs];
            _latchBytes = new byte[(numLEDs + 31) / 32];

            // Init all of the LEDs to off.
            for (int i = 0; i < _ledData.Length; i++)
            {
                _ledData[i] = Colors.Black;
            }

            // Init the latch bytes to 0
            _latchBytes.Initialize();

            // Set up the I/O pins
            var gpio = GpioController.GetDefault();
            if(gpio == null)
            {
                throw new Exception("Call to GpioController.GetDefault failed.");
            }

            _dataPin = gpio.OpenPin(dataPin);
            _dataPin.Write(GpioPinValue.Low);
            _dataPin.SetDriveMode(GpioPinDriveMode.Output);

            _clockPin = gpio.OpenPin(clockPin);
            _clockPin.Write(GpioPinValue.Low);
            _clockPin.SetDriveMode(GpioPinDriveMode.Output);

            Initialize();
        }

        public void Refresh()
        {
            // Update all of the LEDs
            for (int i = 0; i < _ledData.Length; i++)
            {
                // Data must be sent to the LED strip as GRB, not RGB
                // So for each 32 bit led value we have to move some
                // things around to send it properly
                sendByte((byte)(_ledData[i].G | 0x80));
                sendByte((byte)(_ledData[i].R | 0x80));
                sendByte((byte)(_ledData[i].B | 0x80));
            }

            // Send the latch bytes
            for (int i = 0; i < _latchBytes.Length; i++)
            {
                sendByte(_latchBytes[i]);
            }
        }

        public void SetLEDColor(int index, Color color)
        {
            _ledData[index] = color;
        }

        public void SetLEDColor(int index, byte r, byte g, byte b)
        {
            SetLEDColor(index, new Color { R = r, G = g, B = b });
        }

        public void SetLEDColor(int index, String cssColor)
        {
            byte r = byte.Parse(cssColor.Substring(1, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(cssColor.Substring(3, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(cssColor.Substring(5, 2), NumberStyles.HexNumber);

            Windows.UI.Color color = Color.FromArgb(0, r, g, b);
            SetLEDColor(index, color);
        }

        public void SetStripColor(Color color)
        {
            for (int i = 0; i < _ledData.Length; i++)
            {
                _ledData[i] = color;
            }
        }

        public void SetStripColor(byte r, byte g, byte b)
        {
            SetStripColor(new Color { R = r, G = g, B = b });
        }

        public void SetStripColor(String cssColor)
        {
            byte r = byte.Parse(cssColor.Substring(1, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(cssColor.Substring(3, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(cssColor.Substring(5, 2), NumberStyles.HexNumber);

            Windows.UI.Color color = Color.FromArgb(0, r, g, b);
            SetStripColor(color);
        }

        private void Initialize()
        {
            for (int i = 0; i < _latchBytes.Length * 8; i++)
            {
                _clockPin.Write(GpioPinValue.High);
                _clockPin.Write(GpioPinValue.Low);
            }
        }

        private void sendByte(byte b)
        {
            for (byte bit = 0x80; bit > 0; bit >>= 1)
            {
                if ((bit & b) > 0)
                    _dataPin.Write(GpioPinValue.High);
                else
                    _dataPin.Write(GpioPinValue.Low);

                _clockPin.Write(GpioPinValue.High);
                _clockPin.Write(GpioPinValue.Low);
            }
        }
    }
}
