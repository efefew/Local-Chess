using UnityEngine;
/// <summary>
/// Клетка
/// </summary>
public class Cage : MonoBehaviour
{
    public Vector2Int position;
    public Table table;
    /// <summary>
    /// фигура стоящая на клетке
    /// </summary>
    public Shape shape = null;
    public GameObject label;
    public void Build(Vector2Int position, Table table)
    {
        this.position = position;
        this.table = table;
        name = position.x + " " + position.y;
    }
    public void OnTrigger(bool on)
    {
        if (!on || table == null)
            return;
        table.OnCage(this);

    }
    [ContextMenu("DestroyShape")]
    public void DestroyShape()
    {
        if (!shape)
            return;
        table.shapes.Remove(shape);
        Destroy(shape.gameObject);
        shape = null;
    }
}
public struct InformationOfTheSituation
{
    public bool hints;
    public bool moving;
    /// <summary>
    /// Ходы для шаха короля
    /// </summary>
    public bool rayFindKing;
    /// <summary>
    /// Ходы при шахе
    /// </summary>
    public bool rayOnChecked;
}
