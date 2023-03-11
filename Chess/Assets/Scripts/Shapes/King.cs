using System.Collections.Generic;
/// <summary>
/// Король
/// </summary>
public class King : Shape
{
    public override void SetCage(Cage cage)
    {
        if (firstStep)
        {
            if (cage == cages[X + 2, Y])//ракировка 🢂🢂
                cages[Chess.size - 1, Y].shape.SetCage(cages[X + 1, Y]);//ракировать башню
            if (cage == cages[X - 2, Y])//ракировка 🢀🢀
                cages[0, Y].shape.SetCage(cages[X - 1, Y]);//ракировать башню
        }

        base.SetCage(cage);
    }
    public override List<Cage> CheckAllMoves(bool check)
    {
        List<Cage> moveCages = new List<Cage>();
        //🡼🢁🢅
        //🢀 🢂
        //🢇🢃🢆
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                if (!(x == 0 && y == 0) && InRange(x, y))
                    KingMove(cages[X + x, Y + y], ref moveCages, check);
        #region ракировка
        if (firstStep && !check)
        {
            bool obstacle = false;//препятствие в виде фигур для ракировки
            if (cages[Chess.size - 1, Y].shape && cages[Chess.size - 1, Y].shape.firstStep)
            {
                for (int x = X + 1; x < Chess.size - 1; x++)
                    if (cages[x, Y].shape)
                    {
                        obstacle = true;
                        break;
                    }
                if (!obstacle && !IsDanger(cages[X + 2, Y]))
                    moveCages.Add(cages[X + 2, Y]);
            }//ракировка 🢂🢂

            obstacle = false;
            if (cages[0, Y].shape && cages[0, Y].shape.firstStep)
            {
                for (int x = X - 1; x > 0; x--)
                    if (cages[x, Y].shape)
                    {
                        obstacle = true;
                        break;
                    }
                if (!obstacle && !IsDanger(cages[X - 2, Y]))
                    moveCages.Add(cages[X - 2, Y]);
            }//ракировка 🢀🢀
        }
        #endregion
        return moveCages;
    }
    private bool InRange(int x, int y)
    {
        int X = cageSelf.position.x,
            Y = cageSelf.position.y;
        return (X + x) < Chess.size && (Y + y) < Chess.size && (X + x) >= 0 && (Y + y) >= 0;
    }
    private void KingMove(Cage cage, ref List<Cage> moveCages, bool check)
    {
        if ((!cage.shape || side != cage.shape.side || check) && !IsDanger(cage))//если на пути нет фигур  или это враг или сейчас трасировка шаха и при этом там безопасно
            moveCages.Add(cage);
    }
    private bool IsDanger(Cage cage) => cage.table.dangerCages.Contains(cage);
}
