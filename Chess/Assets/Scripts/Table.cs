using UnityEngine;
using UnityEngine.UI;
public struct InformationOfTheSituation
{
    public Cage[][] cages;
    public bool
        hideHint,
        moving,
        rayCheck,
        rayMove;
}
[System.Serializable]
public struct InformationForGeneralMove
{
    public int oldX, oldY, newX, newY;
}
[System.Serializable]
public class Cage
{
    #region PUBLIC
    public bool
        canMove = false,//может ходить
        moverBlack,//цвет фигуры, которая хочет встать на эту клетку
        black,//цвет фигуры на этой клетке
        saveCheck,//клетка, спасающая от шаха или мата
        firstStep,//первый шаг
        killPawnInPass;//взятие пешки в проходе
    public int
        countFreeMove,//количество свободных ходов
        idBind,//индекс связывания
        moverX,
        moverY,
        moverRank = -1,
        moverId = -1,
        checkMove = 0,
        id,
        rank;//-1-ничего 0-ладья 1-конь 2-слон 3-королева 4-король 5-пешка
    public Vector2 position;
    public GameObject image;
    public Table table;
    #endregion
    #region PRIVATE
    private int X, Y;
    #endregion
    public Cage(int x, int y, float scalePos, GameObject image, Table table, bool black = false, int rank = -1, int id = -1)
    {
        X = x;
        Y = y;
        this.image = image;
        this.black = black;
        this.rank = rank;
        this.id = id;
        this.table = table;
        switch (rank)
        {
            case 0: firstStep = true; break;
            case 4: firstStep = true; break;
            case 5: firstStep = true; break;
            default: firstStep = false; break;
        }
        position = new Vector2(X * scalePos, Y * scalePos);
    }
    public void Clear(Cage[][] cages)//очистка наличия ходов этой фигурой
    {
        InformationOfTheSituation info = new InformationOfTheSituation();
        info.hideHint = true;
        info.cages = cages;
        info.rayCheck = false;
        info.moving = false;
        info.rayMove = false;
        ChooseChessPiece(info);
    }
    public int Move(Cage[][] cages, bool hideHint = false)//проверка на наличие ходов этой фигурой
    {
        countFreeMove = 0;
        InformationOfTheSituation info = new InformationOfTheSituation();
        info.hideHint = hideHint;
        info.cages = cages;
        info.rayCheck = false;
        info.moving = true;
        info.rayMove = false;
        ChooseChessPiece(info);

        return countFreeMove;
    }
    public int RayMove(Cage[][] cages)//проверка на наличие ходов этой фигурой
    {
        countFreeMove = 0;
        InformationOfTheSituation info = new InformationOfTheSituation();
        info.hideHint = true;
        info.cages = cages;
        info.rayCheck = false;
        info.moving = true;
        info.rayMove = true;
        ChooseChessPiece(info);

        return countFreeMove;
    }
    public void RayCheck(Cage[][] cages)//проверка на наличие шаха этой фигурой
    {
        InformationOfTheSituation info = new InformationOfTheSituation();
        info.hideHint = false;
        info.cages = cages;
        info.rayCheck = true;
        info.moving = false;
        info.rayMove = false;
        ChooseChessPiece(info);
    }

    private void ChooseChessPiece(InformationOfTheSituation info)//выбор шахматной Фигуры
    {
        switch (rank)
        {
            case 0: Tower(info); break;
            case 1: Horse(info); break;
            case 2: Bishop(info); break;
            case 3: Queen(info); break;
            case 4: King(info); break;
            case 5: Pawn(info); break;
            default: break;
        }
    }
    public bool MoveEnd(Cage moverCage)//при окончании хода
    {
        moverCage.rank = -1;
        moverCage.id = -1;
        rank = moverRank;
        id = moverId;
        black = moverBlack;
        if (rank == 4)//если король
        {
            if (moverCage.firstStep && Mathf.Abs(moverCage.X - X) > 1)
                table.GeneralMove();
        }
        if (rank == 5)//если пешка
        {
            if (Mathf.Abs(moverY - Y) == 2)
            {
                table.taking_a_PawnOnThePass = true;
                table.xPawn = X;
                table.yPawn = Y;
            }
            if (killPawnInPass)
            {
                table.GeneralDestoy(table.xPawn, table.yPawn);
            }
            if (black)
            {
                if (Y == 0)//если дошёл до конца
                    return true;
            }
            else
            {
                if (Y == 7)//если дошёл до конца
                    return true;
            }

        }
        moverCage.firstStep = false;
        return false;
    }

