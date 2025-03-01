using UnityEngine;
using System.Collections;
using DG.Tweening;

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

    void Start()
    {
        SetupLine(line1);
        SetupLine(line2);

        // Başlangıçta çizgileri gizle
        line1.enabled = false;
        line2.enabled = false;
    }

    void SetupLine(LineRenderer line)
    {
        // Material oluştur ve rengi ayarla
        //Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        //lineMaterial.color = lineColor;
        
        //line.material = lineMaterial;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = 2;
        line.useWorldSpace = false;
        line.sortingOrder = 1;

        // Gradient ile renk ayarla
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(lineColor, 0.0f), new GradientColorKey(lineColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        line.colorGradient = gradient;
    }

    public void DrawX()
    {
        // Eğer önceki animasyon varsa durdur
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(AnimateX());
    }
    
    private void StopCurrentAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }

    IEnumerator AnimateX()
    {
        // İlk çizgi için pozisyonlar (sol üstten sağ alta)
        Vector3 line1Start = new Vector3(-0.4f, 0.4f, 0);
        Vector3 line1End = new Vector3(0.4f, -0.4f, 0);

        // İkinci çizgi için pozisyonlar (sol alttan sağ üste)
        Vector3 line2Start = new Vector3(-0.4f, -0.4f, 0);
        Vector3 line2End = new Vector3(0.4f, 0.4f, 0);

        // İlk çizgiyi çiz
        line1.enabled = true;
        yield return StartCoroutine(DrawLine(line1, line1Start, line1End));

        // İki çizgi arası bekleme
        yield return new WaitForSeconds(delayBetweenLines);

        // İkinci çizgiyi çiz
        line2.enabled = true;
        yield return StartCoroutine(DrawLine(line2, line2Start, line2End));

        currentAnimation = null;
    }

    IEnumerator DrawLine(LineRenderer line, Vector3 start, Vector3 end)
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

    public void ClearX()
    {
        // Animasyonu durdur
        StopCurrentAnimation();

        // Çizgileri gizle
        if (line1 != null)
        {
            line1.enabled = false;
            // Çizgiyi sıfırla
            line1.SetPosition(0, Vector3.zero);
            line1.SetPosition(1, Vector3.zero);
        }
        
        if (line2 != null)
        {
            line2.enabled = false;
            // Çizgiyi sıfırla
            line2.SetPosition(0, Vector3.zero);
            line2.SetPosition(1, Vector3.zero);
        }
    }

    public void SetColor(Color newColor)
    {
        lineColor = newColor;
        if (line1 != null && line2 != null)
        {
            // Gradient'i güncelle
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(newColor, 0.0f), new GradientColorKey(newColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
            
            line1.colorGradient = gradient;
            line2.colorGradient = gradient;
        }
    }

    public void BlinkHighlight()
    {
        // Eğer önceki blink animasyonu varsa durdur
        StopBlinkAnimation();
        
        // Orijinal rengi kaydet
        originalColor = lineColor;
        
        // Blink animasyonunu başlat
        currentBlinkAnimation = StartCoroutine(BlinkAnimation());
    }
    
    private void StopBlinkAnimation()
    {
        if (currentBlinkAnimation != null)
        {
            StopCoroutine(currentBlinkAnimation);
            currentBlinkAnimation = null;
        }
    }
    
    private IEnumerator BlinkAnimation()
    {
        Sequence blinkSequence = DOTween.Sequence();
        
        for (int i = 0; i < blinkCount; i++)
        {
            // Geçici bir renk değişkeni oluştur
            Color currentColor = originalColor;
            
            // Orijinal renkten highlight rengine
            blinkSequence.Append(
                DOTween.To(
                    () => currentColor,
                    color => {
                        currentColor = color;
                        UpdateLineColors(color);
                    },
                    highlightColor,
                    blinkDuration / 2
                ).SetEase(blinkEase)
            );
            
            // Highlight renginden orijinal renge
            blinkSequence.Append(
                DOTween.To(
                    () => currentColor,
                    color => {
                        currentColor = color;
                        UpdateLineColors(color);
                    },
                    originalColor,
                    blinkDuration / 2
                ).SetEase(blinkEase)
            );
            
            // Eğer flash efekti kullanılıyorsa, çizgilerin genişliğini de animasyonla değiştir
            if (useFlashEffect && line1 != null && line2 != null)
            {
                // Çizgi genişliğini kaydet
                float originalWidth = line1.startWidth;
                
                // Genişliği artır
                blinkSequence.Join(
                    DOTween.To(
                        () => line1.startWidth,
                        width => {
                            line1.startWidth = width;
                            line1.endWidth = width;
                            line2.startWidth = width;
                            line2.endWidth = width;
                        },
                        originalWidth * 1.5f,
                        blinkDuration / 2
                    ).SetEase(blinkEase)
                );
                
                // Genişliği orijinal değerine geri getir
                blinkSequence.Join(
                    DOTween.To(
                        () => line1.startWidth,
                        width => {
                            line1.startWidth = width;
                            line1.endWidth = width;
                            line2.startWidth = width;
                            line2.endWidth = width;
                        },
                        originalWidth,
                        blinkDuration / 2
                    ).SetEase(blinkEase).SetDelay(blinkDuration / 2)
                );
            }
        }
        
        // Sequence tamamlanana kadar bekle
        yield return blinkSequence.WaitForCompletion();
        
        // Son olarak orijinal renge dön ve çizgi genişliğini sıfırla
        UpdateLineColors(originalColor);
        if (useFlashEffect && line1 != null && line2 != null)
        {
            line1.startWidth = lineWidth;
            line1.endWidth = lineWidth;
            line2.startWidth = lineWidth;
            line2.endWidth = lineWidth;
        }
        
        currentBlinkAnimation = null;
    }
    
    private void UpdateLineColors(Color newColor)
    {
        if (line1 != null && line2 != null)
        {
            // Gradient'i güncelle
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(newColor, 0.0f), new GradientColorKey(newColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
            
            line1.colorGradient = gradient;
            line2.colorGradient = gradient;
        }
    }
}
