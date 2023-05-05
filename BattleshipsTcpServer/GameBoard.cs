namespace BattleshipsTcpServer;

public class GameBoard
{
    #region Private Values

    private readonly BoardValue[,] _board, _opponentsBoard;
    
    private readonly int _size = 10;

    private static readonly Dictionary<Ship, int> ShipsAmount = new()
    {
        {Ship.Battleship, 1},
        {Ship.Cruiser, 2},
        {Ship.Destroyer, 3},
        {Ship.Speedboat, 4}
    };

    private Dictionary<Ship, int> _shipsAmount = ShipsAmount;

    #endregion


    #region Properties

    public bool Empty => IsBoardEmpty();

    public bool IsAnyActiveShipsLeft => IsAnyShipsLeft();

    private bool IsBoardEmpty()
    {
        for (var i = 0; i < _size; i++)
        {
            for (var j = 0; j < _size; j++)
            {
                if (_board[i, j] != BoardValue.None) return false;
                
            }
        }
        return true;
    }

    private bool IsAnyShipsLeft()
    {
        for (var i = 0; i < _size; i++)
        {
            for (var j = 0; j < _size; j++)
            {
                if (_board[i, j] == BoardValue.ActiveShipPart) return true;
            }
        }
        return false;
    }

    #endregion


    #region Constructors

    public GameBoard()
    {
        _board = new BoardValue[_size, _size];
        _opponentsBoard = new BoardValue[_size, _size];
        ClearBoard();
    }

    #endregion
    

    #region Setting Board

    public void AutomaticGeneration()
    {
        ClearBoard();

        var positions = new List<(int, int)>(_size * _size);
        for (var i = 0; i < _size; i++) for (var j = 0; j < _size; j++) positions.Add((i, j));

        foreach (var pair in _shipsAmount)
        {
            var amount = pair.Value;
            for (var i = 0; i < amount; i++)
            {
                while (true)
                {
                    var position = Extensions.GetRandomTuple(positions);

                    var direction = Extensions.GetRandomDirection();
                    var directionTries = 0;
                    var shipSet = false;

                    while (true)
                    {
                        try
                        {
                            if (SetShip(pair.Key, position, direction))
                            {
                                shipSet = true;
                                break;
                            }
                            directionTries++;
                            if (directionTries > Enum.GetNames(typeof(Direction)).Length)
                            {
                                positions.Remove(position);
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            directionTries++;
                            if (directionTries > Enum.GetNames(typeof(Direction)).Length)
                            {
                                positions.Remove(position);
                                break;
                            }
                        }
                    }

                    if (!shipSet) continue;
                    
                    var currentCoordinates = position;
                    for (var j = 0; j < pair.Key.ToInt(); j++)
                    {
                        positions.Remove(currentCoordinates);
                        positions.Remove(Extensions.SumTuple(currentCoordinates, Direction.North.GetOffset()));
                        positions.Remove(Extensions.SumTuple(currentCoordinates, Direction.East.GetOffset()));
                        positions.Remove(Extensions.SumTuple(currentCoordinates, Direction.South.GetOffset()));
                        positions.Remove(Extensions.SumTuple(currentCoordinates, Direction.West.GetOffset()));
                        currentCoordinates = Extensions.SumTuple(currentCoordinates, direction.GetOffset());
                    }
                    break;
                }
            }
        }
    }
    
    public bool SetShip(Ship ship, (int, int) startPosition, Direction direction)
    {
        var shipSize = ship.ToInt();

        if (direction == Direction.North && startPosition.Item1 - shipSize < 0 ||
            direction == Direction.East && startPosition.Item2 + shipSize > _size ||
            direction == Direction.South && startPosition.Item1 + shipSize > _size ||
            direction == Direction.West && startPosition.Item2 - shipSize < 0)
            throw new Exception("Incorrect coordinates");

        if (_shipsAmount[ship] == 0) return false;
        
        var currentCoordinates = startPosition;
        for (var i = 0; i < shipSize; i++)
        {
            if (!CheckCoordinates(currentCoordinates)) return false;
            currentCoordinates = Extensions.SumTuple(currentCoordinates, direction.GetOffset());
        }

        currentCoordinates = startPosition;
        for (var i = 0; i < shipSize; i++)
        {
            _board[currentCoordinates.Item1, currentCoordinates.Item2] = BoardValue.ActiveShipPart;
            currentCoordinates = Extensions.SumTuple(currentCoordinates, direction.GetOffset());
        }

        _shipsAmount[ship]--;
        return true;
    }

    private bool CheckCoordinates((int, int) coordinates)
    {
        var xl = Math.Max(coordinates.Item1 - 1, 0);
        var xu = Math.Min(coordinates.Item1 + 1, _size);

        var yl = Math.Max(coordinates.Item2 - 1, 0);
        var yu = Math.Min(coordinates.Item2 + 1, _size);

        for (var j = yl; j <= yu; j++)
        {
            for (var i = xl; i <= xu; i++)
            {
                if (_board[i, j] is BoardValue.ActiveShipPart)
                    return false;
            }
        }

        return true;
    }

    private void ClearBoard()
    {
        for (var i = 0; i < _size; i++)
        {
            for (var j = 0; j < _size; j++)
            {
                _board[i, j] = BoardValue.None;
                _opponentsBoard[i, j] = BoardValue.None;
            }
        }

        _shipsAmount = new() 
        {
            {Ship.Battleship, 1},
            {Ship.Cruiser, 2},
            {Ship.Destroyer, 3},
            {Ship.Speedboat, 4}
        };
    }

    #endregion


    #region Second Player Methods

    public bool Shot((int, int) coordinates, out bool killed)
    {
        killed = false;
        if(_board[coordinates.Item1, coordinates.Item2] is BoardValue.None or 
           BoardValue.Missed or BoardValue.DestroyedShipPart)
            return false;

        _board[coordinates.Item1, coordinates.Item2] = BoardValue.DestroyedShipPart;
        if (CheckCoordinates(coordinates)) killed = true;
        return true;
    }
    
    public BoardValue[,] GetShotsBoard()
    {
        var board = new BoardValue[_size, _size];
        for (var i = 0; i < _size; i++)
        {
            for (var j = 0; j < _size; j++)
            {
                if (_board[i, j] is BoardValue.ActiveShipPart or BoardValue.None)
                {
                    board[i, j] = BoardValue.None;
                    continue;
                }

                board[i, j] = _board[i, j];
            }
        }

        return board;
    }

    #endregion
    

    #region Print

    public void Print()
    {
        Console.WriteLine(ToString());
    }
    
    public override string ToString()
    {
        var s = "";
        for (var i = 0; i < _size; i++) s += i + "\t";
        s += "\n" + string.Join("", Enumerable.Repeat('=', _size * 8)) + "\n";

        for (var i = 0; i < _size; i++)
        {
            for (var j = 0; j < _size; j++)
            {
                s += (_board[i,j] != BoardValue.None ? "X" : "_") + "\t";
            }
            s += $"| {i}\n";
        }
        s += "\n" + string.Join("", Enumerable.Repeat('=', _size * 8)) + "\n";
        return s;
    }

    public BoardValue[,] GetBoardValues()
    {
        return _board;
    }

    #endregion
}