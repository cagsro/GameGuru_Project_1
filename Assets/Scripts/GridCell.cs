using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private XDrawer xDrawer;
    private bool hasX = false;
    private Vector2Int gridPosition;
    private GridManager gridManager;

    void Start()
    {

    }

    public void Initialize(int x, int y, GridManager manager)
    {
        gridPosition = new Vector2Int(x, y);
        gridManager = manager;
    }

    private void OnMouseDown()
    {
        if (!hasX)
        {
            hasX = true;
            xDrawer.DrawX();
            gridManager.CheckConnectedCells(gridPosition.x, gridPosition.y);
        }
    }

    public void RemoveX()
    {
        hasX = false;
        xDrawer.ClearX();
    }

    public bool HasX => hasX;
}
