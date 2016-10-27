// This file has been generated in EZ-Builder using the Auto Position SDK Code Creator.

using EZ_B;
using System.Threading.Tasks;

namespace EZ_ROBOT_AUTO_POSITION
{

    public class AutoPositions
    {
        EZ_B.EZB _ezb;

        public AutoPositions(EZ_B.EZB ezb)
        {
            _ezb = ezb;
        }

        public async Task StartAction_Dance()
        {
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D0, 60);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D1, 120);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D2, 60);

            await Task.Delay(1000);

            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D0, 120);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D1, 60);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D2, 120);

            await Task.Delay(1000);

            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D0, 180);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D1, 120);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D2, 60);

            await Task.Delay(1000);

            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D0, 120);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D1, 60);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D2, 120);

            await Task.Delay(1000);
        }

        public async Task StartAction_Stand()
        {

            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D0, 120);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D1, 60);
            await Task.Delay(1000);
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D2, 120);
            await Task.Delay(1000);
        }

        public async Task StartAction_LeftArmUp()
        {
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D1, 60);
        }

        public async Task StartAction_LeftArmDown()
        {
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D2, 120);
        }

        public async Task StartAction_RightArmUp()
        {
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D1, 120);
        }

        public async Task StartAction_RightArmDown()
        {
            await _ezb.Servo.SetServoPosition(Servo.ServoPortEnum.D1, 60);
        }
    }
}
