namespace BattleshipsTcpServer;

public static class Extensions
{
    private static readonly Random RandomGenerator = new Random();
    
    public static (int, int) SumTuple((int, int) first, (int, int) second)
    {
        return (first.Item1 + second.Item1, first.Item2 + second.Item2);
    }

    public static (int, int) GetRandomTuple(List<(int, int)> tuples) => tuples[RandomGenerator.Next(tuples.Count)];

    public static Direction GetRandomDirection() => (Direction) RandomGenerator.Next(Enum.GetNames(typeof(Direction)).Length);
}



public static class EnumExtensions
{
    public static int ToInt(this Ship val) => Convert.ToInt32(val);

    public static (int, int) GetOffset(this Direction direction)
    {
        return direction switch
        {
            Direction.North => (-1, 0),
            Direction.East => (0, 1),
            Direction.South => (1, 0),
            Direction.West => (0, -1),
            _ => (0, 0)
        };
    }
}