    private void ChooseTarget(Cage moveToCage, InformationOfTheSituation info)//выбор клетки
    {
        bool bind = idBind == 0 || !table.check && (moveToCage.idBind == idBind);

        if (bind && (!table.check || moveToCage.saveCheck) || rank == 4)
        {
            countFreeMove++;
            if (!info.rayMove)
            {
                if (info.rayCheck)
                {
                    moveToCage.checkMove++;
                    if (moveToCage.rank == 4 && moveToCage.black != black)
                        saveCheck = true;
                }
                else
                {
                    moveToCage.image.SetActive(!info.hideHint);
                    moveToCage.canMove = info.moving;
                    moveToCage.moverX = X;
                    moveToCage.moverY = Y;
                    moveToCage.moverRank = rank;
                    moveToCage.moverId = id;
                    moveToCage.moverBlack = black;
                }
            }
        }

    }

    private void Horse(InformationOfTheSituation info)
    {
        Cage cage;
        for (int i = -1; i <= 1; i += 2)
            for (int ii = -2; ii <= 2; ii += 4)
            {
                if ((X + i) < 8 && (Y + ii) < 8 && (X + i) >= 0 && (Y + ii) >= 0)
                {
                    cage = info.cages[X + i][Y + ii];
                    if (cage.rank == -1 || black != cage.black || info.rayCheck)
                        ChooseTarget(cage, info);
                }
                if ((Y + i) < 8 && (X + ii) < 8 && (Y + i) >= 0 && (X + ii) >= 0)
                {
                    cage = info.cages[X + ii][Y + i];
                    if (cage.rank == -1 || black != cage.black || info.rayCheck)
                        ChooseTarget(cage, info);
                }
            }
    }

    private void Bishop(InformationOfTheSituation info)
    {
        Vector2Int posKing = Vector2Int.one * -1;
        bool bindShape = false, firstEnemy;
        firstEnemy = true;
        for (int xy = 1; (X + xy) < 8 && (Y + xy) < 8; xy++)
        {
            int a = X + xy, b = Y + xy;
            if (Ray(info, info.cages[a][b], ref posKing, ref bindShape, ref firstEnemy))
                break;
        }   //🢅
        firstEnemy = true;
        for (int xy = 1; (X + xy) < 8 && (Y - xy) >= 0; xy++)
        {
            int a = X + xy, b = Y - xy;
            if (Ray(info, info.cages[a][b], ref posKing, ref bindShape, ref firstEnemy))
                break;
        }  //🢆
        firstEnemy = true;
        for (int xy = 1; (X - xy) >= 0 && (Y + xy) < 8; xy++)
        {
            int a = X - xy, b = Y + xy;
            if (Ray(info, info.cages[a][b], ref posKing, ref bindShape, ref firstEnemy))
                break;
        }  //🡼
        firstEnemy = true;
        for (int xy = 1; (X - xy) >= 0 && (Y - xy) >= 0; xy++)
        {
            int a = X - xy, b = Y - xy;
            if (Ray(info, info.cages[a][b], ref posKing, ref bindShape, ref firstEnemy))
                break;
        } //🢇


        if (posKing.x != -1)
        {
            if (bindShape)
                idBind = table.countBindShape;
            else
                saveCheck = true;

            if (posKing.x > X)
            {
                if (posKing.y > Y)
                    for (int xy = 1; (X + xy) < 8 && (Y + xy) < 8; xy++)
                    {
                        int a = X + xy, b = Y + xy;
                        if (InCheckRay(info.cages[a][b], bindShape))
                            break;
                    }//🢅
                if (posKing.y < Y)
                    for (int xy = 1; (X + xy) < 8 && (Y - xy) >= 0; xy++)
                    {
                        int a = X + xy, b = Y - xy;
                        if (InCheckRay(info.cages[a][b], bindShape))
                            break;
                    }//🢆
            }
            if (posKing.x < X)
            {
                if (posKing.y > Y)
                    for (int xy = 1; (X - xy) >= 0 && (Y + xy) < 8; xy++)
                    {
                        int a = X - xy, b = Y + xy;
                        if (InCheckRay(info.cages[a][b], bindShape))
                            break;
                    }//🡼

                if (posKing.y < Y)
                    for (int xy = 1; (X - xy) >= 0 && (Y - xy) >= 0; xy++)
                    {
                        int a = X - xy, b = Y - xy;
                        if (InCheckRay(info.cages[a][b], bindShape))
                            break;
                    }//🢇
            }
        }//линия шаха королю
    }

