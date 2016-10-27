using Bamboo.Models;
using Bamboo.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Bamboo.Movement
{
    public class EZBMovementManager : IMovementManager
    {
        private DispatcherTimer ezbHeartbeat;

        private static readonly EZ_B.Servo.ServoPortEnum PORT_HEAD_HORIZONTAL = EZ_B.Servo.ServoPortEnum.D0;
        private static readonly EZ_B.Servo.ServoPortEnum PORT_RIGHT_CHEST = EZ_B.Servo.ServoPortEnum.D1;
        private static readonly EZ_B.Servo.ServoPortEnum PORT_LEFT_CHEST = EZ_B.Servo.ServoPortEnum.D2;

        private EZ_B.EZB ezbController;
        private EZ_ROBOT_AUTO_POSITION.AutoPositions ezbPresetActions;

        private static readonly EZ_B.Servo.ServoPortEnum[] ATTACHED_SERVOS_LIST = {
            PORT_HEAD_HORIZONTAL,
            PORT_RIGHT_CHEST,
            PORT_LEFT_CHEST,
        };

        public Drivetrain DriveTrain { get; set; } = new Drivetrain();

        public async Task Initialize()
        {
            //init ezb
            ezbController = new EZ_B.EZB("Demo Controller");
            await initializeEZB();

            //init heartbeat for ezb
            ezbHeartbeat = new DispatcherTimer();
            ezbHeartbeat.Tick += ezbHeartbeat_Tick;
            ezbHeartbeat.Interval = new TimeSpan(0, 0, 5);

            //init drive train
            await DriveTrain.Initialize();
        }

        public async Task MoveForward(int count = 1)
        {
            await DriveTrain.Forward((double)count/2);
        }

        public async Task MoveBackward(int count = 1)
        {
            await DriveTrain.Reverse((double)count/2);
        }

        public async Task TurnRight(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                await DriveTrain.TurnRight(90.0);
            }
        }

        public async Task TurnLeft(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                await DriveTrain.TurnLeft(90.0);
            }
        }

        public async Task Stop()
        {
            await DriveTrain.Stop();
        }

        public async Task LeftArmUp()
        {
            await ezbPresetActions.StartAction_LeftArmUp();
        }

        public async Task LeftArmDown()
        {
            await ezbPresetActions.StartAction_LeftArmDown();
        }

        public async Task RightArmUp()
        {
            await ezbPresetActions.StartAction_RightArmUp();
        }

        public async Task RightArmDown()
        {
            await ezbPresetActions.StartAction_RightArmDown();
        }

        public async Task Dance()
        {
            await ezbPresetActions.StartAction_Dance();
        }

        private async Task initializeEZB()
        {
            try
            {
                if (ezbController.IsConnected)
                {
                    ezbController.Disconnect();
                }

                var serialPorts = await EZ_B.SerialClient.GetSerialPortList();

                var portToUse =
                    (
                        from port in serialPorts
                        where port.Id.Contains("UART1")
                        select port
                    ).FirstOrDefault();

                if (null != portToUse)
                {
                    await ezbController.Connect(portToUse);
                }
                else
                {
                    Logger.GetInstance().LogLine("Unable to find the serial port for the EZ-B");
                }

                ezbPresetActions = new EZ_ROBOT_AUTO_POSITION.AutoPositions(ezbController);
                await ezbPresetActions.StartAction_Stand();

                foreach (var servo in ATTACHED_SERVOS_LIST)
                {
                    await Task.Delay(250);
                    await ezbController.Servo.SetServoSpeed(servo, 2);
                }
            }
            catch (Exception ex)
            {
                Logger.GetInstance().LogLine(ex.Message);
            }
        }

        private async void ezbHeartbeat_Tick(object sender, object e)
        {
            if (!(await ezbController.PingController()) || !ezbController.IsConnected)
            {
                ezbHeartbeat.Stop();
                await initializeEZB();
                ezbHeartbeat.Start();
            }
        }
    }
}
