using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// Grid hücrelerini temsil eden sınıf
/// </summary>
public class GridCell : MonoBehaviour
{
    [SerializeField] private XDrawer xDrawer;
    [SerializeField] private Transform XPivot;
    
    [Header("Animation Settings")]
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private float punchStrength = 0.5f;
    [SerializeField] private float drawCompletionTime = 0.5f;
    
    private bool hasX = false;
    private Vector2Int gridPosition;
    private GridManager gridManager;

    /// <summary>
    /// Hücreyi initialize eder
    /// </summary>
    public void Initialize(int x, int y, GridManager manager)
    {
        gridPosition = new Vector2Int(x, y);
        gridManager = manager;
    }
    
    /// <summary>
    /// Hücreye tıklandığında çalışır
    /// </summary>
    private void OnMouseDown()
    {
        if (!hasX)
        {
            hasX = true;
            xDrawer.DrawX();
            StartCoroutine(WaitForXDrawAndCheck());
        }
    }
    
    /// <summary>
    /// X çiziminin tamamlanmasını bekler ve sonra pattern kontrolü yapar
    /// </summary>
    public IEnumerator WaitForXDrawAndCheck()
    {
        // X çiziminin tamamlanması için bekle
        yield return new WaitForSeconds(drawCompletionTime);
        
        // Pattern kontrolünü başlat
        gridManager.CheckConnectedCells(gridPosition.x, gridPosition.y);
    }

    /// <summary>
    /// X işaretine punch animasyonu uygular
    /// </summary>
    public void PunchAnim()
    {
        // Eğer XPivot yoksa işlem yapma
        if (XPivot == null) return;
        
        // X işaretinin pivot objesini punch scale et
        DOTween.Kill(XPivot);
        
        // Başlangıç scale değerini kaydet
        Vector3 xInitScale = XPivot.localScale;
        
        // Punch animasyonu uygula
        XPivot.DOPunchScale(xInitScale * punchStrength, punchDuration, 0, 0)
            .OnComplete(() => {
                // Animasyon bitiminde orijinal scale'e geri dön
                XPivot.localScale = xInitScale;
            });
        
        // Aynı zamanda blink efekti uygula
        if (xDrawer != null)
        {
            xDrawer.BlinkHighlight();
        }
    }

    /// <summary>
    /// X işaretini kaldırır
    /// </summary>
    public void RemoveX()
    {
        hasX = false;
        xDrawer.ClearX();
    }

    /// <summary>
    /// Hücrenin X işaretine sahip olup olmadığını döndürür
    /// </summary>
    public bool HasX => hasX;
}