    private void Tower(InformationOfTheSituation info)
    {
        Vector2Int posKing = Vector2Int.one * -1;
        bool bindShape = false, firstEnemy;
        firstEnemy = true;
        for (int x = X + 1; x < 8; x++)
        {
            int a = x, b = Y;
            if (Ray(info, info.cages[a][b], ref posKing, ref bindShape, ref firstEnemy))
                break;
        }   //🢂
        firstEnemy = true;
        for (int x = X - 1; x >= 0; x--)
        {
            int a = x, b = Y;
            if (Ray(info, info.cages[a][b], ref posKing, ref bindShape, ref firstEnemy))
                break;
        }  //🢀
        firstEnemy = true;
        for (int y = Y + 1; y < 8; y++)
        {
            int a = X, b = y;
            if (Ray(info, info.cages[a][b], ref posKing, ref bindShape, ref firstEnemy))
                break;
        }   //🢁
        firstEnemy = true;
        for (int y = Y - 1; y >= 0; y--)
        {
            int a = X, b = y;
            if (Ray(info, info.cages[a][b], ref posKing, ref bindShape, ref firstEnemy))
                break;
        }  //🢃

        if (posKing.x != -1)
        {
            if (bindShape)
                idBind = table.countBindShape;
            else
                saveCheck = true;

            if (posKing.x > X)
                for (int x = X + 1; x < 8; x++)
                {
                    int a = x, b = Y;
                    if (InCheckRay(info.cages[a][b], bindShape))
                        break;
                }   //🢂
            if (posKing.x < X)
                for (int x = X - 1; x >= 0; x--)
                {
                    int a = x, b = Y;
                    if (InCheckRay(info.cages[a][b], bindShape))
                        break;
                }  //🢀
            if (posKing.y > Y)
                for (int y = Y + 1; y < 8; y++)
                {
                    int a = X, b = y;
                    if (InCheckRay(info.cages[a][b], bindShape))
                        break;
                }   //🢁
            if (posKing.y < Y)
                for (int y = Y - 1; y >= 0; y--)
                {
                    int a = X, b = y;
                    if (InCheckRay(info.cages[a][b], bindShape))
                        break;
                }  //🢃
        }//линия шаха королю
    }

    private bool InCheckRay(Cage cage, bool bindShape)
    {
        bool enemyKing = cage.rank == 4 && black != cage.black;
        if (enemyKing)
            return true;

        if (bindShape)
            cage.idBind = table.countBindShape;
        else
            cage.saveCheck = true;
        return false;
    }

    private bool Ray(InformationOfTheSituation info, Cage cage, ref Vector2Int posKing, ref bool bindShape, ref bool firstEnemy)
    {
        bool enemyKing = cage.rank == 4 && black != cage.black;
        if (info.rayCheck && enemyKing)
        {
            posKing.x = cage.X;
            posKing.y = cage.Y;
            if (!firstEnemy)
            {
                table.countBindShape++;
                bindShape = true;
            }//проверка связывания
        }
        else
        if (cage.rank != -1)
        {
            if (Enemy(cage) || info.rayCheck)
            {
                if (Enemy(cage) && firstEnemy && info.rayCheck && !enemyKing)
                {
                    firstEnemy = false;
                    return false;
                }
                if (firstEnemy)
                {
                    if (posKing.x == -1)
                        ChooseTarget(cage, info);
                    else
                        ChooseTarget(cage, info);
                }


            }
            return true;
        }//если на пути фигура и сейчас не трасировка шаха и вражеский король
        if (firstEnemy)
            ChooseTarget(cage, info);
        return false;
    }

