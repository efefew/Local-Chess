using UnityEngine;

public class CageTrigger : MonoBehaviour
{
    public int x, y;
    public Table table;
    public void OnTrigger(bool on)
    {
        if(on && table && !table.wait)
            if(table.OnCageIsOn(x, y))
                table.OnVisualChangeRank(x, y);
    }
}
