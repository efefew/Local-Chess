using System.Collections.Generic;
/// <summary>
/// Пешка
/// </summary>
public class Pawn : Shape
{
    private int add;
    public override void Build(TypeSide side, Cage cageSelf, Cage[,] cages, bool firstStep = true)
    {
        base.Build(side, cageSelf, cages, firstStep);
        add = side == TypeSide.white ? 1 : -1;
    }
    public override void SetCage(Cage cage)
    {
        Pawn pawn = cageSelf.table.pawnOnThePass;
        if (pawn && InRange())
        {
            if (X - 1 >= 0 && pawn.X == (X - 1) && pawn.Y == Y)//🡼
                cages[X - 1, Y].DestroyShape();
            if (X + 1 < Chess.size && pawn.X == (X + 1) && pawn.Y == Y)//🢅
                cages[X + 1, Y].DestroyShape();
        }
        base.SetCage(cage);
        cageSelf.table.pawnOnThePass = firstStep && cage == cages[X, Y + 2 * add] ? this : null;//🢁🢁
        if (!InRange())
            cageSelf.table.OnPawnUpgrade(this);
    }
    private bool InRange() => cageSelf.position.y - 1 >= 0 && cageSelf.position.y + 1 < Chess.size;
    public override List<Cage> CheckAllMoves(bool check)
    {
        List<Cage> moveCages = new List<Cage>();

        if (InRange())
        {
            Cage cage = cages[X, Y + add];
            if (!cage.shape && !check)
            {
                if (CanMove(cage, check))
                    moveCages.Add(cage);//🢁
                if (firstStep)
                {
                    cage = cages[X, Y + 2 * add];
                    if (!cage.shape && CanMove(cage, check))
                        moveCages.Add(cage);//🢁🢁
                }
            }
            if (X - 1 >= 0)
            {
                cage = cages[X - 1, Y + add];
                if (check || cage.shape && side != cage.shape.side && CanMove(cage, check))
                {
                    KillerKing(cage);
                    moveCages.Add(cage);//🡼
                }
            }
            if (X + 1 < Chess.size)
            {
                cage = cages[X + 1, Y + add];
                if (check || cage.shape && side != cage.shape.side && CanMove(cage, check))
                {
                    KillerKing(cage);
                    moveCages.Add(cage);//🢅
                }
            }
            Pawn pawn = cageSelf.table.pawnOnThePass;
            if (!check && pawn)
            {
                if (X - 1 >= 0 && pawn.X == (X - 1) && pawn.Y == Y && CanMove(cage, check))
                    moveCages.Add(cages[X - 1, Y + add]);//🡼
                if (X + 1 < Chess.size && pawn.X == (X + 1) && pawn.Y == Y && CanMove(cage, check))
                    moveCages.Add(cages[X + 1, Y + add]);//🢅
            }//взятие пешки на проходе
        }
        return moveCages;
    }
}
