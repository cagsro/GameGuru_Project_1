using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;

/// <summary>
/// Grid animasyonlarını yöneten sınıf.
/// GridManager'dan ayrılarak kod organizasyonu iyileştirildi.
/// </summary>
public class GridAnimator
{
    private GridCell[,] grid;
    
    public GridAnimator(GridCell[,] grid)
    {
        this.grid = grid;
    }
    
    public void UpdateGrid(GridCell[,] newGrid)
    {
        grid = newGrid;
    }
    
    /// <summary>
    /// Hücre oluşturma animasyonunu gerçekleştirir
    /// </summary>
    public void AnimateCellCreation(GameObject cellObj, float delay, Vector3 targetScale)
    {
        // Görünmez başla
        var cellRenderer = cellObj.GetComponent<SpriteRenderer>();
        if (cellRenderer != null)
        {
            Color startColor = cellRenderer.color;
            startColor.a = 0f;
            cellRenderer.color = startColor;
        }
        
        // Scale ve fade animasyonları
        cellObj.transform.DOScale(targetScale, 0.3f)
            .SetDelay(delay)
            .SetEase(Ease.OutBack);

        if (cellRenderer != null)
        {
            cellRenderer.DOFade(1f, 0.3f)
                .SetDelay(delay)
                .SetEase(Ease.OutCubic);
        }
    }
    
    /// <summary>
    /// Hücre silme animasyonunu gerçekleştirir
    /// </summary>
    public void AnimateCellRemoval(GameObject cellObj, float delay, Action onComplete = null)
    {
        var cellRenderer = cellObj.GetComponent<SpriteRenderer>();
        
        // Fade-out ve scale animasyonu
        if (cellRenderer != null)
        {
            cellRenderer.DOFade(0f, 0.2f).SetDelay(delay);
        }
        
        cellObj.transform.DOScale(Vector3.zero, 0.2f)
            .SetDelay(delay)
            .OnComplete(() => {
                if (onComplete != null)
                    onComplete();
                else
                    UnityEngine.Object.Destroy(cellObj);
            });
    }
    
    /// <summary>
    /// Pattern'leri sırayla vurgular
    /// </summary>
    public void HighlightPatternsSequentially(List<List<Vector2Int>> patterns, Action onComplete)
    {
        // Eğer pattern yoksa direkt tamamla
        if (patterns.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }
        
        // Her pattern için animasyon süresi ve aralarındaki gecikme
        float animDuration = 0.3f;
        float delayBetweenPatterns = 0.2f;
        
        // Sırayla pattern'leri vurgula
        DOVirtual.DelayedCall(0.1f, () => {
            HighlightPatternSequence(patterns, 0, animDuration, delayBetweenPatterns, onComplete);
        });
    }
    
    /// <summary>
    /// Pattern'leri sırayla vurgular (recursive)
    /// </summary>
    private void HighlightPatternSequence(List<List<Vector2Int>> patterns, int currentIndex, float animDuration, float delay, Action onComplete)
    {
        // Tüm pattern'ler tamamlandıysa bitir
        if (currentIndex >= patterns.Count)
        {
            onComplete?.Invoke();
            return;
        }
        
        // Mevcut pattern'i vurgula
        var currentPattern = patterns[currentIndex];
        
        // Her pattern için tamamlanma sesini çal
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCompleteSound();
        }
        
        // Pattern'deki tüm hücreleri vurgula
        foreach (var pos in currentPattern)
        {
            if (IsValidPosition(pos.x, pos.y) && grid[pos.x, pos.y] != null)
            {
                // X işaretine punch animasyonu uygula
                grid[pos.x, pos.y].PunchAnim();
            }
        }
        
        // Sonraki pattern'e geç
        DOVirtual.DelayedCall(animDuration + delay, () => {
            HighlightPatternSequence(patterns, currentIndex + 1, animDuration, delay, onComplete);
        });
    }
    
    /// <summary>
    /// Belirtilen konumun grid içinde geçerli olup olmadığını kontrol eder
    /// </summary>
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
    }
}
