using UnityEngine;
using System.Collections.Generic;
using System;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Grid animasyonlarını yöneten sınıf.
/// GridManager'dan ayrılarak kod organizasyonu iyileştirildi.
/// </summary>
public class GridAnimator
{
    private GridCell[,] grid;
    
    // Animasyon ayarları
    private const float CELL_CREATION_DURATION = 0.3f;
    private const float CELL_REMOVAL_DURATION = 0.2f;
    private const float PATTERN_HIGHLIGHT_DURATION = 0.3f;
    private const float PATTERN_HIGHLIGHT_DELAY = 0.2f;
    private const float INITIAL_HIGHLIGHT_DELAY = 0.1f;
    
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
        var cellRenderer = cellObj.GetComponent<SpriteRenderer>();
        SetInitialCellState(cellRenderer);
        AnimateCellAppearance(cellObj, cellRenderer, delay, targetScale);
    }

    /// <summary>
    /// Hücrenin başlangıç durumunu ayarlar
    /// </summary>
    private void SetInitialCellState(SpriteRenderer cellRenderer)
    {
        if (cellRenderer != null)
        {
            Color startColor = cellRenderer.color;
            startColor.a = 0f;
            cellRenderer.color = startColor;
        }
    }

    /// <summary>
    /// Hücre görünüm animasyonlarını başlatır
    /// </summary>
    private void AnimateCellAppearance(GameObject cellObj, SpriteRenderer cellRenderer, float delay, Vector3 targetScale)
    {
        // Scale animasyonu
        cellObj.transform.DOScale(targetScale, CELL_CREATION_DURATION)
            .SetDelay(delay)
            .SetEase(Ease.OutBack);

        // Fade animasyonu
        if (cellRenderer != null)
        {
            cellRenderer.DOFade(1f, CELL_CREATION_DURATION)
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
        AnimateCellDisappearance(cellObj, cellRenderer, delay, onComplete);
    }

    /// <summary>
    /// Hücre kaybolma animasyonlarını başlatır
    /// </summary>
    private void AnimateCellDisappearance(GameObject cellObj, SpriteRenderer cellRenderer, float delay, Action onComplete)
    {
        // Fade-out animasyonu
        if (cellRenderer != null)
        {
            cellRenderer.DOFade(0f, CELL_REMOVAL_DURATION).SetDelay(delay);
        }
        
        // Scale animasyonu
        cellObj.transform.DOScale(Vector3.zero, CELL_REMOVAL_DURATION)
            .SetDelay(delay)
            .OnComplete(() => {
                if (onComplete != null)
                {
                    onComplete();
                }
                else
                {
                    UnityEngine.Object.Destroy(cellObj);
                }
            });
    }
    
    /// <summary>
    /// Pattern'leri sırayla vurgular
    /// </summary>
    public IEnumerator HighlightPatternsCoroutine(List<List<Vector2Int>> patterns)
    {
        if (patterns == null || patterns.Count == 0)
            yield break;

        // Her pattern için highlight işlemini yap
        foreach (var pattern in patterns)
        {
            // İlk bekleme
            yield return new WaitForSeconds(INITIAL_HIGHLIGHT_DELAY);
            
            // Pattern'i vurgula
            HighlightPattern(pattern);
            
            // Pattern arası bekleme
            yield return new WaitForSeconds(PATTERN_HIGHLIGHT_DURATION + PATTERN_HIGHLIGHT_DELAY);
        }
    }

    /// <summary>
    /// Tek bir pattern'i vurgular
    /// </summary>
    private void HighlightPattern(List<Vector2Int> pattern)
    {
        PlayPatternCompleteSound();
        
        foreach (var pos in pattern)
        {
            if (IsValidPosition(pos.x, pos.y) && grid[pos.x, pos.y] != null)
            {
                grid[pos.x, pos.y].PunchAnim();
            }
        }
    }

    /// <summary>
    /// Pattern tamamlanma sesini çalar
    /// </summary>
    private void PlayPatternCompleteSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCompleteSound();
        }
    }
    
    /// <summary>
    /// Belirtilen konumun grid içinde geçerli olup olmadığını kontrol eder
    /// </summary>
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
    }
}
