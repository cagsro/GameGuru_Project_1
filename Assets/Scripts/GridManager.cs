using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using DG.Tweening;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int gridSize = 5;
    [SerializeField] private TMPro.TMP_InputField sizeInputField;
    
    private GridCell[,] grid;
    private int matchCount = 0;
    private PatternDetector patternDetector;
    private GridAnimator gridAnimator;
    
    public System.Action<int> OnMatchCountChanged;
    
    private void Start()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        grid = new GridCell[gridSize, gridSize];
        
        // Pattern algılayıcı ve animatör oluştur
        patternDetector = new PatternDetector(grid, gridSize);
        gridAnimator = new GridAnimator(grid);
        
        var (cellSize, spacing) = CalculateGridDimensions();
        var startPositions = CalculateStartPositions(cellSize, spacing);

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                CreateCell(x, y, cellSize, spacing, startPositions);
            }
        }
    }

    private (float cellSize, float spacing) CalculateGridDimensions()
    {
        float height = 2f * mainCam.orthographicSize;
        float width = height * mainCam.aspect;
        float totalSize = Mathf.Min(width, height) * 0.9f;
        float rawCellSize = totalSize / gridSize;
        float spacing = rawCellSize * 0.02f;
        float adjustedCellSize = (totalSize - (spacing * (gridSize - 1))) / gridSize;
        
        return (adjustedCellSize, spacing);
    }

    private (float startX, float startY) CalculateStartPositions(float cellSize, float spacing)
    {
        float totalSize = (cellSize * gridSize) + (spacing * (gridSize - 1));
        return (-totalSize / 2f, totalSize / 2f);
    }

    private void CreateCell(int x, int y, float cellSize, float spacing, (float startX, float startY) start)
    {
        float posX = start.startX + (x * (cellSize + spacing));
        float posY = start.startY - (y * (cellSize + spacing));
        Vector3 position = new Vector3(posX + cellSize/2f, posY - cellSize/2f, 0);

        float randomRotation = Random.Range(-2f, 2f);
        GameObject cellObj = Instantiate(cellPrefab, position, Quaternion.Euler(0, 0, randomRotation), transform);
        cellObj.transform.localScale = Vector3.zero;

        // Hücre bileşenini al ve initialize et
        grid[x, y] = cellObj.GetComponent<GridCell>();
        grid[x, y].Initialize(x, y, this);
        
        // Animasyon uygula
        Vector3 targetScale = new Vector3(cellSize, cellSize, 1);
        float delay = (x + y) * 0.05f;
        gridAnimator.AnimateCellCreation(cellObj, delay, targetScale);
    }

    public void CheckConnectedCells(int x, int y)
    {
        // Pattern algılama işlemini gerçekleştir
        var (cellsToRemove, allPatterns, foundMatches) = patternDetector.DetectPatterns(x, y);

        // İşaretli hücreleri silmeden önce pattern'leri sırayla vurgula
        if (allPatterns.Count > 0)
        {
            // Önce pattern'leri sırayla vurgula, sonra hücreleri sil
            gridAnimator.HighlightPatternsSequentially(allPatterns, () => {
                // Vurgulama tamamlandıktan sonra hücreleri sil
                foreach (var pos in cellsToRemove)
                {
                    if (grid[pos.x, pos.y] != null && grid[pos.x, pos.y].HasX)
                    {
                        grid[pos.x, pos.y].RemoveX();
                    }
                }

                // Match count'u güncelle
                if (foundMatches > 0)
                {
                    matchCount += foundMatches;
                    OnMatchCountChanged?.Invoke(matchCount);
                }
            });
        }
    }

    public void ResetGrid()
    {
        // Önce tüm hücreleri fade-out ile sil
        if (grid != null)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y] != null)
                    {
                        var cell = grid[x, y];
                        float delay = (x + y) * 0.02f;
                        
                        // Hücre silme animasyonu
                        gridAnimator.AnimateCellRemoval(cell.gameObject, delay);
                    }
                }
            }
        }

        // Kısa bir gecikme ile yeni grid'i oluştur
        DOVirtual.DelayedCall(0.5f, () => {
            // Reset match count
            matchCount = 0;
            OnMatchCountChanged?.Invoke(matchCount);

            // Get new size from input field
            if (int.TryParse(sizeInputField.text, out int newSize))
            {
                gridSize = Mathf.Clamp(newSize, 3, 10); // Limit size between 3 and 10
            }

            // Create new grid
            CreateGrid();
        });
    }
}
