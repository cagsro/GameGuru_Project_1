using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Kullanıcı arayüzü elemanlarını yöneten sınıf
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TextMeshProUGUI matchCountText;
    
    [Header("Animation Settings")]
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private float punchStrength = 0.1f;
    [SerializeField] private int vibrato = 0;
    [SerializeField] private float elasticity = 0;

    private void Start()
    {
        InitializeUI();
    }

    /// <summary>
    /// UI elemanlarını başlatır ve event listener'ları ekler
    /// </summary>
    private void InitializeUI()
    {
        // Reset butonuna tıklama olayını ekle
        resetButton.onClick.AddListener(OnResetButtonClick);
        
        // Match count değişikliğini dinle
        gridManager.OnMatchCountChanged += UpdateMatchCountText;
        
        // Başlangıç değerini ayarla
        UpdateMatchCountText(0);
    }

    private void OnDestroy()
    {
        // Event listener'ları temizle
        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetButtonClick);
            
        if (gridManager != null)
            gridManager.OnMatchCountChanged -= UpdateMatchCountText;
    }

    /// <summary>
    /// Match count metnini günceller ve animasyon uygular
    /// </summary>
    private void UpdateMatchCountText(int count)
    {
        // Metni güncelle
        matchCountText.text = $"MATCH COUNT: {count}";
        
        // Animasyon uygula
        AnimatePunchScale(matchCountText.transform);
    }

    /// <summary>
    /// Reset butonuna tıklandığında çalışır
    /// </summary>
    private void OnResetButtonClick()
    {
        // Buton animasyonu
        AnimatePunchScale(resetButton.transform);
        
        // Grid'i sıfırla
        gridManager.ResetGrid();
    }

    /// <summary>
    /// Belirtilen transform'a punch scale animasyonu uygular
    /// </summary>
    private void AnimatePunchScale(Transform target)
    {
        // Önceki animasyonu durdur
        DOTween.Kill(target);
        
        // Scale'i sıfırla ve animasyonu başlat
        target.localScale = Vector3.one;
        target.DOPunchScale(Vector3.one * punchStrength, punchDuration, vibrato, elasticity);
    }
}
