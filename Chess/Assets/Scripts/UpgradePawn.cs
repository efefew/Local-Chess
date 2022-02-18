using UnityEngine;

public class UpgradePawn : MonoBehaviour
{
    public GameObject g;
    public Table table;
    int x, y;
    public void OnUpgrade(int x, int y)
    {
        this.x = x;
        this.y = y;
        g.SetActive(true);
    }
    public void OnChoosedUpgrade(int rank)
    {
        if (table.ChangeRank(x, y, rank))
            g.SetActive(false);
    }
}
