using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Фигура
/// </summary>
public abstract class Shape : MonoBehaviour
{
    #region Enums

    public enum TypeSide
    {
        white,
        black
    }
    public enum TypeShape
    {
        pawn,
        tower,
        hourse,
        bishop,
        queen,
        king
    }
    #endregion Enums

    #region Properties

    protected int X
    { get { return cageSelf.position.x; } }
    protected int Y
    { get { return cageSelf.position.y; } }
    public bool firstStep { get; protected set; }

    #endregion Properties

    #region Fields

    protected Cage[,] cages;
    public TypeSide side;
    public TypeShape type;
    public GameObject green, red;
    /// <summary>
    /// клетка на которой фигура стоит
    /// </summary>
    public Cage cageSelf;

    public List<Cage> relatedCages = new List<Cage>();

    #endregion Fields

    #region Methods
    protected void KillerKing(Cage cage)
    {
        if (cage.shape && cage.shape.type == TypeShape.king)
            cageSelf.table.killKingCages.Add(cageSelf);
    }
    private void AfterRay(List<Cage> localMoveCages, List<Cage> relatedMoveCages, Shape relatedShape, bool check, bool killKing)
    {
        if (!check)
            return;
        if (killKing)
        {
            if (relatedShape)
            {
                relatedShape.relatedCages.Clear();
                relatedShape.relatedCages.Add(cageSelf);
                relatedShape.relatedCages.AddRange(localMoveCages);
                relatedShape.relatedCages.AddRange(relatedMoveCages);
            }
            else
            {
                cageSelf.table.killKingCages.Add(cageSelf);
                cageSelf.table.killKingCages.AddRange(localMoveCages);
            }
        }
    }

    /// <summary>
    /// Лучевой ход
    /// </summary>
    /// <param name="cage">крайняя клетка луча</param>
    /// <param name="killKing">луч пересекается вражеского короля</param>
    /// <returns>закончить трассировку</returns>
    protected bool Ray(Cage cage, ref List<Cage> moveCages, bool check, ref Shape relatedShape, ref bool killKing)
    {
        if (!check && cage.shape && cage.shape.side == side)//дружественная фигура
            return true;
        if (!CanMove(cage, check))
            return false;
        if (!cage.shape)
        {
            moveCages.Add(cage);
            return false;
        }

        if (cage.shape.side != side)
        {
            if (check)
            {
                if (cage.shape.type == TypeShape.king)
                    killKing = true;
                else
                    relatedShape = cage.shape;
            }
            moveCages.Add(cage);
        }
        else
        if (check)
            moveCages.Add(cage);

        return true;
    }

    /// <summary>
    /// Проверяет можно ли наступить на клетку
    /// </summary>
    /// <param name="cage">клетка</param>
    /// <param name="check">потенциальный ход</param>
    /// <returns>можно ли наступить</returns>
    protected bool CanMove(Cage cage, bool check)
    {
        if (relatedCages.Count > 0 && !relatedCages.Contains(cage))//фигура связанна и эти клетки не в связке
            return false;
        if (!check)//это не потенциальный ход
        {
            if (cageSelf.table.countCheckKing > 0 && !cageSelf.table.killKingCages.Contains(cage))//на короля нападают и ход на эту клетку не защитит короля
                return false;
            if (cage.shape && cage.shape.side == side)//дружественная фигура
                return false;
        }
        return true;
    }
    /// <summary>
    /// 🢁
    /// </summary>
    /// <param name="check"></param>
    /// <param name="moveCages"></param>
    protected void Up(bool check, ref List<Cage> moveCages)
    {
        int a, b;
        bool killKing = false;
        Shape relatedShape = null;
        List<Cage> localMoveCages = new();
        List<Cage> relatedMoveCages = new();
        for (int y = Y + 1; y < Chess.size; y++)
        {
            a = X;
            b = y;
            if (FinishRay(check, a, b, ref killKing, ref relatedShape, ref localMoveCages, ref relatedMoveCages))
                break;
        }
        moveCages.AddRange(localMoveCages);
        AfterRay(localMoveCages, relatedMoveCages, relatedShape, check, killKing);
    }

