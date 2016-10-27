using System.Threading.Tasks;

namespace Bamboo.Movement
{
    public interface IMovementManager
    {
        Task Dance();
        Task Initialize();
        Task LeftArmDown();
        Task LeftArmUp();
        Task MoveBackward(int count = 1);
        Task MoveForward(int count = 1);
        Task RightArmDown();
        Task RightArmUp();
        Task Stop();
        Task TurnLeft(int count = 1);
        Task TurnRight(int count = 1);
    }
}