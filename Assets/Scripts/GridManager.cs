using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int gridSize = 5;
    
    private GridCell[,] grid;
    private readonly (int x, int y)[] directions = { (1,0), (-1,0), (0,1), (0,-1) };
    private readonly (int x, int y)[][] lShapes = {
        new[] { (1,0), (0,1) },   // Sağ-Yukarı
        new[] { (-1,0), (0,1) },  // Sol-Yukarı
        new[] { (1,0), (0,-1) },  // Sağ-Aşağı
        new[] { (-1,0), (0,-1) }  // Sol-Aşağı
    };
    
    private void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
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

        GameObject cellObj = Instantiate(cellPrefab, position, Quaternion.identity, transform);
        cellObj.transform.localScale = new Vector3(cellSize, cellSize, 1);

        grid[x, y] = cellObj.GetComponent<GridCell>();
        grid[x, y].Initialize(x, y, this);
    }

    public void CheckConnectedCells(int x, int y)
    {
        if (!grid[x, y].HasX) return;

        var cellsToRemove = new HashSet<Vector2Int>();
        var startPoints = new List<Vector2Int>(5) { new Vector2Int(x, y) };
        
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

        // Her başlangıç noktası için şekilleri kontrol et
        for (int i = 0; i < startPoints.Count; i++)
        {
            CheckPatterns(startPoints[i].x, startPoints[i].y, cellsToRemove);
        }

        // İşaretli hücreleri sil
        var positions = cellsToRemove.ToArray();
        for (int i = 0; i < positions.Length; i++)
        {
            grid[positions[i].x, positions[i].y].RemoveX();
        }
    }

    private void CheckPatterns(int x, int y, HashSet<Vector2Int> cellsToRemove)
    {
        CheckStraightLines(x, y, cellsToRemove);
        CheckLShapes(x, y, cellsToRemove);
    }

    private void CheckStraightLines(int x, int y, HashSet<Vector2Int> cellsToRemove)
    {
        // Yatay ve dikey kontrol
        for (int i = 0; i < 2; i++)
        {
            bool isHorizontal = i == 0;
            var line = GetLine(x, y, isHorizontal);
            if (line.Count >= 3)
            {
                for (int j = 0; j < line.Count; j++)
                {
                    cellsToRemove.Add(line[j]);
                }
            }
        }
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

    private void CheckLShapes(int x, int y, HashSet<Vector2Int> cellsToRemove)
    {
        if (!grid[x, y].HasX) return;

        for (int i = 0; i < lShapes.Length; i++)
        {
            var shape = lShapes[i];
            int x1 = x + shape[0].x;
            int y1 = y + shape[0].y;
            int x2 = x + shape[1].x;
            int y2 = y + shape[1].y;

            if (IsValidPosition(x1, y1) && IsValidPosition(x2, y2) && 
                grid[x1, y1].HasX && grid[x2, y2].HasX)
            {
                cellsToRemove.Add(new Vector2Int(x, y));
                cellsToRemove.Add(new Vector2Int(x1, y1));
                cellsToRemove.Add(new Vector2Int(x2, y2));
            }
        }
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridSize && y >= 0 && y < gridSize;
    }
}
