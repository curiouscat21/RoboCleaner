using System;
using System.Threading;

namespace RoboCleaner
{
    public interface ICleaningStrategy
    {
        void Clean(Robot robot, Map map);
    }

    public class Map
    {
        private enum CellType { Empty, Dirt, Obstacle, Cleaned };
        private readonly CellType[,] _grid;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            _grid = new CellType[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _grid[x, y] = CellType.Empty;
        }

        public bool IsInBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
        public bool IsDirt(int x, int y) => _grid[x, y] == CellType.Dirt;
        public bool IsObstacle(int x, int y) => _grid[x, y] == CellType.Obstacle;
        public void AddDirt(int x, int y) => _grid[x, y] = CellType.Dirt;
        public void AddObstacle(int x, int y) => _grid[x, y] = CellType.Obstacle;

        public void Clean(int x, int y)
        {
            if (IsDirt(x, y))
                _grid[x, y] = CellType.Cleaned;
        }

        public void Display(int robotX, int robotY)
        {
            Console.Clear();
            Console.WriteLine("Vacuum cleaner robot simulation");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Legends: #=Obstacle, D=Dirt, C=Cleaned, R=Robot, .=Empty\n");

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (x == robotX && y == robotY)
                        Console.Write("R ");
                    else
                        Console.Write(_grid[x, y] switch
                        {
                            CellType.Empty => ". ",
                            CellType.Dirt => "D ",
                            CellType.Obstacle => "# ",
                            CellType.Cleaned => "C ",
                            _ => "? "
                        });
                }
                Console.WriteLine();
            }
        }
    }

    public class Robot
    {
        private readonly Map _map;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int SpeedSettings { get; private set; } = 150;
        public int Battery { get; private set; }
        public int MaxBattery { get; }
        private ICleaningStrategy _strategy = new SPatternStrategy();

        public Robot(Map map, int batteryCapacity = 200)
        {
            _map = map;
            Battery = batteryCapacity;
            MaxBattery = batteryCapacity;
        }

        public void AdjustSpeedSettings(int newSpeedSettings)
        {
            if (newSpeedSettings <= 0)
                throw new ArgumentOutOfRangeException(nameof(newSpeedSettings), "Speed must be greater than zero.");
            SpeedSettings = newSpeedSettings;
        }

        public void Recharge() => Battery = MaxBattery;
        public void SetStrategy(ICleaningStrategy newStrategy) => _strategy = newStrategy;

        public bool Move(int newX, int newY)
        {
            if (Battery <= 0) return false;
            if (!_map.IsInBounds(newX, newY)) return false;
            if (_map.IsObstacle(newX, newY)) return false;

            X = newX;
            Y = newY;
            _map.Display(X, Y);
            Battery--;
            return true;
        }

        public void CleanCurrentSpot()
        {
            if (!_map.IsDirt(X, Y)) return;
            _map.Clean(X, Y);
            _map.Display(X, Y);
        }

        public void StartCleaning() => _strategy.Clean(this, _map);
    }

    public class SPatternStrategy : ICleaningStrategy
    {
        public void Clean(Robot robot, Map map)
        {
            Console.WriteLine("Utilizing S-Pattern Strategy...");
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
                        direction *= -1;
                        y += direction;
                        continue;
                    }
                    robot.CleanCurrentSpot();
                    Thread.Sleep(150);
                }
                direction *= -1;
            }

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
            Console.WriteLine("Utilizing Random Path Strategy...");
            Random rand = new();

            int[][] directions =
            {
                new[] {1, 0},
                new[] {0, 1},
                new[] {-1, 0},
                new[] {0, -1}
            };

            while (robot.Battery > 0)
            {
                int[] dir = directions[rand.Next(directions.Length)];
                int newX = robot.X + dir[0];
                int newY = robot.Y + dir[1];

                if (!robot.Move(newX, newY)) continue;
                robot.CleanCurrentSpot();
                Thread.Sleep(150);
            }

            if (robot.Battery <= 0)
                Console.WriteLine("Cleaning stopped: Battery depleted.");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Map myMap = new(10, 10);
            myMap.AddDirt(2, 3);
            myMap.AddDirt(6, 2);
            myMap.AddDirt(3, 1);
            myMap.AddDirt(7, 9);
            myMap.AddObstacle(3, 2);
            myMap.AddObstacle(5, 5);

            Robot myRobot = new(myMap);
            myRobot.AdjustSpeedSettings(250);

            Random rand = new();
            int randomChoice = rand.Next(2);

            ICleaningStrategy strategy = randomChoice switch
            {
                0 => new SPatternStrategy(),
                1 => new RandomPathStrategy(),
                _ => new SPatternStrategy()
            };

            myRobot.SetStrategy(strategy);
            Console.WriteLine($"Selected Strategy: {strategy.GetType().Name}");
            myRobot.StartCleaning();
            Console.WriteLine("\nCleaning session finished.");
        }
    }
}