    /// <summary>
    /// 🢅
    /// </summary>
    /// <param name="check"></param>
    /// <param name="moveCages"></param>
    protected void RightUp(bool check, ref List<Cage> moveCages)
    {
        int a, b;
        bool killKing = false;
        Shape relatedShape = null;
        List<Cage> localMoveCages = new();
        List<Cage> relatedMoveCages = new();
        for (int xy = 1; (X + xy) < Chess.size && (Y + xy) < Chess.size; xy++)
        {
            a = X + xy;
            b = Y + xy;
            if (FinishRay(check, a, b, ref killKing, ref relatedShape, ref localMoveCages, ref relatedMoveCages))
                break;
        }
        moveCages.AddRange(localMoveCages);
        AfterRay(localMoveCages, relatedMoveCages, relatedShape, check, killKing);
    }

    /// <summary>
    /// 🢂
    /// </summary>
    /// <param name="check"></param>
    /// <param name="moveCages"></param>
    protected void Right(bool check, ref List<Cage> moveCages)
    {
        int a, b;
        bool killKing = false;
        Shape relatedShape = null;
        List<Cage> localMoveCages = new();
        List<Cage> relatedMoveCages = new();
        for (int x = X + 1; x < Chess.size; x++)
        {
            a = x;
            b = Y;
            if (FinishRay(check, a, b, ref killKing, ref relatedShape, ref localMoveCages, ref relatedMoveCages))
                break;
        }
        moveCages.AddRange(localMoveCages);
        AfterRay(localMoveCages, relatedMoveCages, relatedShape, check, killKing);
    }

    /// <summary>
    /// 🢆
    /// </summary>
    /// <param name="check"></param>
    /// <param name="moveCages"></param>
    protected void RightDown(bool check, ref List<Cage> moveCages)
    {
        int a, b;
        bool killKing = false;
        Shape relatedShape = null;
        List<Cage> localMoveCages = new();
        List<Cage> relatedMoveCages = new();
        for (int xy = 1; (X + xy) < Chess.size && (Y - xy) >= 0; xy++)
        {
            a = X + xy;
            b = Y - xy;
            if (FinishRay(check, a, b, ref killKing, ref relatedShape, ref localMoveCages, ref relatedMoveCages))
                break;
        }
        moveCages.AddRange(localMoveCages);
        AfterRay(localMoveCages, relatedMoveCages, relatedShape, check, killKing);
    }

    /// <summary>
    /// 🢃
    /// </summary>
    /// <param name="check"></param>
    /// <param name="moveCages"></param>
    protected void Down(bool check, ref List<Cage> moveCages)
    {
        int a, b;
        bool killKing = false;
        Shape relatedShape = null;
        List<Cage> localMoveCages = new();
        List<Cage> relatedMoveCages = new();
        for (int y = Y - 1; y >= 0; y--)
        {
            a = X;
            b = y;
            if (FinishRay(check, a, b, ref killKing, ref relatedShape, ref localMoveCages, ref relatedMoveCages))
                break;
        }
        moveCages.AddRange(localMoveCages);
        AfterRay(localMoveCages, relatedMoveCages, relatedShape, check, killKing);
    }

    /// <summary>
    /// 🢇
    /// </summary>
    /// <param name="check"></param>
    /// <param name="moveCages"></param>
    protected void LeftDown(bool check, ref List<Cage> moveCages)
    {
        int a, b;
        bool killKing = false;
        Shape relatedShape = null;
        List<Cage> localMoveCages = new();
        List<Cage> relatedMoveCages = new();
        for (int xy = 1; (X - xy) >= 0 && (Y - xy) >= 0; xy++)
        {
            a = X - xy;
            b = Y - xy;
            if (FinishRay(check, a, b, ref killKing, ref relatedShape, ref localMoveCages, ref relatedMoveCages))
                break;
        }
        moveCages.AddRange(localMoveCages);
        AfterRay(localMoveCages, relatedMoveCages, relatedShape, check, killKing);
    }

