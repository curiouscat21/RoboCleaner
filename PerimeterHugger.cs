using System;
using System.Threading;

namespace RoboCleaner
{
    public class PerimeterHuggerStrategy : ICleaningStrategy
    {
        public void Clean(Robot robot, Map map)
        {
            Console.WriteLine("Starting Perimeter Hugger Strategy...");

            // Move Right
            while (robot.Move(robot.X + 1, robot.Y))
            {
                robot.CleanCurrentSpot();
                Thread.Sleep(150);
            }

            // Move Down
            while (robot.Move(robot.X, robot.Y + 1))
            {
                robot.CleanCurrentSpot();
                Thread.Sleep(150);
            }

            // Move Left
            while (robot.Move(robot.X - 1, robot.Y))
            {
                robot.CleanCurrentSpot();
                Thread.Sleep(150);
            }

            // Move Up
            while (robot.Move(robot.X, robot.Y - 1))
            {
                robot.CleanCurrentSpot();
                Thread.Sleep(150);
            }

            Console.WriteLine("Perimeter cleaning complete.");
        }
    }
}
