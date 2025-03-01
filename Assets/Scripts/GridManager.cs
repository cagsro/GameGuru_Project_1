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
    
    // Komşu hücre yönlerini tanımla
    private readonly (int x, int y)[] neighborDirections = { (0, 0), (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, 1), (1, -1), (-1, -1) };
    
    // Eşleşen pattern'leri geçici olarak saklamak için
    private Dictionary<Vector2Int, (HashSet<Vector2Int> cellsToRemove, List<List<Vector2Int>> allPatterns, int matchCount)> matchCache = 
        new Dictionary<Vector2Int, (HashSet<Vector2Int>, List<List<Vector2Int>>, int)>();
    
    // Eşleşmiş sayılan hücreleri takip etmek için
    private HashSet<Vector2Int> matchedCells = new HashSet<Vector2Int>();
    
    public System.Action<int> OnMatchCountChanged;
    
    private void Start()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        grid = new GridCell[gridSize, gridSize];
        
        // Pattern algılayıcı ve animatör oluştur
        patternDetector = new PatternDetector(grid, gridSize, this);
        gridAnimator = new GridAnimator(grid);
        
        var (cellSize, spacing) = CalculateGridDimensions();
        var startPositions = CalculateStartPositions(cellSize, spacing);

        // Grid oluşturma sesini çal (artan pitch ile)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGridCreateSoundWithPitch(gridSize * gridSize);
        }

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

    /// <summary>
    /// Belirtilen konumun grid içinde geçerli olup olmadığını kontrol eder
    /// </summary>
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    }

    /// <summary>
    /// Belirtilen hücrenin eşleşmiş sayılıp sayılmadığını kontrol eder
    /// </summary>
    public bool IsCellMatched(int x, int y)
    {
        return matchedCells.Contains(new Vector2Int(x, y));
    }

    /// <summary>
    /// Hücreye tıklandığında pattern kontrolü yapar ve sonuçları cache'ler
    /// </summary>
    public void CheckForPotentialMatch(int x, int y)
    {
        // Pattern algılama işlemini gerçekleştir
        var result = patternDetector.DetectPatterns(x, y);
        Vector2Int cellPos = new Vector2Int(x, y);
        
        // Eğer pattern oluşmuşsa, bu pattern'i oluşturan hücreleri eşleşmiş olarak işaretle
        if (result.allPatterns.Count > 0)
        {
            foreach (var pattern in result.allPatterns)
            {
                foreach (var pos in pattern)
                {
                    matchedCells.Add(pos);
                }
            }
        }
        
        // Sonuçları cache'le
        matchCache[cellPos] = result;
    }
    
    /// <summary>
    /// X çizimi tamamlandıktan sonra eşleşen pattern'leri işler
    /// </summary>
    public void ProcessMatchedPatterns(int x, int y)
    {
        Vector2Int cellPos = new Vector2Int(x, y);
        
        // Cache'den sonuçları al
        if (matchCache.TryGetValue(cellPos, out var result))
        {
            var (cellsToRemove, allPatterns, foundMatches) = result;
            
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
                            // Eşleşmiş hücrelerden çıkar
                            matchedCells.Remove(pos);
                        }
                    }

                    // Match count'u güncelle
                    if (foundMatches > 0)
                    {
                        matchCount += foundMatches;
                        OnMatchCountChanged?.Invoke(matchCount);
                    }
                    
                    // Cache'den temizle
                    matchCache.Remove(cellPos);
                });
            }
            else
            {
                // Eşleşme yoksa sadece cache'den temizle
                matchCache.Remove(cellPos);
            }
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
        
        // Eşleşmiş hücreleri temizle
        matchedCells.Clear();
        matchCache.Clear();
        
        // Match sayacını sıfırla
        matchCount = 0;
        OnMatchCountChanged?.Invoke(matchCount);
        
        // Kısa bir gecikme ile yeni grid'i oluştur
        DOVirtual.DelayedCall(0.5f, () => {
            // Eski grid'i tamamen temizle
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            // Get new size from input field
            if (int.TryParse(sizeInputField.text, out int newSize))
            {
                gridSize = Mathf.Clamp(newSize, 3, 10); // Limit size between 3 and 10
            }
            
            // Yeni grid oluştur
            CreateGrid();
        });
    }
}
