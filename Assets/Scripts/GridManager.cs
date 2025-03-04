using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using DG.Tweening;
using System.Collections;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField, Range(3, 10)] private int gridSize = 5;
    [SerializeField] private TMPro.TMP_InputField sizeInputField;
    
    private GridCell[,] grid;
    private int matchCount = 0;
    private PatternDetector patternDetector;
    private GridAnimator gridAnimator;

    
    // Pattern eşleşme sonuçlarını tutan yapı
    private struct MatchResult
    {
        public HashSet<Vector2Int> CellsToRemove;
        public List<List<Vector2Int>> Patterns;
        public int MatchCount;

        public MatchResult(HashSet<Vector2Int> cellsToRemove, List<List<Vector2Int>> patterns, int matchCount)
        {
            CellsToRemove = cellsToRemove;
            Patterns = patterns;
            MatchCount = matchCount;
        }
    }

    // Eşleşen pattern'leri geçici olarak saklamak için
    private Dictionary<Vector2Int, MatchResult> matchCache = new Dictionary<Vector2Int, MatchResult>();
    
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

        // Grid oluşturma ses progress'ini sıfırla
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResetGridProgress();
        }

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                CreateCell(x, y, cellSize, spacing, startPositions);
            }
        }
    }

    // Grid'in ekrana sığması için boyut hesaplaması
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
        
        // Grid oluşturma sesini çal (hücre oluşturma animasyonuyla senkronize)
        float delay = (x + y) * 0.05f;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGridCreateSound(delay);
        }
        
        // Animasyon uygula
        Vector3 targetScale = new Vector3(cellSize, cellSize, 1);
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
        if (result.Patterns.Count > 0)
        {
            foreach (var pattern in result.Patterns)
            {
                foreach (var pos in pattern)
                {
                    matchedCells.Add(pos);
                }
            }
        }
        
        // Sonuçları cache'le
        matchCache[cellPos] = new MatchResult(result.CellsToRemove, result.Patterns, result.MatchCount);
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
            if (result.Patterns.Count > 0)
            {
                // Pattern'leri vurgula ve sonrasında hücreleri sil
                StartCoroutine(ProcessPatternsCoroutine(result));
            }
            
            // Cache'den temizle
            matchCache.Remove(cellPos);
        }
    }

    /// <summary>
    /// Pattern'leri sırayla işleyen coroutine
    /// </summary>
    private IEnumerator ProcessPatternsCoroutine(MatchResult result)
    {
        // Önce pattern'leri vurgula
        yield return StartCoroutine(gridAnimator.HighlightPatternsCoroutine(result.Patterns));
        
        // Sonra hücreleri sil
        foreach (var pos in result.CellsToRemove)
        {
            if (grid[pos.x, pos.y] != null && grid[pos.x, pos.y].HasX)
            {
                grid[pos.x, pos.y].RemoveX();
                matchedCells.Remove(pos);
            }
        }

        // Match count'u güncelle
        if (result.MatchCount > 0)
        {
            matchCount += result.MatchCount;
            OnMatchCountChanged?.Invoke(matchCount);
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
            // Ses progress'ini sıfırla
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ResetGridProgress();
            }
            
            // Eski grid'i tamamen temizle
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            
            // Get new size from input field
            if (int.TryParse(sizeInputField.text, out int newSize))
            {
                gridSize = Mathf.Clamp(newSize, 3, 10); // Limit size between 3 and 10
                sizeInputField.text=gridSize.ToString();
            }
            
            // Yeni grid oluştur
            CreateGrid();
        });
    }
}
