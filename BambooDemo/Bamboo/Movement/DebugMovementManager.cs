using Bamboo.Movement;
using Bamboo.Utilities;
using System;
using System.Threading.Tasks;

namespace BroxtonDemo.Movement
{
    public class DebugMovementManager : IMovementManager
    {
        private Logger logger;

        public DebugMovementManager()
        {
            logger = Logger.GetInstance();
        }

        public Task Dance()
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine("Called Dance()");
            }));
        }

        public Task Initialize()
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine("Called Initialize()");
            }));
        }

        public Task LeftArmDown()
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine("Called LeftArmDown()");
            }));
        }

        public Task LeftArmUp()
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine("Called LeftArmUp()");
            }));
        }

        public Task MoveBackward(int count = 1)
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine($"Called MoveBackward({count})");
            }));
        }

        public Task MoveForward(int count = 1)
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine($"Called MoveForward({count})");
            }));
        }

        public Task RightArmDown()
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine("Called RightArmDown()");
            }));
        }

        public Task RightArmUp()
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine("Called RightArmUp()");
            }));
        }

        public Task Stop()
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine("Called Stop()");
            }));
        }

        public Task TurnLeft(int count = 1)
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine($"Called TurnLeft({count})");
            }));
        }

        public Task TurnRight(int count = 1)
        {
            return Task.Run(new Action(() =>
            {
                logger.LogLine($"Called TurnRight({count})");
            }));
        }
    }
}
