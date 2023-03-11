using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using static Shape;

public class Table : MonoBehaviour
{
    #region Properties

    /// <summary>
    /// Пешка на проходе
    /// </summary>
    public Pawn pawnOnThePass { get; set; }
    private Pawn pawnUpgrade;
    #endregion Properties

    #region Fields

    /// <summary>
    /// ход стороны
    /// </summary>
    private TypeSide sideStep;

    private bool realTable;
    private Chess chess;
    private Cage targetCage;
    private Cage[,] cages;
    private Dictionary<TypeSide, Shape> kings = new();
    private List<Cage> moveCages = new();
    public List<Cage> dangerCages = new(), killKingCages = new();
    public bool mate, pat;

    /// <summary>
    /// количество фигур шахующих короля
    /// </summary>
    public int countCheckKing;

    /// <summary>
    /// количество свободных ходов
    /// </summary>
    public int countFreeMoves;

    public List<Shape> shapes = new();

    #endregion Fields

    #region Methods

    /// <summary>
    /// Создание реальной шахматной доски
    /// </summary>
    private void CreateChessBoard(Cage whiteCage, Cage blackCage)
    {
        cages = new Cage[Chess.size, Chess.size];
        for (int x = 0; x < Chess.size; x++)
            for (int y = 0; y < Chess.size; y++)
            {
                if ((x + y) % 2 == 0)
                    cages[x, y] = Instantiate(blackCage, new Vector2(Chess.scalePosition * x, Chess.scalePosition * y), Quaternion.identity, transform);
                else
                    cages[x, y] = Instantiate(whiteCage, new Vector2(Chess.scalePosition * x, Chess.scalePosition * y), Quaternion.identity, transform);

                cages[x, y].GetComponent<Toggle>().group = chess.group;
                cages[x, y].Build(new Vector2Int(x, y), this);
                cages[x, y].transform.SetSiblingIndex(0);
            }
    }

    /// <summary>
    /// Создание виртуальной шахматной доски
    /// </summary>
    private void CreateChessBoard()
    {
        cages = new Cage[Chess.size, Chess.size];
        for (int x = 0; x < Chess.size; x++)
            for (int y = 0; y < Chess.size; y++)
                cages[x, y].Build(new Vector2Int(x, y), this);
    }

    /// <summary>
    /// Создание фигур
    /// </summary>
    private void CreateShapes(Shape[] whitePrefabs, Shape[] blackPrefabs)
    {
        for (int x = 0; x < Chess.size; x++)
        {
            CreateShape(whitePrefabs[0], cages[x, 1]);
            CreateShape(blackPrefabs[0], cages[x, Chess.size - 2]);
        }//пешки
        for (int id = 0; id < whitePrefabs.Length - 3/*исключаем пешку, королеву и короля*/; id++)
        {
            CreateShape(whitePrefabs[id + 1], cages[id, 0]);
            CreateShape(blackPrefabs[id + 1], cages[id, Chess.size - 1]);
            CreateShape(whitePrefabs[id + 1], cages[Chess.size - 1 - id, 0]);
            CreateShape(blackPrefabs[id + 1], cages[Chess.size - 1 - id, Chess.size - 1]);
        }//башня, конь, слон

        CreateShape(whitePrefabs[4], cages[3, 0]);
        CreateShape(blackPrefabs[4], cages[3, Chess.size - 1]);
        //королева

        CreateShape(whitePrefabs[5], cages[4, 0]);
        kings.Add(TypeSide.white, shapes.Last());
        CreateShape(blackPrefabs[5], cages[4, Chess.size - 1]);
        kings.Add(TypeSide.black, shapes.Last());
        //король
    }

    /// <summary>
    /// Создание фигуры
    /// </summary>
    /// <param name="shapePrefab">пример фигуры</param>
    /// <param name="x">позиция по оси x</param>
    /// <param name="y">позиция по оси y</param>
    /// <returns>созданная фигура</returns>
    private void CreateShape(Shape shapePrefab, Cage cage)
    {
        Shape shape;
        if (realTable)
            shape = Instantiate(shapePrefab, cage.transform.position, Quaternion.identity, cage.transform);
        else
            shape = (Shape)Activator.CreateInstance(shapePrefab.GetType());
        shape.Build(shapePrefab.side, cage, cages);
        shapes.Add(shape);
    }