    private void Queen(InformationOfTheSituation info)
    {
        Tower(info);
        Bishop(info);
    }

    private bool Enemy(Cage cage)
    {
        return black != cage.black;
    }

    private void King(InformationOfTheSituation info)
    {
        if (Y + 1 < 8)//не выходит за границы y
            KingMove(info.cages[X][Y + 1], info);//🢁
        if (Y - 1 >= 0)//не выходит за границы y
            KingMove(info.cages[X][Y - 1], info);//🢃
        if (X + 1 < 8)//не выходит за границы x
        {
            KingMove(info.cages[X + 1][Y], info);//🢂
            if (Y + 1 < 8)//не выходит за границы y
                KingMove(info.cages[X + 1][Y + 1], info);//🢅
            if (Y - 1 >= 0)//не выходит за границы y
                KingMove(info.cages[X + 1][Y - 1], info);//🢆
        }
        if (X - 1 >= 0)//не выходит за границы x
        {
            KingMove(info.cages[X - 1][Y], info);//🢀
            if (Y + 1 < 8)//не выходит за границы y
                KingMove(info.cages[X - 1][Y + 1], info);//🡼
            if (Y - 1 >= 0)//не выходит за границы y
                KingMove(info.cages[X - 1][Y - 1], info);//🢇
        }
        #region или можно кратко, но менее понятно
        //for (int x = -1; x <= 1; x++)
        //    for (int y = -1; y <= 1; y++)
        //        if (!(x == 0 && y == 0))
        //            if ((X + x) < 8 && (Y + y) < 8 && (X + x) >= 0 && (Y + y) >= 0)
        //                KingMove(info.cages[X + x][Y + y], info);
        #endregion

        if (firstStep && !info.rayMove && !info.rayCheck)
        {
            bool obstacle = false;//препятствие для ракировки
            {

                for (int x = X + 1; x < 8 - 1; x++)
                    if (info.cages[x][Y].rank != -1)
                        obstacle = true;
                if (!obstacle && info.cages[X + 2][Y].checkMove == 0)
                {
                    if (info.moving)
                    {
                        table.generalMove.oldX = 7;
                        table.generalMove.oldY = Y;
                        table.generalMove.newX = X + 1;
                        table.generalMove.newY = Y;
                    }
                    ChooseTarget(info.cages[X + 2][Y], info);
                }
            }//ракировка 🢂

            obstacle = false;
            {
                for (int x = X - 1; x > 0; x--)
                    if (info.cages[x][Y].rank != -1)
                        obstacle = true;
                if (!obstacle && info.cages[X - 2][Y].checkMove == 0)
                {
                    if (info.moving)
                    {
                        table.generalMove.oldX = 0;
                        table.generalMove.oldY = Y;
                        table.generalMove.newX = X - 1;
                        table.generalMove.newY = Y;
                    }
                    ChooseTarget(info.cages[X - 2][Y], info);
                }
            }//ракировка 🢀
        }//ракировка
    }

    private void KingMove(Cage cage, InformationOfTheSituation info)
    {
        if ((cage.rank == -1 || black != cage.black || info.rayCheck) && cage.checkMove == 0)//если на пути нет фигур  или это враг или сейчас трасировка шаха и при этом там безопасно
            ChooseTarget(cage, info);
    }

