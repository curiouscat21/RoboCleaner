using System;

namespace VacuumCleanerRobot
{
    public interface ICleaningStrategy
    {
        void Clean(Robot robot, Map map);
    }

    public class SPatternStrategy : ICleaningStrategy
    {
        public void Clean(Robot robot, Map map)
        {
            // Utilizing an S-Pattern Strategy...
            int direction = 1;

            for (int x = 0; x < map.Width; x++)
            {
                if (robot.Battery <= 0) break;

                int startY = (direction == 1) ? 0 : map.Height - 1;
                int endY = (direction == 1) ? map.Height : -1;

                for (int y = startY; y != endY; y += direction)
                {
                    if (robot.Battery <= 0) break;

                    if (!map.IsInBounds(x, y))
                        break;

                    if (!robot.Move(x, y))
                    {
                        // obstacle detected, change direction
                        direction *= -1;
                        y += direction;
                        continue;
                    }

                    robot.CleanCurrentSpot();
                    Thread.Sleep(150);
                }
                direction *= -1; // reverse direction for the next column
            }

            // Outcome message
            if (robot.Battery <= 0)
                Console.WriteLine("Cleaning stopped: Battery depleted.");
            else
                Console.WriteLine("Cleaning completed: S-Pattern Strategy finished.");
        }
    }

    public class RandomPathStrategy : ICleaningStrategy
    {
        public void Clean(Robot robot, Map map)
        {
            // Utilizing a Random Path Strategy... 
            Random rand = new();
            int[][] directions = [
                [1, 0],
                [ 0, 1 ],
                [ -1, 0 ],
                [ 0, -1 ]
            ];

            while (robot.Battery > 0)
            {
                int[] direction = directions[rand.Next(directions.Length)];

                int x = robot.X + direction[0];
                int y = robot.Y + direction[1];

                if (!robot.Move(x, y)) continue;

                robot.CleanCurrentSpot();
                Thread.Sleep(150);
            }

            // Outcome message
            if (robot.Battery <= 0) Console.WriteLine("Cleaning stopped: Battery depleted.");
        }
    }
    public class Map
    {
        private enum CellType { Empty, Dirt, Obstacle, Cleaned };
        private readonly CellType[,] _grid;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Map(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            _grid = new CellType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = CellType.Empty;
                }
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < this.Width && y >= 0 && y < this.Height;
        }

        public bool IsDirt(int x, int y)
        {
            return this._grid[x, y] == CellType.Dirt;
        }

        public bool IsObstacle(int x, int y)
        {
            return this._grid[x, y] == CellType.Obstacle;
        }

        public void AddDirt(int x, int y)
        {
            this._grid[x, y] = CellType.Dirt;
        }

        public void AddObstacle(int x, int y)
        {
            this._grid[x, y] = CellType.Obstacle;
        }

        public void Clean(int x, int y)
        {
            if (IsDirt(x, y))
            {
                this._grid[x, y] = CellType.Cleaned;
            }
        }

        public void Display(int robotX, int robotY)
        {
            Console.Clear();
            Console.WriteLine("Vacuum cleaner robot simulation");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Legends: #=Obstacles, D=Dirt, .=Empty, R=Robot, C=Cleaned");

            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    if (x == robotX && y == robotY)
                    {
                        Console.Write("R ");
                    }
                    else
                    {
                        switch (this._grid[x, y])
                        {
                            case CellType.Empty: Console.Write(". "); break;
                            case CellType.Dirt: Console.Write("D "); break;
                            case CellType.Obstacle: Console.Write("# "); break;
                            case CellType.Cleaned: Console.Write("C "); break;
                        }
                    }
                }
                Console.WriteLine();
            }
        }
    }

    public class Robot(Map map, int batteryCapacity = 200)
    {

        private readonly Map _map = map;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        public int SpeedSettings { get; private set; } = 150;

        public int Battery { get; private set; } = batteryCapacity;

        public int MaxBattery { get; } = batteryCapacity;

        private ICleaningStrategy _strategy = new SPatternStrategy();

        public void AdjustSpeedSettings(int newSpeedSettings)
        {
            if (newSpeedSettings <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(newSpeedSettings),
                    "Speed settings must be greater than zero."
                );
            }

            this.SpeedSettings = newSpeedSettings;
        }

        public void Recharge()
        {
            this.Battery = this.MaxBattery;
        }

        public void SetStrategy(ICleaningStrategy newStrategy)
        {
            this._strategy = newStrategy;
        }
        public bool Move(int newX, int newY)
        {
            if (this.Battery <= 0)
                return false;

            if (!this._map.IsInBounds(newX, newY))
                return false;

            if (this._map.IsObstacle(newX, newY))
                return false;

            // set new location
            this.X = newX;
            this.Y = newY;

            // display the map with the robot's new location
            this._map.Display(this.X, this.Y);
            this.Battery--;
            return true;
        }

        public void CleanCurrentSpot()
        {
            if (!this._map.IsDirt(this.X, this.Y)) return;

            this._map.Clean(this.X, this.Y);
            this._map.Display(this.X, this.Y);
        }

        public void StartCleaning()
        {
            this._strategy.Clean(this, this._map);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            // Room Map
            Map myMap = new(10, 10);

            // Dirts
            myMap.AddDirt(2,3);
            myMap.AddDirt(6, 2);
            myMap.AddDirt(3, 1);
            myMap.AddDirt(7, 9);

            // Obstacles
            myMap.AddObstacle(3, 2);
            myMap.AddObstacle(3, 2);

            // Cleaning Robot
            Robot myRobot = new(myMap);
            myRobot.AdjustSpeedSettings(250);

            // Cleaning Strategies
            SPatternStrategy sPatternStrategy = new();
            RandomPathStrategy randomPathStrategy = new();
            myRobot.SetStrategy(randomPathStrategy);

            myRobot.StartCleaning();
        }
    }
}