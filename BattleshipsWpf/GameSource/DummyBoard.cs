namespace Battleships;

public class DummyBoard
{
    #region Private

    private readonly BoardValue[,] _board;
    
    private const int Size = 10;
    
    private const int ShipsParts = 20;

    #endregion


    #region Properties

    public bool AnyShipsLeft
    {
        get
        {
            var counter = 0;
            for (var i = 0; i < Size; i++)
            {
                for (var j = 0; j < Size; j++)
                {
                    if (_board[i, j] == BoardValue.DestroyedShipPart) counter++;
                }
            }

            return counter != ShipsParts;
        }
    }

    public BoardValue[,] Board { get => _board; }

    #endregion

    
    public DummyBoard()
    {
        _board = new BoardValue[Size,Size];
        for (var i = 0; i < Size; i++)
        {
            for (var j = 0; j < Size; j++)
            {
                _board[i, j] = BoardValue.None;
            }
        }
    }

    
    #region Public Methods

    public void InTargetShot((int, int) coordinates)
    {
        _board[coordinates.Item1, coordinates.Item2] = BoardValue.DestroyedShipPart;
    }

    public void MissedShot((int, int) coordinates)
    {
        _board[coordinates.Item1, coordinates.Item2] = BoardValue.Missed;
    }

    #endregion
}