    private void Pawn(InformationOfTheSituation info)
    {
        int size = 8,
        add = black ? -1 : 1;

        bool inRange = Y - 1 >= 0 && Y + 1 < 8;
        if (inRange)
        {
            Cage cage = info.cages[X][Y + add];
            if (cage.rank == -1 && !info.rayCheck)
            {
                if (firstStep)
                {
                    cage = info.cages[X][Y + 2 * add];
                    if (cage.rank == -1)
                        ChooseTarget(cage, info);//🢁🢁
                }
                cage = info.cages[X][Y + add];
                ChooseTarget(cage, info);//🢁
            }
            if (X - 1 >= 0)
            {
                cage = info.cages[X - 1][Y + add];
                if (cage.rank != -1 && black != cage.black || info.rayCheck)
                    ChooseTarget(cage, info);//🡼
            }
            if (X + 1 < size)
            {
                cage = info.cages[X + 1][Y + add];
                if (cage.rank != -1 && black != cage.black || info.rayCheck)
                    ChooseTarget(cage, info);//🢅
            }

            if (!info.rayCheck && table.taking_a_PawnOnThePass)
            {
                if (X - 1 >= 0 && table.xPawn == (X - 1) && table.yPawn == Y)
                {
                    ChooseTarget(info.cages[X - 1][Y + add], info);//🡼
                    info.cages[X - 1][Y + add].killPawnInPass = true;
                }
                if (X + 1 < size && table.xPawn == (X + 1) && table.yPawn == Y)
                {
                    ChooseTarget(info.cages[X + 1][Y + add], info);//🢅
                    info.cages[X + 1][Y + add].killPawnInPass = true;
                }
            }//взятие пешки на проходе
        }
    }
    public void ClearCage()
    {
        image.SetActive(false);
        canMove = false;
    }
}
public class Table : MonoBehaviour
{
    #region PUBLIC
    public GameObject CageObj, checkLabel, mateLabel;
    public GameObject[] White, Black;
    public Sprite[] sprites;
    public UpgradePawn upgradePawn;
    public Vector2Int whiteKing, blackKing;
    public float Poz;
    public bool wait, check, mate, taking_a_PawnOnThePass/*взятие пешки на проходе*/;
    public Cage showCage;
    public int showX, showY;
    public int xPawn, yPawn, countCheck, countBindShape/*количество связаных фигур*/;
    public InformationForGeneralMove generalMove;
    #endregion
    #region PRIVATE
    private GameObject[] WhiteClone, BlackClone;
    private GameObject[][] cagesObj;
    private Cage[][] cages;
    private ToggleGroup tGroup;
    private bool target, blackSideTime;
    private int previousX, previousY;
    private const int size = 8;
    private Table table;
    #endregion
    private void Start()
    {
        tGroup = GetComponent<ToggleGroup>();
        table = GetComponent<Table>();
        generalMove = new InformationForGeneralMove();
        CreateTable();
    }
    public void CreateTable()//создание шахматной доски
    {
        Camera.main.backgroundColor = new Color32(220, 220, 220, 255);
        previousX = -1;
        previousY = -1;
        blackSideTime = false;
        cagesObj = new GameObject[size][];
        cages = new Cage[size][];
        for (int x = 0; x < size; x++)
        {
            cagesObj[x] = new GameObject[size];
            cages[x] = new Cage[size];
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                cagesObj[x][y] = Instantiate(CageObj, new Vector2(Poz * x, Poz * y), Quaternion.identity, transform);
                cagesObj[x][y].name = x + " " + y;
                if ((x + y) % 2 == 0)
                    cagesObj[x][y].GetComponent<Image>().color = Color.grey;
                cagesObj[x][y].GetComponent<Toggle>().group = tGroup;
                cagesObj[x][y].GetComponent<CageTrigger>().x = x;
                cagesObj[x][y].GetComponent<CageTrigger>().y = y;
                cagesObj[x][y].GetComponent<CageTrigger>().table = GetComponent<Table>();
            }
        }//доска

        whiteKing = new Vector2Int(4, 0);
        blackKing = new Vector2Int(4, 7);

