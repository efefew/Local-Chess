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
    /// Метод отвечает за создание фигур на шахматной доске
    /// </summary>
    private void CreateShapes(Shape[] whitePrefabs, Shape[] blackPrefabs)
    {
        // Создаем пешки и устанавливаем их на соответствующие клетки на первом и последнем рядах доски для белых и чёрных соответственно
        for (int x = 0; x < Chess.size; x++)
        {
            CreateShape(whitePrefabs[0], cages[x, 1]);
            CreateShape(blackPrefabs[0], cages[x, Chess.size - 2]);
        }

        // Создаем фигуры, кроме пешек, королевы и короля, и устанавливаем их на соответствующие клетки на первом и последнем рядах доски для белых и чёрных соответственно. 
        // Это башни, кони и слоны.
        for (int id = 0; id < whitePrefabs.Length - 3/*(-3) = исключаем пешку, королеву и короля*/; id++)
        {
            CreateShape(whitePrefabs[id + 1], cages[id, 0]);
            CreateShape(blackPrefabs[id + 1], cages[id, Chess.size - 1]);
            CreateShape(whitePrefabs[id + 1], cages[Chess.size - 1 - id, 0]);
            CreateShape(blackPrefabs[id + 1], cages[Chess.size - 1 - id, Chess.size - 1]);
        }

        // Создаем королеву и устанавливаем ее на клетку d1 для белых и d8 для чёрных
        CreateShape(whitePrefabs[4], cages[3, 0]);
        CreateShape(blackPrefabs[4], cages[3, Chess.size - 1]);

        // Создаем фигуру короля и устанавливаем ее на клетку e1 для белых и e8 для чёрных. 
        // Фигуры королей добавляем в словарь "kings", где ключом является цвет фигуры, а значением - объект фигуры. 
        CreateShape(whitePrefabs[5], cages[4, 0]);
        kings.Add(TypeSide.white, shapes.Last());
        CreateShape(blackPrefabs[5], cages[4, Chess.size - 1]);
        kings.Add(TypeSide.black, shapes.Last());
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

    /// <summary>
    /// Метод отвечает за переход хода от одной стороны к другой и обновление состояния доски
    /// </summary>
    private void NextStep()
    {
        // Если игра происходит в реальном режиме, сбрасываем все выделения клеток
        if (realTable)
            chess.group.SetAllTogglesOff();

        // Очищаем список клеток, на которых можно взять короля
        killKingCages.Clear();

        // Переходим к следующей стороне
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

        // Очищаем все выделенные клетки
        ClearCages();

        // Очищаем список опасных клеток
        dangerCages.Clear();

        // Обновляем список возможных ходов для фигур противоположной стороны
        for (int id = 0; id < shapes.Count; id++)
            if (shapes[id].side != sideStep)
                shapes[id].relatedCages.Clear();

        // Получаем объект фигуры короля текущей стороны
        Shape king = kings[sideStep];

        // Проверяем, находится ли король в опасности и обновляем список опасных клеток
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

    /// <summary>
    /// Метод проверяет, находится ли король текущей стороны в опасности, и обновляет список опасных клеток
    /// </summary>
    /// <param name="king">король текущей стороны</param>
    private void CheckDangerCages(Shape king)
    {
        // Создаем список клеток, на которые можно сделать ход фигур противоположной стороны
        List<Cage> freeMoves = new List<Cage>();

        // Для каждой фигуры противоположной стороны получаем список всех клеток, на которые она может сделать ход
        for (int id = 0; id < shapes.Count; id++)
            if (shapes[id].side != sideStep)
                dangerCages.AddRange(shapes[id].CheckAllMoves(true));

        // Проверяем, находится ли король на одной из опасных клеток
        countCheckKing = dangerCages.Where(cage => cage && cage.shape && cage.shape == king).Count();

        // Получаем список всех клеток, на которые текущая сторона может сделать ход
        for (int id = 0; id < shapes.Count; id++)
            if (shapes[id].side == sideStep)
                freeMoves.AddRange(shapes[id].CheckAllMoves());

        // Обновляем количество свободных ходов текущей стороны
        countFreeMoves = freeMoves.Count;

        // Если свободных ходов нет и король находится под ударом, то это мат
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
        // Если на клетке есть фигура и ее сторона совпадает со значением переменной sideStep
        // и эта фигура - король или число проверок на шах короля меньше 2
        if (cage.shape && cage.shape.side == sideStep && (cage.shape.type == TypeShape.king || countCheckKing < 2))
        {
            // Вызываем метод ActiveCages со значением кликнутой клетки и возвращаем управление
            ActiveCages(cage);
            return;
        }
        // Если нет выбранной целевой клетки или список доступных для хода клеток не содержит кликнутую клетку
        // то вызываем метод ClearCages и возвращаем управление
        if (!targetCage || !moveCages.Contains(cage))
        {
            ClearCages();
            return;
        }
        // Если переменная realTable равна true, то отключаем зеленую подсветку на целевой клетке
        if (realTable)
            targetCage.shape.green.SetActive(false);
        // Устанавливаем фигуру с кликнутой клетки на целевую клетку и вызываем метод NextStep
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