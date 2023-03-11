using System.Collections.Generic;
/// <summary>
/// Слон
/// </summary>
public class Bishop : Shape
{
    public override List<Cage> CheckAllMoves(bool check)
    {
        List<Cage> moveCages = new List<Cage>();
        RightUp(check, ref moveCages);
        RightDown(check, ref moveCages);
        LeftDown(check, ref moveCages);
        LeftUp(check, ref moveCages);
        return moveCages;
    }
}
