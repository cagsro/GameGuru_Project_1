using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// X işaretlerinin çizim ve animasyonlarını yöneten sınıf
/// </summary>
public class XDrawer : MonoBehaviour
{
    [SerializeField] private LineRenderer line1;
    [SerializeField] private LineRenderer line2;
    
    [Header("Line Settings")]
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = Color.red;
    [SerializeField] private float drawSpeed = 2f;
    
    [Header("Animation Settings")]
    [SerializeField] private float delayBetweenLines = 0.1f;
    [SerializeField] private AnimationCurve drawCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float blinkDuration = 0.3f;
    [SerializeField] private int blinkCount = 2;
    [SerializeField] private Ease blinkEase = Ease.InOutSine;
    [SerializeField] private bool useFlashEffect = true;

    private Coroutine currentAnimation;
    private Coroutine currentBlinkAnimation;
    private Color originalColor;
    
    // X çizimi devam ediyor mu? (sadece durum takibi için, engelleme yapmıyor)
    private bool isDrawing = false;
    
    // X çizgilerinin pozisyonları
    private readonly Vector3 line1Start = new Vector3(-0.4f, 0.4f, 0);
    private readonly Vector3 line1End = new Vector3(0.4f, -0.4f, 0);
    private readonly Vector3 line2Start = new Vector3(-0.4f, -0.4f, 0);
    private readonly Vector3 line2End = new Vector3(0.4f, 0.4f, 0);

    void Start()
    {
        InitializeLines();
    }

    /// <summary>
    /// LineRenderer'ları başlangıç ayarları ile yapılandırır
    /// </summary>
    private void InitializeLines()
    {
        if (line1 != null) ConfigureLine(line1);
        if (line2 != null) ConfigureLine(line2);
        
        // Başlangıçta çizgileri gizle
        SetLinesVisibility(false);
    }
    
    /// <summary>
    /// Bir LineRenderer'ı yapılandırır
    /// </summary>
    private void ConfigureLine(LineRenderer line)
    {
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = 2;
        line.useWorldSpace = false;
        line.sortingOrder = 1;
        
        // Gradient ile renk ayarla
        UpdateLineGradient(line, lineColor);
    }
    
