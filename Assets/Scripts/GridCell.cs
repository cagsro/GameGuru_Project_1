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
    [SerializeField] private Transform sprite;
    
    [Header("Animation Settings")]
    [SerializeField] private float clickAnimationDuration = 0.3f;
    [SerializeField] private float clickAnimationScale = 0.1f;
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
        if (CanPlaceX())
        {
            PlaceX();
        }
    }

    /// <summary>
    /// Hücreye X işareti koyulabilir mi kontrol eder
    /// </summary>
    private bool CanPlaceX()
    {
        return !hasX && !gridManager.IsCellMatched(gridPosition.x, gridPosition.y);
    }

    /// <summary>
    /// Hücreye X işareti koyar ve gerekli işlemleri yapar
    /// </summary>
    private void PlaceX()
    {
        hasX = true;
        PlayClickAnimation();
        ProcessXPlacement();
    }

    /// <summary>
    /// Tıklama animasyonunu oynatır
    /// </summary>
    private void PlayClickAnimation()
    {
        sprite.DOPunchScale(Vector3.one * clickAnimationScale, clickAnimationDuration, 0, 0f);
    }

    /// <summary>
    /// X yerleştirme işlemlerini yapar
    /// </summary>
    private void ProcessXPlacement()
    {
        // Pattern kontrolü yap ve sonuçları cache'le
        gridManager.CheckForPotentialMatch(gridPosition.x, gridPosition.y);
        
        // X çizme animasyonunu başlat
        xDrawer.DrawX();
        
        // X çizimi tamamlandıktan sonra eşleşen pattern'leri işle
        StartCoroutine(WaitForXDrawAndProcess());
    }
    
    /// <summary>
    /// X çiziminin tamamlanmasını bekler ve sonra eşleşen pattern'leri işler
    /// </summary>
    private IEnumerator WaitForXDrawAndProcess()
    {
        yield return new WaitForSeconds(drawCompletionTime);
        gridManager.ProcessMatchedPatterns(gridPosition.x, gridPosition.y);
    }

    /// <summary>
    /// X işaretine vurgu animasyonu uygular
    /// </summary>
    public void PunchAnim()
    {
        if (XPivot == null) return;
        
        PlayPunchAnimation();
        PlayBlinkEffect();
    }

    /// <summary>
    /// Punch animasyonunu oynatır
    /// </summary>
    private void PlayPunchAnimation()
    {
        DOTween.Kill(XPivot);
        Vector3 xInitScale = XPivot.localScale;
        
        XPivot.DOPunchScale(xInitScale * punchStrength, punchDuration, 0, 0)
            .OnComplete(() => XPivot.localScale = xInitScale);
    }

    /// <summary>
    /// Blink efektini oynatır
    /// </summary>
    private void PlayBlinkEffect()
    {
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
