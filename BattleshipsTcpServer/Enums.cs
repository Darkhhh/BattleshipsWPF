namespace BattleshipsTcpServer;

public enum ShotResult : byte
{
    Missed, Shotted, Killed
}

public enum Turn : byte
{
    First = 0, Second = 1
}

public enum Indicators : byte
{
    ResultIndicator = 11, TurnIndicator = 12
}


public enum Ship
{
    Speedboat = 1, Destroyer, Cruiser, Battleship
}

public enum BoardValue
{
    None, ActiveShipPart, DestroyedShipPart, Missed
}

public enum Direction
{
    North, East, South, West
}