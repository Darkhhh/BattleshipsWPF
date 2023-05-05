namespace Battleships;

public static class Position
{
    private static readonly Dictionary<char, int> ColumnValues = new ()
    {
        {'A', 1}, {'B', 2}, {'C', 3}, {'D', 4}, {'E', 5}, {'F', 6}, {'G', 7}, {'H', 8}, {'I', 9}, {'J', 10}
    };

    public static (int, int) Raw(char column, int row) => (ColumnValues[char.ToUpper(column)] - 1, row - 1);
    
    public static (int, int) Raw(string column, int row) => (ColumnValues[char.ToUpper(column[0])] - 1, row - 1);
}