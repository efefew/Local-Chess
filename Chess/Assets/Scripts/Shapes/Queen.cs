using System.Collections.Generic;
/// <summary>
/// Королева
/// </summary>
public class Queen : Shape
{
    public override List<Cage> CheckAllMoves(bool check)
    {
        List<Cage> moveCages = new List<Cage>();
        Up(check, ref moveCages);
        RightUp(check, ref moveCages);
        Right(check, ref moveCages);
        RightDown(check, ref moveCages);
        Down(check, ref moveCages);
        LeftDown(check, ref moveCages);
        Left(check, ref moveCages);
        LeftUp(check, ref moveCages);
        return moveCages;
    }
}
