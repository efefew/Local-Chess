using System.Collections.Generic;
/// <summary>
/// Башня
/// </summary>
public class Tower : Shape
{
    public override List<Cage> CheckAllMoves(bool check)
    {
        List<Cage> moveCages = new List<Cage>();
        Up(check, ref moveCages);
        Right(check, ref moveCages);
        Down(check, ref moveCages);
        Left(check, ref moveCages);
        return moveCages;
    }
}
