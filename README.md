# Bamboo - Windows 10 IoT Core with Intel&reg; Joule&trade;

Bamboo is a prototype robot panda built around the Intel&reg; Joule&trade; with Windows 10 IoT Core. 

Bamboo has the following abilities: 

  * Moves via closed loop control and easy to access busses – GPIO and PWM using off the shelf motor controllers, motors, encoders and libraries.
  * Sees and processes the world using depth information from an Intel&reg; RealSense&trade; camera over USB 3.0.
  * Listens via command and control. While Bamboo understands English, you can speak to her in any language, which is translated via Microsoft Translation services into her native English.
  * Speaks via attached off-the-shelf USB speakers. 


**NOTE: Running the code in this repository on a device with Windows 10 IoT Core requires an Intel driver that is not yet publicly available. This only affects the camera and mapping portions of the demo. This notice will be removed as soon as that driver is released.**

## Get the Code
This repository contains references to submodules. Be sure to use the --recursive flag when cloning.

```git clone --recursive https://github.com/ms-iot/bamboo-demo.git```

The code is comprised of several projects which are explained in more detail below.

  * [Bamboo](#bamboo)
  * [Motor](#motor)
  * [PidController](#pidcontroller)
  * [PwmPCA9685](#pwmpca9685)
  * [UniversalMediaEngine](#universalmediaengine)

## Software Setup
If you wish to recreate this project or even just run certain pieces of the code you will need to configure several services and provide some information about the mechanical parts you have chosen to use if different from those listed in the part list.

### Translation Service
You will need to setup a subscription with Microsoft Translator for the multilingual command and control feature of Bamboo to work. [Click here to get started.](https://www.microsoft.com/en-us/translator/default.aspx)

[Speech Translation API Documentation](https://docs.microsofttranslator.com/speech-translate.html)

Once you have set up the translation account you need to enter your client ID and secret token into [Secrets.cs](BambooDemo/Bamboo/Secrets.cs).
```cs
// ID and Secret for translation service
public const string AzureDataMarketClientId = "";
public const string AzureDataMarketClientSecret = "";
```

### Configuration Constants
All of the configuration constants for Bamboo are located in [Configuration.cs](BambooDemo/Bamboo/Configuration.cs). If you build your own platform you will need to change the values in this file for things like wheel diameter, axle length and motor gearbox ratio among other things.

## Hardware and Electrical Setup

MORE INFO COMING SOON HERE

**NOTE: Because of how the ```AudioGraph``` API works the speech and translation functionality will not initialize unless an audio output device is connected to the Intel&reg; Joule&trade;**

### Part List
| Part                                         | Quantity | Notes                                                                                                                                                                |
|----------------------------------------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Intel&reg; Joule&trade;                      | 1        | Available from [Mouser](http://www.mouser.com/new/Intel/intel-joule/) and [Newegg](http://www.newegg.com/Product/Product.aspx?Item=N82E16813121832)                  |
| Intel&reg; RealSense&trade; SR300 camera     | 1        |                                                                                                                                                                      |
| 10 Series 2 Hole Inside Corner Gusset        | 20       | [https://8020.net/4132.html](https://8020.net/4132.html)                                                                                                             |
| 1" x 1" T-Slotted Extrusion                  | 6 ft     | [https://8020.net/1010.html](https://8020.net/1010.html)                                                                                                             |
| 1/4-20 x 1/2" BHSCS, ECON T-NUT              | 40       | [https://8020.net/3393.html](https://8020.net/3393.html)                                                                                                             |
| Perforated Polycarbonate Sheet               | 1        | [http://www.andymark.com/product-p/am-2170.htm](http://www.andymark.com/product-p/am-2170.htm)                                                                       |
| Victor SP Speed Controller                   | 2        | [http://www.andymark.com/Victor-SP-p/am-2855.htm](http://www.andymark.com/Victor-SP-p/am-2855.htm)                                                                   |
| Pololu 6VDC 210 RPM Gearmotor w/ Encoder     | 2        | https://www.pololu.com/product/2274                                                                                                                                  |
| 37D mm Metal Gearmotor Bracket (Pair)        | 1        | [https://www.pololu.com/product/1084](https://www.pololu.com/product/1084)                                                                                           |
| Universal Aluminum 4mm Mounting Hub (2-pack) | 1        | [https://www.pololu.com/product/1081](https://www.pololu.com/product/1081)                                                                                           |
| 90 x 10mm Wheel (Pair)                       | 1        | [https://www.pololu.com/product/1435](https://www.pololu.com/product/1435)                                                                                           |
| Ball Caster with 3/4" Metal Ball             | 2        | [https://www.pololu.com/product/955](https://www.pololu.com/product/955)                                                                                             |
| Machine Screw #4-40 1/2" (Pack of 25)        | 1        | [https://www.pololu.com/product/1962](https://www.pololu.com/product/1962)                                                                                           |
| Machine Hex Nut #4-40 (Pack of 25)           | 1        | [https://www.pololu.com/product/1068](https://www.pololu.com/product/1068)                                                                                           |
| 8 Gang Fuse Block                            | 1        | [http://www.andymark.com/product-p/am-3136.htm](http://www.andymark.com/product-p/am-3136.htm)                                                                       |
| Fuse Holder w/ 10A Fuse                      | 1        | [http://www.robotshop.com/en/fuse-holder-10a-fuse.html](http://www.robotshop.com/en/fuse-holder-10a-fuse.html)                                                       |
| 3A Fuse                                      | 6        | Can be obtained at any automotive store                                                                                                                              |
| UBEC DC/DC Buck Converter                    | 1        | [https://www.adafruit.com/products/1385](https://www.adafruit.com/products/1385)                                                                                     |
| USB Speaker                                  | 1        | We used the [iLuv](https://www.amazon.com/iLuv-Compact-USB-powered-speakers-laptop-Silver/dp/B006EF689M) but any USB speaker should work.                            |
| RGB LED Strip                                | 1        | We used the [weatherproof strip](https://www.adafruit.com/products/1948) from Adafruit.                                                                              |
| USB 3.0 Hub                                  | 1        | We used a [Targus 3 port hub](https://www.amazon.com/Targus-USB-3-port-Ethernet-AH122USZ/dp/B00DFBDQ2K).                                                             |
## Windows IoT Remote Experience
While Bamboo works great without an attached screen you may want to see the camera view or look at the 3D map that is being generated. If you have access to the HDMI port on the Intel&reg; Joule&trade; you can connect a display there but another option is to use the [Windows IoT Remote Client](https://www.microsoft.com/en-us/store/p/windows-iot-remote-client/9nblggh5mnxz) which allows you to have complete remote control of Bamboo's UI from any Windows 10 device.

## Bamboo
This project contains the main UI for Bamboo as well as things like the reminder manager, odometry, drivetrain code and camera depth processing logic.

### Drivetrain Class
This class coordinates the actions of both motors to create directional movement like forward, reverse, and turning. It uses PID control to smoothly drive each motor to specific RPM values.

#### Constructor
```cs
Drivetrain drivetrain = new Drivetrain();
```

#### Methods
| Method                                  | Description                                                       |
|-----------------------------------------|-------------------------------------------------------------------|
| Forward(double distanceInMeters)        | Drive the robot forward ```distanceInMeters```.                   |
| Initialize()                            | Initialize the motors and start the PID loop.                     |
| Reverse(double distanceInMeters)        | Drive the robot backward ```distanceInMeters```.                  |
| SetPIDValues(float p, float i, float d) | Change the PID loop proportional, integral, and derivative gains. |
| Stop()                                  | Set the target RPM for both motors to 0.                          |
| TurnLeft(double degrees)                | Turn the robot left by the specified number of degrees.           |
| TurnRight(double degrees)               | Turn the robot right by the specified number of degrees.          |

**Note:** Don't forget to set the drivetrain-related constants in [Configuration.cs](BambooDemo/Bamboo/Configuration.cs).

### LEDStrip Class
The LED strip used in this project is NOT a NeoPixel device. It is an [RGB LED Weatherproof Strip](https://www.adafruit.com/products/1948) from Adafruit and relies on shift registers to individually address each LED. We created a class to simplify updating each LED on the strip or all of them at once. Because the LEDs are updated via cascading shift registers the entire strip must be updated at once. So while it is possible to change each LED individually an entire strip refresh is needed and is accomplished by calling the ```Refresh()``` method after making one or more set calls.

#### Constructor
```cs
LEDStrip ledStrip = new LEDStrip(int dataPin, int clockPin, int numLEDs);
```

  * ```dataPin``` - The pin to which the strip data line is connected
  * ```clockPin``` - The pin to which the strip clock line is connected
  * ```numLEDs``` - The number of LEDs on the strip

#### Methods
| Method                                              | Description                                                                              |
|-----------------------------------------------------|------------------------------------------------------------------------------------------|
| Refresh()                                           | Update the entire strip with any pending changes.                                        |
| SetLEDColor(int ledIndex, Color c)                  | Set the LED at ```ledIndex``` to ```c```.                                                |
| SetLEDColor(int ledIndex, byte r, byte g, byte b)   | Set the LED at ```ledIndex``` to the RGB color defined by ```r```, ```g```, and ```b```. |
| SetLEDColor(int ledIndex, String cssColor)          | Set the LED at ```ledIndex``` to ```cssColor``` which is in the form #000000.            |
| SetStripColor(int ledIndex, Color c)                | Set every LED to ```c```.                                                                |
| SetStripColor(int ledIndex, byte r, byte g, byte b) | Set every LED to the RGB color defined by ```r```, ```g```, and ```b```.                 |
| SetStripColor(int ledIndex, String cssColor)        | Set every LED to ```cssColor``` which is in the form #000000.                            |

#### Properties
| Property | Type      | Access  | Description                      |
|----------|-----------|---------|----------------------------------|
| Length   | ```int``` | get/set | The number of LEDs on the strip. |

### MovementManager Class
This class manages the servos and drivetrain. It uses the ```Drivetrain``` class to interact with the physical platform.

#### Constructor
```cs
MovementManager manager = new MovementManager();
```

#### Methods
| Method                  | Description                                   |
|-------------------------|-----------------------------------------------|
| Dance()                 | Perform the pre-programmed dance routine.     |
| Initialize()            | Initialize the drivetrain |
| LeftArmDown()           | Move left arm to the resting position.        |
| LeftArmUp()             | Move left arm to 90 degrees.                  |
| MoveBackward(int count) | Move backward ```count``` meters.             |
| MoveForward(int count)  | Move forward ```count``` meters.              |
| RightArmDown()          | Move right arm to the resting position.       |
| RightArmUp()            | Move right arm to 90 degrees.                 |
| Stop()                  | Stop the drivetrain.                          |
| TurnLeft(int count)     | Rotate left 90 degrees ```count``` times.     |
| TurnRight(int count)    | Rotate right 90 degrees ```count``` times.    |

#### Properties
| Property   | Type             | Access  | Description                                                             |
|------------|------------------|---------|-------------------------------------------------------------------------|
| DriveTrain | ```Drivetrain``` | get/set | The ```Drivetrain``` object responsible for moving the platform around. |

### Odometer Class
The SLAM algorithm in this project requires accurate position and angle information from the drivetrain. Because Bamboo doesn't have an [IMU](https://en.wikipedia.org/wiki/Inertial_measurement_unit) or GPS unit we used a common dead reckoning algorithm to determine coordinate position and angle from only the encoder output. This singleton class could be used in any robotic project that has motors with rotary encoder output. **Note:** Don't forget to set the drivetrain-related constants in [Configuration.cs](BambooDemo/Bamboo/Configuration.cs). Otherwise this class will not generate accurate odometry.

#### Constructor
The Odometer is a global singleton that is accessed directly as ```Odometer.Instance```.

#### Events
| Event                    | Description                                                                                                                   |
|--------------------------|-------------------------------------------------------------------------------------------------------------------------------|
| PositionChanged          | Notifies when new position or angle information is available.                                                                 |
| ThresholdAngleReached    | Notifies when the current angle is within a small tolerance of the target angle set through ```SetTrackingThreshold```.       |
| ThresholdPositionReached | Notifies when the current position is within a small tolerance of the target position set through ```SetTrackingThreshold```. |

#### Methods
| Method                                                                | Description                                                          |
|-----------------------------------------------------------------------|----------------------------------------------------------------------|
| Reset()                                                               | Set the current position to (0,0) and angle to 0 degrees.            |
| SetTrackingThreshold(double x, double y, double theta)                | Set a threshold that, when reached, will fire an event.              |
| Update(int leftRPM, long leftPulses, int right RPM, long rightPulses) | Update the internal odometry data using information from the motors. |

#### Properties
| Property   | Type             | Access  | Description         |
|------------|------------------|---------|---------------------|
| Instance   | ```Odometer```   | get     | Odometer singleton. |


### SpeechManager Class
This class handles Bamboo's voice command interface. You can change Bamboo's name by simply changing the ```ROBOT_NAME``` constant in [Configuration.cs](BambooDemo/Bamboo/Configuration.cs).

#### Example
```cs
// Listen for "Bamboo Forward"
SpeechManager manager = new SpeechManager();
await manager.Initialize();
manager.AddCommand("Forward", async () =>
{
	// add logic here
});

```

#### Constructor
```cs
SpeechManager manager = new SpeechManager();
```

#### Events
| Event                 | Description                                                                                                                    |
|-----------------------|--------------------------------------------------------------------------------------------------------------------------------|
| ListenStateChanged    | Notifies when the listen state changes. Valid states are ```Initializing```, ```NotListening```, ```Listening```, ```Error```. |
| SourceLanguageChanged | Notifies when the current source language has changed.                                                                         |

#### Methods
| Method                                                    | Description                                                                  |
|-----------------------------------------------------------|------------------------------------------------------------------------------|
| AddCommand(String name, commandExecutionRoutine callback) | Add a new voice command ```name``` that executes ```callback``` when spoken. |
| Initialize()                                              | Initialize the speech manager and begin listening for the command keyword.   |

#### Properties
| Property       | Type              | Access  | Description                                                 |
|----------------|-------------------|---------|-------------------------------------------------------------|
| ListenState    | ```ListenState``` | get/set | The current ```ListenState``` of the manager.               |
| SourceLanguage | ```Language```    | get/set | The language in which the manager will listen for commands. |

## Motor
This project contains the logic for getting the RPM of the motor shaft and controlling the motor throttle. Throttle is controlled using a PWM-controlled motor controller which is attached to an Adafruit PWM Servo Hat. The RPM value is calculated using an optical encoder and the high speed interrupt capabilities of Windows 10 IoT Core. Knowing the pulses per revolution value of the encoder along with the elapsed time between interrupts we can easily calculate the RPM. We used an [exponential moving average](https://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average) to smooth out the noise.

### Example
```cs
Motor motor = new Motor(0, 36, 250);
await motor.Initialize();
motor.Throttle = 50.0; // Set the throttle to 50%
var rpm = motor.RPM;   // Get the current RPM
```

### Constructor
```cs
Motor motor = new Motor(int motorPin, int encoderPin, int encoderPPR);
```

  * ```motorPin``` - The PWM channel on the HAT to which the motor is connected
  * ```encoderPin``` - The pin to which the optical encoder channel is connected
  * ```encoderPPR``` - The pulses per revolution value of the encoder used (found in the datasheet)

### Events
| Event        | Description                                                                               |
|--------------|-------------------------------------------------------------------------------------------|
| RpmUpdated   | Notifies that the RPM member has been updated. Does not imply that the value has changed. |

### Methods
| Method       | Description                                                                                                |
|--------------|------------------------------------------------------------------------------------------------------------|
| Initialize() | Initializes the motor. Must be called before any other operations are performed on the ```Motor``` object. |

### Properties
| Property      | Type         | Access  | Description                                                                                                                          |
|---------------|--------------|---------|--------------------------------------------------------------------------------------------------------------------------------------|
| Throttle      | ```double``` | get/set | The throttle of the motor as a percentage between -100.0 and 100.0. -100.0 is full reverse, 0 is stopped, and 100.0 is full forward. |
| RPM           | ```int```    | get     | The current RPM of the motor. Updated every 50ms by default.                                                                         |
| EncoderPulses | ```long```   | get     | The current count of raw encoder pulses.                                                                                             |

## PidController
This project contains the PID logic for achieving closed loop control of the motor RPM and is documented [here](https://github.com/ms-iot/pid-controller). The PID loop needs to be tuned to achieve smooth motor control. The gain constants are in [Configuration.cs](BambooDemo/Bamboo/Configuration.cs) and may need to be modified given your motor and gearbox choices.

```cs
// The PID gain constants were derived from trial and error tuning
public const float PROPORTIONAL_GAIN = 0.1f;
public const float INTEGRAL_GAIN = 0.15f;
public const float DERIVATIVE_GAIN = 0f;
```

## PwmPCA9685
This project was copied from the [BusProviders repository](https://github.com/ms-iot/BusProviders) and it allows us to use the Adafruit PWM Servo HAT to communicate with the Victor SP motor controllers. The controllers require a PWM input signal which is delivered by the Servo HAT. The HAT is connected to the Intel&reg; Joule&trade; via I2C and creates a nice abstraction for communicating with multiple motor controllers without having to drive the PWM signal directly. 

## UniversalMediaEngine
This is a library that simplifies the playing of media in Windows 10 IoT Core and is documented [here](https://github.com/bethoma/UniversalMediaEngine).

===

This project has adopted the [Microsoft Open Source Code of Conduct](http://microsoft.github.io/codeofconduct). For more information see the [Code of Conduct FAQ](http://microsoft.github.io/codeofconduct/faq.md) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments. 