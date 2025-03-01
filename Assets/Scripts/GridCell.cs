using DG.Tweening;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private XDrawer xDrawer;
    private bool hasX = false;
    private Vector2Int gridPosition;
    private GridManager gridManager;

    private Vector3 initScale;

    void Start()
    {

    }

    public void Initialize(int x, int y, GridManager manager)
    {
        gridPosition = new Vector2Int(x, y);
        gridManager = manager;
        initScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        if (!hasX)
        {
            hasX = true;
            xDrawer.DrawX();
            PunchAnim();
            gridManager.CheckConnectedCells(gridPosition.x, gridPosition.y);
        }
    }
    private void PunchAnim()
    {
        DOTween.Kill(transform);
        transform.localScale = initScale;
        transform.DOPunchScale(initScale * 0.1f, 0.3f, 0, 0);
    }

    public void RemoveX()
    {
        hasX = false;
        xDrawer.ClearX();
    }

    public bool HasX => hasX;
}