    /// <summary>
    /// 🢀
    /// </summary>
    /// <param name="check"></param>
    /// <param name="moveCages"></param>
    protected void Left(bool check, ref List<Cage> moveCages)
    {
        int a, b;
        bool killKing = false;
        Shape relatedShape = null;
        List<Cage> localMoveCages = new();
        List<Cage> relatedMoveCages = new();
        for (int x = X - 1; x >= 0; x--)
        {
            a = x;
            b = Y;
            if (FinishRay(check, a, b, ref killKing, ref relatedShape, ref localMoveCages, ref relatedMoveCages))
                break;
        }
        moveCages.AddRange(localMoveCages);
        AfterRay(localMoveCages, relatedMoveCages, relatedShape, check, killKing);
    }

    /// <summary>
    /// 🡼
    /// </summary>
    /// <param name="check"></param>
    /// <param name="moveCages"></param>
    protected void LeftUp(bool check, ref List<Cage> moveCages)
    {
        int a, b;
        bool killKing = false;
        Shape relatedShape = null;
        List<Cage> localMoveCages = new();
        List<Cage> relatedMoveCages = new();
        for (int xy = 1; (X - xy) >= 0 && (Y + xy) < Chess.size; xy++)
        {
            a = X - xy;
            b = Y + xy;
            if (FinishRay(check, a, b, ref killKing, ref relatedShape, ref localMoveCages, ref relatedMoveCages))
                break;
        }
        moveCages.AddRange(localMoveCages);
        AfterRay(localMoveCages, relatedMoveCages, relatedShape, check, killKing);
    }

    private bool FinishRay(bool check, int a, int b, ref bool killKing, ref Shape relatedShape, ref List<Cage> localMoveCages, ref List<Cage> relatedMoveCages)
    {
        if (killKing)
        {
            localMoveCages.Add(cages[a, b]);
            return true;
        }
        if (!relatedShape)
        {
            if (Ray(cages[a, b], ref localMoveCages, check, ref relatedShape, ref killKing))
                if (!killKing && !relatedShape)
                    return true;
        }
        else
        {
            if (Ray(cages[a, b], ref relatedMoveCages, check, ref relatedShape, ref killKing))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Все возможные ходы фигуры
    /// </summary>
    /// <param name="check">потенциальные ходы</param>
    /// <returns>Все возможные ходы</returns>
    public abstract List<Cage> CheckAllMoves(bool check = false);

    public virtual void Build(TypeSide side, Cage cageSelf, Cage[,] cages, bool firstStep = true)
    {
        this.side = side;
        this.cageSelf = cageSelf;
        this.cages = cages;
        this.firstStep = firstStep;
        cageSelf.shape = this;
        switch (this)
        {
            case Pawn pawn: type = TypeShape.pawn; break;
            case Tower tower: type = TypeShape.tower; break;
            case Hourse hourse: type = TypeShape.hourse; break;
            case Bishop bishop: type = TypeShape.bishop; break;
            case Queen queen: type = TypeShape.queen; break;
            case King king: type = TypeShape.king; break;
            default: throw new System.Exception("объект не является фигрой");
        }
    }

    /// <summary>
    /// Встать фигурой на клетку
    /// </summary>
    /// <param name="cage">клетка</param>
    public virtual void SetCage(Cage cage)
    {
        cageSelf.table.pawnOnThePass = null;
        firstStep = false;
        cageSelf.shape = null;
        cageSelf = cage;
        transform.position = cage.transform.position;
        transform.parent = cage.transform;
        cage.DestroyShape();
        cage.shape = this;
    }

    [ContextMenu("DestroySelf")]
    public void DestroySelf() => cageSelf.DestroyShape();

    #endregion Methods
}