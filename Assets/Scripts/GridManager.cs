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
    private readonly (int x, int y)[] directions = { (1,0), (-1,0), (0,1), (0,-1) };
    private readonly (int x, int y)[][] lShapes = {
        new[] { (1,0), (0,1) },   // Sağ-Yukarı
        new[] { (-1,0), (0,1) },  // Sol-Yukarı
        new[] { (1,0), (0,-1) },  // Sağ-Aşağı
        new[] { (-1,0), (0,-1) }  // Sol-Aşağı
    };
    private int matchCount = 0;
    public System.Action<int> OnMatchCountChanged;
    
    private void Start()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        grid = new GridCell[gridSize, gridSize];
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

        // Görünmez başla
        var cellRenderer = cellObj.GetComponent<SpriteRenderer>();
        if (cellRenderer != null)
        {
            Color startColor = cellRenderer.color;
            startColor.a = 0f;
            cellRenderer.color = startColor;
        }

        // Animasyon gecikmesi hesapla (soldan sağa ve yukarıdan aşağıya doğru artan gecikme)
        float delay = (x + y) * 0.05f;

        // Scale ve fade animasyonları
        cellObj.transform.DOScale(new Vector3(cellSize, cellSize, 1), 0.3f)
            .SetDelay(delay)
            .SetEase(Ease.OutBack);

        if (cellRenderer != null)
        {
            cellRenderer.DOFade(1f, 0.3f)
                .SetDelay(delay)
                .SetEase(Ease.OutCubic);
        }

        grid[x, y] = cellObj.GetComponent<GridCell>();
        grid[x, y].Initialize(x, y, this);
    }

    public void CheckConnectedCells(int x, int y)
    {
        if (!grid[x, y].HasX) return;

        var cellsToRemove = new HashSet<Vector2Int>();
        var startPoints = new List<Vector2Int>(5) { new Vector2Int(x, y) };
        int foundMatches = 0;
        
        // Komşu noktaları ekle
        for (int i = 0; i < directions.Length; i++)
        {
            int newX = x + directions[i].x;
            int newY = y + directions[i].y;
            
            if (IsValidPosition(newX, newY) && grid[newX, newY].HasX)
            {
                startPoints.Add(new Vector2Int(newX, newY));
            }
        }

        // Her başlangıç noktası için L şekillerini kontrol et
        foreach (var point in startPoints)
        {
            int lShapeMatches = CheckPatterns(point.x, point.y, cellsToRemove);
            if (lShapeMatches > 0)
            {
                foundMatches += lShapeMatches;
            }
        }

        // Düz çizgileri de kontrol et
        int straightMatches = CheckStraightLines(x, y, cellsToRemove);
        if (straightMatches > 0)
        {
            foundMatches += straightMatches;
        }

        // İşaretli hücreleri sil
        if (cellsToRemove.Count > 0)
        {
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
        }
    }

    private int CheckPatterns(int x, int y, HashSet<Vector2Int> cellsToRemove)
    {
        int matchesFound = 0;
        
        for (int i = 0; i < lShapes.Length; i++)
        {
            bool isValidShape = true;
            var currentShape = new List<Vector2Int>();
            currentShape.Add(new Vector2Int(x, y));

            for (int j = 0; j < lShapes[i].Length; j++)
            {
                int newX = x + lShapes[i][j].x;
                int newY = y + lShapes[i][j].y;

                if (!IsValidPosition(newX, newY) || !grid[newX, newY].HasX)
                {
                    isValidShape = false;
                    break;
                }
                currentShape.Add(new Vector2Int(newX, newY));
            }

            if (isValidShape)
            {
                matchesFound++;
                foreach (var pos in currentShape)
                {
                    if (!cellsToRemove.Contains(pos))
                    {
                        cellsToRemove.Add(pos);
                    }
                }
            }
        }
        
        return matchesFound;
    }

    private int CheckStraightLines(int x, int y, HashSet<Vector2Int> cellsToRemove)
    {
        // Yatay kontrol
        var horizontalLine = GetLine(x, y, true);
        if (horizontalLine.Count >= 3)
        {
            foreach (var pos in horizontalLine)
            {
                if (!cellsToRemove.Contains(pos))
                {
                    cellsToRemove.Add(pos);
                }
            }
            return 1;
        }

        // Yatay match bulunamadıysa dikey kontrol
        var verticalLine = GetLine(x, y, false);
        if (verticalLine.Count >= 3)
        {
            foreach (var pos in verticalLine)
            {
                if (!cellsToRemove.Contains(pos))
                {
                    cellsToRemove.Add(pos);
                }
            }
            return 1;
        }

        return 0;
    }

    private List<Vector2Int> GetLine(int x, int y, bool isHorizontal)
    {
        var line = new List<Vector2Int>(gridSize) { new Vector2Int(x, y) };
        int[] directions = { 1, -1 };

        for (int d = 0; d < 2; d++)
        {
            int dir = directions[d];
            int current = isHorizontal ? x : y;
            
            while (true)
            {
                current += dir;
                int checkX = isHorizontal ? current : x;
                int checkY = isHorizontal ? y : current;
                
                if (!IsValidPosition(checkX, checkY)) break;
                if (!grid[checkX, checkY].HasX) break;
                
                line.Add(new Vector2Int(checkX, checkY));
            }
        }

        return line;
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
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
                        var cellRenderer = cell.GetComponent<SpriteRenderer>();
                        float delay = (x + y) * 0.02f;

                        // Fade-out ve scale animasyonu
                        if (cellRenderer != null)
                        {
                            cellRenderer.DOFade(0f, 0.2f).SetDelay(delay);
                        }
                        
                        cell.transform.DOScale(Vector3.zero, 0.2f)
                            .SetDelay(delay)
                            .OnComplete(() => Destroy(cell.gameObject));
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