        WhiteClone = new GameObject[16];
        BlackClone = new GameObject[16];
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (y < 2)
                {
                    int rank, id = x + y * size;
                    if (id < 5) rank = id;
                    else if (id < 8) rank = 7 - id;
                    else if (id < 16) rank = 5;
                    else rank = -1;
                    cages[x][y] = new Cage(x, y, Poz, cagesObj[x][y].transform.GetChild(0).gameObject, table, false, rank, id);
                    if (rank != -1)
                        WhiteClone[id] = Instantiate(White[rank], cagesObj[x][y].transform.position, Quaternion.identity, transform);

                    if (id == 3)
                        rank = 4;
                    if (id == 4)
                        rank = 3;

                    cages[7 - x][7 - y] = new Cage(7 - x, 7 - y, Poz, cagesObj[7 - x][7 - y].transform.GetChild(0).gameObject, table, true, rank, id);
                    if (rank != -1)
                        BlackClone[id] = Instantiate(Black[rank], cagesObj[7 - x][7 - y].transform.position, Quaternion.identity, transform);
                }
                else
                    cages[x][y] = new Cage(x, y, Poz, cagesObj[x][y].transform.GetChild(0).gameObject, table);
            }
        }//фигуры
    }
    public void DestroyTable()//уничтожение шахматной доски
    {
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                Destroy(cagesObj[x][y]);//доска

        for (int id = 0; id < 16; id++)
        {
            Destroy(WhiteClone[id]);
            Destroy(BlackClone[id]);
        }//фигуры
    }

    private void Clear()//очистка информации на доске
    {
        taking_a_PawnOnThePass = false;
        countBindShape = 0;
        target = false;
        tGroup.SetAllTogglesOff();
        for (int X = 0; X < 8; X++)
            for (int Y = 0; Y < 8; Y++)
                cages[X][Y].ClearCage();
    }
    public void GeneralMove()//перемещение без проверок (пока если это пустая клетка)
    {
        {

            if (blackSideTime)//ход чёрных
                BlackClone[cages[generalMove.oldX][generalMove.oldY].id].transform.position = cages[generalMove.newX][generalMove.newY].position;
            else//ход белых
                WhiteClone[cages[generalMove.oldX][generalMove.oldY].id].transform.position = cages[generalMove.newX][generalMove.newY].position;
            cages[generalMove.newX][generalMove.newY].moverRank = cages[generalMove.oldX][generalMove.oldY].rank;
            cages[generalMove.newX][generalMove.newY].moverId = cages[generalMove.oldX][generalMove.oldY].id;
            cages[generalMove.newX][generalMove.newY].MoveEnd(cages[generalMove.oldX][generalMove.oldY]);
        }//если это пустая клетка
    }
    public void GeneralDestoy(int x, int y)//уничтожение без проверок
    {
        if (cages[x][y].black)//чёрный
        {
            Destroy(BlackClone[cages[x][y].id]);
        }
        else//белый
        {
            Destroy(WhiteClone[cages[x][y].id]);
        }
        cages[x][y].id = -1;
        cages[x][y].rank = -1;
    }
    public bool OnCageIsOn(int x, int y)//при выборе клетки
    {
        if (!target)
        {
            if (cages[x][y].rank != -1 && blackSideTime == cages[x][y].black)
            {
                cages[x][y].Move(cages);
                target = true;
            }//если это фигура и цвет фигуры дружественный
            else
            {
                Clear();
            }
        }//если кем ходить не выбрали
        else
        {
            if (cages[x][y].canMove)
            {
                if (cages[x][y].rank != -1)
                {
                    if (blackSideTime != cages[x][y].black)
                    {
                        if (blackSideTime)
                        {
                            BlackClone[cages[x][y].moverId].transform.position = cages[x][y].position;
                            if (cages[cages[x][y].moverX][cages[x][y].moverY].rank == 4)
                                blackKing = new Vector2Int(x, y);
                            Destroy(WhiteClone[cages[x][y].id]);
                        }//ход чёрных
                        else
                        {
                            WhiteClone[cages[x][y].moverId].transform.position = cages[x][y].position;
                            if (cages[cages[x][y].moverX][cages[x][y].moverY].rank == 4)
                                whiteKing = new Vector2Int(x, y);
                            Destroy(BlackClone[cages[x][y].id]);
                        }//ход белых

                        return NextStep(x, y);
                    }//если цвет фигуры вражеский
                    else
                    {
                        if (previousX != -1)
                            cages[previousX][previousY].Clear(cages);
                        cages[x][y].Move(cages);
                    }//если цвет фигуры дружественный
                }//если это фигура
                else
                {
                    if (blackSideTime)//ход чёрных
                    {
                        BlackClone[cages[x][y].moverId].transform.position = cages[x][y].position;
                        if (cages[cages[x][y].moverX][cages[x][y].moverY].rank == 4)
                            blackKing = new Vector2Int(x, y);
                    }
                    else//ход белых
                    {
                        WhiteClone[cages[x][y].moverId].transform.position = cages[x][y].position;
                        if (cages[cages[x][y].moverX][cages[x][y].moverY].rank == 4)
                            whiteKing = new Vector2Int(x, y);
                    }

                    return NextStep(x, y);
                }//если это пустая клетка
            }//фигура может встать здесь
            else
            {
                target = false;
                if (previousX != -1)
                    cages[previousX][previousY].Clear(cages);
                if (cages[x][y].rank != -1 && blackSideTime == cages[x][y].black)//если это фигура и цвет фигуры дружественный
                {///не выключает клетку!!!
                    cages[x][y].Move(cages);
                    target = true;
                }
                else
                    tGroup.SetAllTogglesOff();

            }//фигура не может встать здесь
        }//если кем ходить выбрали
        previousX = x;
        previousY = y;
        return false;
    }

    private bool NextStep(int x, int y)//следующий ход
    {
        Clear();
        bool be = cages[x][y].MoveEnd(cages[cages[x][y].moverX][cages[x][y].moverY]);
        for (int X = 0; X < 8; X++)
            for (int Y = 0; Y < 8; Y++)
            {
                cages[X][Y].idBind = 0;
                cages[X][Y].killPawnInPass = false;
            }
        Check();
        blackSideTime = !blackSideTime;
        if (blackSideTime)
            Camera.main.backgroundColor = new Color32(20, 20, 20, 255);
        else
            Camera.main.backgroundColor = new Color32(220, 220, 220, 255);
        return be;
    }

    private void Check()//проверка шаха
    {
        check = false;
        countCheck = 0;
        int size = 8;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                cages[x][y].checkMove = 0;
                cages[x][y].saveCheck = false;
            }


        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                if (cages[x][y].black == blackSideTime)
                    cages[x][y].RayCheck(cages);

        if (blackSideTime)
            countCheck = cages[whiteKing.x][whiteKing.y].checkMove;
        else
            countCheck = cages[blackKing.x][blackKing.y].checkMove;
        if (countCheck > 0)
            check = true;

        if (check)
        {
            bool canSave = false;
            if (countCheck == 1)
            {
                for (int x = 0; x < size; x++)
                    for (int y = 0; y < size; y++)
                        if (cages[x][y].black != blackSideTime)
                            if (cages[x][y].RayMove(cages) > 0)
                            {
                                canSave = true;
                                break;
                            }
            }
            else
            {
                if (blackSideTime)
                {
                    if (cages[whiteKing.x][whiteKing.y].RayMove(cages) > 0)
                        canSave = true;
                }
                else
                {
                    if (cages[blackKing.x][blackKing.y].RayMove(cages) > 0)
                        canSave = true;
                }
            }
            if (!canSave)
                mate = true;
        }
        checkLabel.SetActive(check);
        mateLabel.SetActive(mate);
    }
    public void Show()//узнать информации клетки (для разработки)
    {
        showCage = cages[showX][showY];
    }
    public bool ChangeRank(int x, int y, int rank)//превращение пешки в другую фигуру
    {
        if (rank <= 3)
            cages[x][y].rank = rank;
        else
            return false;
        wait = false;
        if (cages[x][y].black)
            BlackClone[cages[x][y].id].GetComponent<Image>().sprite = sprites[rank];
        else
            WhiteClone[cages[x][y].id].GetComponent<Image>().sprite = sprites[rank + 4];
        return true;
    }
    public void OnVisualChangeRank(int x, int y)//превращение пешки в другую фигуру с визуализацией
    {
        wait = true;
        upgradePawn.OnUpgrade(x, y);
    }
}