    /// <summary>
    /// LineRenderer'ın gradient rengini günceller
    /// </summary>
    private void UpdateLineGradient(LineRenderer line, Color color)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        line.colorGradient = gradient;
    }
    
    /// <summary>
    /// Çizgilerin görünürlüğünü ayarlar
    /// </summary>
    private void SetLinesVisibility(bool visible)
    {
        if (line1 != null) line1.enabled = visible;
        if (line2 != null) line2.enabled = visible;
    }

    /// <summary>
    /// X işaretini çizmeye başlar
    /// </summary>
    public void DrawX()
    {
        // Eğer önceki animasyon varsa durdur
        StopCurrentAnimation();
        
        // X çizmeye başla
        currentAnimation = StartCoroutine(AnimateX());
    }
    
    /// <summary>
    /// Mevcut çizim animasyonunu durdurur
    /// </summary>
    private void StopCurrentAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
            isDrawing = false;
        }
    }

    /// <summary>
    /// X işaretini animasyonlu şekilde çizer
    /// </summary>
    private IEnumerator AnimateX()
    {
        // Çizim başladı
        isDrawing = true;
        
        // İlk çizgiyi çiz
        line1.enabled = true;
        yield return StartCoroutine(DrawLine(line1, line1Start, line1End));

        // İki çizgi arası bekleme
        yield return new WaitForSeconds(delayBetweenLines);

        // İkinci çizgiyi çiz
        line2.enabled = true;
        yield return StartCoroutine(DrawLine(line2, line2Start, line2End));

        // Çizim tamamlandı
        isDrawing = false;
        currentAnimation = null;
    }

    /// <summary>
    /// Bir çizgiyi animasyonlu şekilde çizer
    /// </summary>
    private IEnumerator DrawLine(LineRenderer line, Vector3 start, Vector3 end)
    {
        float progress = 0;
        
        while (progress <= 1.0f)
        {
            // Başlangıç noktası sabit
            line.SetPosition(0, start);
            
            // Bitiş noktasını animasyon eğrisine göre ilerlet
            float curveValue = drawCurve.Evaluate(progress);
            line.SetPosition(1, Vector3.Lerp(start, end, curveValue));
            
            progress += Time.deltaTime * drawSpeed;
            yield return null;
        }
        
        // Son pozisyonu garantile
        line.SetPosition(1, end);
    }

    /// <summary>
    /// X işaretini temizler
    /// </summary>
    public void ClearX()
    {
        // Animasyonu durdur
        StopCurrentAnimation();
        StopBlinkAnimation();

        // Çizgileri gizle
        SetLinesVisibility(false);
        
        // Çizgileri sıfırla
        ResetLinePositions();
    }
    
    /// <summary>
    /// Çizgilerin pozisyonlarını sıfırlar
    /// </summary>
    private void ResetLinePositions()
    {
        if (line1 != null)
        {
            line1.SetPosition(0, Vector3.zero);
            line1.SetPosition(1, Vector3.zero);
        }
        
        if (line2 != null)
        {
            line2.SetPosition(0, Vector3.zero);
            line2.SetPosition(1, Vector3.zero);
        }
    }

    /// <summary>
    /// X işaretinin rengini değiştirir
    /// </summary>
    public void SetColor(Color newColor)
    {
        lineColor = newColor;
        UpdateLineColors(newColor);
    }
    
    /// <summary>
    /// Tüm çizgilerin renklerini günceller
    /// </summary>
    private void UpdateLineColors(Color newColor)
    {
        if (line1 != null) UpdateLineGradient(line1, newColor);
        if (line2 != null) UpdateLineGradient(line2, newColor);
    }

    /// <summary>
    /// X işaretine vurgu animasyonu uygular
    /// </summary>
    public void BlinkHighlight()
    {
        // Eğer önceki blink animasyonu varsa durdur
        StopBlinkAnimation();
        
        // Orijinal rengi kaydet
        originalColor = lineColor;
        
        // Blink animasyonunu başlat
        currentBlinkAnimation = StartCoroutine(BlinkAnimation());
    }
    
    /// <summary>
    /// Mevcut blink animasyonunu durdurur
    /// </summary>
    private void StopBlinkAnimation()
    {
        if (currentBlinkAnimation != null)
        {
            StopCoroutine(currentBlinkAnimation);
            currentBlinkAnimation = null;
        }
    }
    
    /// <summary>
    /// X işaretine yanıp sönme animasyonu uygular
    /// </summary>
    private IEnumerator BlinkAnimation()
    {
        Sequence blinkSequence = DOTween.Sequence();
        
        for (int i = 0; i < blinkCount; i++)
        {
            // Geçici bir renk değişkeni oluştur
            Color currentColor = originalColor;
            
            // Renk değişimi animasyonu
            var colorTween = DOTween.To(
                () => currentColor,
                color => {
                    currentColor = color;
                    UpdateLineColors(color);
                },
                highlightColor,
                blinkDuration / 2
            ).SetEase(blinkEase);
            
            blinkSequence.Append(colorTween);
            
            colorTween = DOTween.To(
                () => currentColor,
                color => {
                    currentColor = color;
                    UpdateLineColors(color);
                },
                originalColor,
                blinkDuration / 2
            ).SetEase(blinkEase);
            
            blinkSequence.Append(colorTween);
            
            // Eğer flash efekti kullanılıyorsa, çizgilerin genişliğini de animasyonla değiştir
            if (useFlashEffect)
            {
                AddWidthAnimation(blinkSequence, i * blinkDuration);
            }
        }
        
        // Sequence tamamlanana kadar bekle
        yield return blinkSequence.WaitForCompletion();
        
        // Son olarak orijinal renge dön ve çizgi genişliğini sıfırla
        UpdateLineColors(originalColor);
        ResetLineWidths();
        
        currentBlinkAnimation = null;
    }
    
    /// <summary>
    /// Blink animasyonuna genişlik değişimi ekler
    /// </summary>
    private void AddWidthAnimation(Sequence sequence, float startDelay)
    {
        if (line1 == null || line2 == null) return;
        
        // Çizgi genişliğini kaydet
        float originalWidth = lineWidth;
        
        // Genişliği artır
        sequence.Join(
            DOTween.To(
                () => line1.startWidth,
                width => SetLineWidths(width),
                originalWidth * 1.5f,
                blinkDuration / 2
            ).SetEase(blinkEase).SetDelay(startDelay)
        );
        
        // Genişliği orijinal değerine geri getir
        sequence.Join(
            DOTween.To(
                () => line1.startWidth,
                width => SetLineWidths(width),
                originalWidth,
                blinkDuration / 2
            ).SetEase(blinkEase).SetDelay(startDelay + blinkDuration / 2)
        );
    }
    
    /// <summary>
    /// Tüm çizgilerin genişliğini ayarlar
    /// </summary>
    private void SetLineWidths(float width)
    {
        if (line1 != null)
        {
            line1.startWidth = width;
            line1.endWidth = width;
        }
        
        if (line2 != null)
        {
            line2.startWidth = width;
            line2.endWidth = width;
        }
    }
    
    /// <summary>
    /// Çizgilerin genişliklerini orijinal değerlerine sıfırlar
    /// </summary>
    private void ResetLineWidths()
    {
        SetLineWidths(lineWidth);
    }
    
    /// <summary>
    /// X çiziminin devam edip etmediğini kontrol eder
    /// </summary>
    public bool IsDrawing()
    {
        return isDrawing;
    }
}
