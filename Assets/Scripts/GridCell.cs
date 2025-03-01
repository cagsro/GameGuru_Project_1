using DG.Tweening;
using System.Collections;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    [SerializeField] private XDrawer xDrawer;
    [SerializeField] private Transform XPivot;
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
            StartCoroutine(WaitForXDrawAndCheck());
        }
    }
    
    public IEnumerator WaitForXDrawAndCheck()
    {
        // X çiziminin tamamlanması için yaklaşık süre (iki çizgi + aralarındaki bekleme)
        float drawTime = 0.5f;
        yield return new WaitForSeconds(drawTime);
        
        // X çizimi tamamlandıktan sonra punch animasyonu yap
        //PunchAnim();
        
        // Pattern kontrolünü başlat
        gridManager.CheckConnectedCells(gridPosition.x, gridPosition.y);
    }

    public void PunchAnim()
    {
        // Eğer XPivot yoksa işlem yapma
        if (XPivot == null) return;
        
        // X işaretinin pivot objesini punch scale et
        DOTween.Kill(XPivot);
        
        // Başlangıç scale değerini kaydet
        Vector3 xInitScale = XPivot.localScale;
        
        // Punch animasyonu uygula
        XPivot.DOPunchScale(xInitScale * 0.5f, 0.3f, 0, 0).OnComplete(() => {
            // Animasyon bitiminde orijinal scale'e geri dön
            XPivot.localScale = xInitScale;
        });
        
        // Aynı zamanda blink efekti uygula
        if (xDrawer != null)
        {
            xDrawer.BlinkHighlight();
        }
    }

    public void RemoveX()
    {
        hasX = false;
        xDrawer.ClearX();
    }

    public bool HasX => hasX;
}
