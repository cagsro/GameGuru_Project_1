using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Pattern algılama işlemlerini yöneten sınıf.
/// GridManager'dan ayrılarak kod organizasyonu iyileştirildi.
/// </summary>
public class PatternDetector
{
    private readonly (int x, int y)[] directions = { (1,0), (-1,0), (0,1), (0,-1) };
    private readonly (int x, int y)[][] lShapes = {
        new[] { (1,0), (0,1) },   // Sağ-Yukarı
        new[] { (-1,0), (0,1) },  // Sol-Yukarı
        new[] { (1,0), (0,-1) },  // Sağ-Aşağı
        new[] { (-1,0), (0,-1) }  // Sol-Aşağı
    };
    
    private GridCell[,] grid;
    private int gridSize;
    private GridManager gridManager;
    
    public PatternDetector(GridCell[,] grid, int gridSize, GridManager gridManager)
    {
        this.grid = grid;
        this.gridSize = gridSize;
        this.gridManager = gridManager;
    }
    
    public void UpdateGrid(GridCell[,] newGrid, int newGridSize)
    {
        grid = newGrid;
        gridSize = newGridSize;
    }
    
    /// <summary>
    /// Pattern eşleşme sonuçlarını tutan yapı
    /// </summary>
    public struct MatchResult
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
    
    /// <summary>
    /// Belirtilen konumdaki hücreye bağlı tüm pattern'leri kontrol eder
    /// </summary>
    public MatchResult DetectPatterns(int x, int y)
    {
        if (!IsValidPosition(x, y) || !grid[x, y].HasX) 
            return new MatchResult(new HashSet<Vector2Int>(), new List<List<Vector2Int>>(), 0);

        var cellsToRemove = new HashSet<Vector2Int>();
        var startPoints = new List<Vector2Int>(5) { new Vector2Int(x, y) };
        int foundMatches = 0;
        
        // Tüm pattern'leri saklamak için liste
        var allPatterns = new List<List<Vector2Int>>();
        
        // Komşu noktaları ekle
        for (int i = 0; i < directions.Length; i++)
        {
            int newX = x + directions[i].x;
            int newY = y + directions[i].y;
            
            if (IsValidPosition(newX, newY) && grid[newX, newY].HasX && !gridManager.IsCellMatched(newX, newY))
            {
                startPoints.Add(new Vector2Int(newX, newY));
            }
        }

        // Her başlangıç noktası için L şekillerini kontrol et
        foreach (var point in startPoints)
        {
            var lShapePatterns = CheckLShapePatterns(point.x, point.y);
            if (lShapePatterns.Count > 0)
            {
                foundMatches += lShapePatterns.Count;
                allPatterns.AddRange(lShapePatterns);
                
                // Tüm pattern'lerdeki hücreleri silme listesine ekle
                foreach (var pattern in lShapePatterns)
                {
                    foreach (var pos in pattern)
                    {
                        cellsToRemove.Add(pos);
                    }
                }
            }
        }

        // Yatay çizgileri de kontrol et
        var horizontalPattern = CheckHorizontalLines(x, y);
        if (horizontalPattern.Count > 0)
        {
            foundMatches += 1;
            allPatterns.Add(horizontalPattern);
            
            // Hücreleri silme listesine ekle
            foreach (var pos in horizontalPattern)
            {
                cellsToRemove.Add(pos);
            }
        }
        // Dikey çizgileri de kontrol et
        var verticalPattern = CheckVerticalLines(x, y);
        if (verticalPattern.Count > 0)
        {
            foundMatches += 1;
            allPatterns.Add(verticalPattern);

            // Hücreleri silme listesine ekle
            foreach (var pos in verticalPattern)
            {
                cellsToRemove.Add(pos);
            }
        }

        return new MatchResult(cellsToRemove, allPatterns, foundMatches);
    }
    
    /// <summary>
    /// L şeklindeki pattern'leri kontrol eder
    /// </summary>
    private List<List<Vector2Int>> CheckLShapePatterns(int x, int y)
    {
        var matchedPatterns = new List<List<Vector2Int>>();
        
        for (int i = 0; i < lShapes.Length; i++)
        {
            bool isValidShape = true;
            var currentShape = new List<Vector2Int>();
            currentShape.Add(new Vector2Int(x, y));

            for (int j = 0; j < lShapes[i].Length; j++)
            {
                int newX = x + lShapes[i][j].x;
                int newY = y + lShapes[i][j].y;

                if (!IsValidPosition(newX, newY) || !grid[newX, newY].HasX || gridManager.IsCellMatched(newX, newY))
                {
                    isValidShape = false;
                    break;
                }
                currentShape.Add(new Vector2Int(newX, newY));
            }

            if (isValidShape)
            {
                matchedPatterns.Add(currentShape);
            }
        }
        
        return matchedPatterns;
    }
    
    /// <summary>
    /// Düz çizgi pattern'lerini kontrol eder
    /// </summary>
    private List<Vector2Int> CheckHorizontalLines(int x, int y)
    {
        // Yatay kontrol
        var horizontalLine = GetLine(x, y, true);
        if (horizontalLine.Count >= 3)
        {
            return horizontalLine;
        }
        return new List<Vector2Int>();
    }
    private List<Vector2Int> CheckVerticalLines(int x, int y)
    {
        // Dikey kontrol
        var verticalLine = GetLine(x, y, false);
        if (verticalLine.Count >= 3)
        {
            return verticalLine;
        }

        return new List<Vector2Int>();
    }

    /// <summary>
    /// Belirtilen konumdan başlayarak yatay veya dikey bir çizgi oluşturur
    /// </summary>
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
                if (gridManager.IsCellMatched(checkX, checkY)) break;
                
                line.Add(new Vector2Int(checkX, checkY));
            }
        }

        return line;
    }
    
    /// <summary>
    /// Belirtilen konumun grid içinde geçerli olup olmadığını kontrol eder
    /// </summary>
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    }
}
