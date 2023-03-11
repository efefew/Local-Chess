using System.Collections.Generic;
/// <summary>
/// Конь
/// </summary>
public class Hourse : Shape
{
    public override List<Cage> CheckAllMoves(bool check)
    {
        List<Cage> moveCages = new List<Cage>();
        Cage cage;
        for (int i = -1; i <= 1; i += 2)
            for (int ii = -2; ii <= 2; ii += 4)
            {
                if ((X + i) < 8 && (Y + ii) < 8 && (X + i) >= 0 && (Y + ii) >= 0)
                {
                    cage = cages[X + i, Y + ii];
                    if (CanMove(cage, check))
                    {
                        KillerKing(cage);
                        moveCages.Add(cage);
                    }
                }
                if ((Y + i) < 8 && (X + ii) < 8 && (Y + i) >= 0 && (X + ii) >= 0)
                {
                    cage = cages[X + ii, Y + i];
                    if (CanMove(cage, check))
                    {
                        KillerKing(cage);
                        moveCages.Add(cage);
                    }
                }
            }
        return moveCages;
    }
}
