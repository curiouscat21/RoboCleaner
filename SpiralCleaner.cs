using System;
using System.Threading;

namespace VacuumCleanerRobot
{
    public class SpiralStrategy : ICleaningStrategy
    {
        public void Clean(Robot robot, Map map)
        {
            Console.WriteLine("Starting Spiral Cleaning Strategy...");

            int[,] directions =
            {
                {1, 0},  // Right
                {0, 1},  // Down
                {-1, 0}, // Left
                {0, -1}  // Up
            };

            int dirIndex = 0;        
            int segmentLength = 1;   
            int turns = 0;

            while (robot.Battery > 0)
            {
                for (int i = 0; i < segmentLength; i++)
                {
                    int newX = robot.X + directions[dirIndex, 0];
                    int newY = robot.Y + directions[dirIndex, 1];

                    if (!robot.Move(newX, newY))
                    {
                        Console.WriteLine("Hit boundary or obstacle, stopping spiral.");
                        return;
                    }

                    robot.CleanCurrentSpot();
                    Thread.Sleep(150);
                }

                dirIndex = (dirIndex + 1) % 4;
                turns++;

                if (turns % 2 == 0)
                    segmentLength++;
            }

            Console.WriteLine("Spiral cleaning complete or battery depleted.");
        }
    }
}