    private void NextStep()
    {
        if (realTable)
            chess.group.SetAllTogglesOff();
        killKingCages.Clear();
        switch (sideStep)
        {
            case TypeSide.white:
                sideStep = TypeSide.black;
                break;

            case TypeSide.black:
                sideStep = TypeSide.white;
                break;

            default:
                break;
        }
        ClearCages();
        dangerCages.Clear();
        for (int id = 0; id < shapes.Count; id++)
            if (shapes[id].side != sideStep)
                shapes[id].relatedCages.Clear();
        Shape king = kings[sideStep];
        CheckDangerCages(king);

    }

    private void ClearCages()
    {
        if (realTable && targetCage && targetCage.shape)
        {
            targetCage.shape.green.SetActive(false);
        }
        targetCage = null;
        OnActiveMoveCages(false);
        moveCages.Clear();

    }

    private void CheckDangerCages(Shape king)
    {
        List<Cage> freeMoves = new List<Cage>();
        for (int id = 0; id < shapes.Count; id++)
            if (shapes[id].side != sideStep)
                dangerCages.AddRange(shapes[id].CheckAllMoves(true));
        //dangerCages = dangerCages.Distinct().ToList();
        countCheckKing = dangerCages.Where(cage => cage && cage.shape && cage.shape == king).Count();

        for (int id = 0; id < shapes.Count; id++)
            if (shapes[id].side == sideStep)
                freeMoves.AddRange(shapes[id].CheckAllMoves());

        countFreeMoves = freeMoves.Count;
        if (countFreeMoves == 0)
        {
            if (countCheckKing > 0)
                mate = true;
            else
                pat = true;
        }
    }

    private void ActiveCages(Cage cage)
    {
        ClearCages();
        moveCages = cage.shape.CheckAllMoves();

        targetCage = cage;
        if (!realTable)
            return;
        OnActiveMoveCages(true);
        targetCage.shape.green.SetActive(true);
    }

    private void CloneShapes(Cage[,] cagesReference, Pawn pawnOnThePass)
    {
        for (int x = 0; x < Chess.size; x++)
            for (int y = 0; y < Chess.size; y++)
                if (cagesReference[x, y].shape)
                {
                    CreateShape(cagesReference[x, y].shape, cages[x, y]);
                    if (pawnOnThePass && cagesReference[x, y].shape == pawnOnThePass)
                        this.pawnOnThePass = (Pawn)shapes.Last();
                }
    }

    /// <summary>
    /// При выборе клетки
    /// </summary>
    /// <param name="cage">выбранная клетка</param>
    /// <returns></returns>
    public void OnCage(Cage cage)
    {
        if (cage.shape && cage.shape.side == sideStep && (cage.shape.type == TypeShape.king || countCheckKing < 2))
        {
            ActiveCages(cage);
            return;
        }
        if (!targetCage || !moveCages.Contains(cage))
        {
            ClearCages();
            return;
        }
        if (realTable)
            targetCage.shape.green.SetActive(false);
        targetCage.shape.SetCage(cage);
        NextStep();
    }

    public void OnActiveMoveCages(bool on)
    {
        for (int i = 0; i < moveCages.Count; i++)
        {
            if (moveCages[i].shape)
                moveCages[i].shape.red.SetActive(on);
            moveCages[i].label.SetActive(on);
        }
    }

    /// <summary>
    /// Создание шахматного стола
    /// </summary>
    public void CreateTable(Chess chess, bool realTable)
    {
        this.realTable = realTable;
        this.chess = chess;
        sideStep = TypeSide.white;
        if (realTable)
            CreateChessBoard(chess.whiteCagePrefab, chess.blackCagePrefab);
        else
            CreateChessBoard();
        CreateShapes(chess.whiteShapePrefabs, chess.blackShapePrefabs);
    }

    public void CloneTable(TypeSide startSide, Chess chess, Pawn pawnOnThePass, Cage[,] cagesRef)
    {
        sideStep = startSide;
        this.chess = chess;
        CreateChessBoard();
        CloneShapes(cagesRef, pawnOnThePass);
    }
    public void OnPawnUpgrade(Pawn pawnUpgrade)
    {
        this.pawnUpgrade = pawnUpgrade;
        if (realTable)
            chess.upgradePawn.SetActive(true);
    }
    /// <summary>
    /// при выборе улучшения пешки
    /// </summary>
    /// <param name="idUpgrade">индекс улучшения</param>
    public void OnChoosingPawnUpgrade(int idUpgrade)
    {
        Cage cage = pawnUpgrade.cageSelf;
        pawnUpgrade.DestroySelf();
        CreateShape(chess.whiteShapePrefabs[idUpgrade], cage);
        pawnUpgrade = null;
    }
    #endregion Methods
}