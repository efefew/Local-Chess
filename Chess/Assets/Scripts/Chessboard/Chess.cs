using UnityEngine;
using UnityEngine.UI;

public class Chess : MonoBehaviour
{
    public const int size = 8;
    public const float scalePosition = 0.5208333f;

    public GameObject checkLabel, mateLabel, upgradePawn;
    public ToggleGroup group { get; private set; }
    private Table realTable;
    public Table tablePrefab;
    public Cage whiteCagePrefab, blackCagePrefab;
    public Shape[] whiteShapePrefabs, blackShapePrefabs;
    private void Awake()
    {
        group = GetComponent<ToggleGroup>();
    }
    private void OnEnable()
    {
        CreateTable();
    }

    private void CreateTable()
    {
        realTable = Instantiate(tablePrefab, transform);
        realTable.CreateTable(this, true);
    }

    public void UpgradePawn(int idUpgrade)
    {
        upgradePawn.SetActive(false);
        realTable.OnChoosingPawnUpgrade(idUpgrade);
    }
    public void Restart()
    {
        DestroyTable();
        CreateTable();
    }
    private void OnDisable()
    {
        DestroyTable();
    }

    private void DestroyTable()
    {
        Destroy(realTable.gameObject);
        realTable = null;
    }

    private void Update()
    {
        if (!realTable)
            return;
        checkLabel.SetActive(realTable.countCheckKing > 0);
        mateLabel.SetActive(realTable.mate);
    }

}
