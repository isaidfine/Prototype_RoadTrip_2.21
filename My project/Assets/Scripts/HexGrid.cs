using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Sprite unexploredSprite;
    [SerializeField] private Sprite exploredSprite;
    
    private GridCell[,] grid;
    public const int WIDTH = 20;
    public const int HEIGHT = 100;
    
    // 修改单元格大小为精确的1单位
    private const float CELL_SIZE = 1.0f;

    private void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new GridCell[WIDTH, HEIGHT];

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                CreateCell(x, y);
            }
        }
    }

    private void CreateCell(int x, int y)
    {
        Vector3 position = CalculateGridPosition(x, y);
        GameObject cellObject = Instantiate(cellPrefab, position, Quaternion.identity, transform);
        
        // 确保sprite大小正确
        cellObject.transform.localScale = new Vector3(CELL_SIZE, CELL_SIZE, 1f);
        
        GridCell cell = cellObject.GetComponent<GridCell>();
        cell.Initialize(new Vector2Int(x, y), unexploredSprite, exploredSprite);
        
        grid[x, y] = cell;
    }

    private Vector3 CalculateGridPosition(int x, int y)
    {
        float xPos = x * CELL_SIZE;
        float yPos = y * CELL_SIZE;
        return new Vector3(xPos, yPos, 0);
    }

    public void ExploreArea(Vector2Int center, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int checkX = center.x + x;
                int checkY = center.y + y;

                if (IsValidCoordinate(checkX, checkY))
                {
                    grid[checkX, checkY].SetExplored();
                }
            }
        }
    }

    private bool IsValidCoordinate(int x, int y)
    {
        return x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT;
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / CELL_SIZE);
        int y = Mathf.RoundToInt(worldPosition.y / CELL_SIZE);
        return new Vector2Int(x, y);
    }

    public GridCell GetCellAt(Vector2Int position)
    {
        if (IsValidCoordinate(position.x, position.y))
        {
            return grid[position.x, position.y];
        }
        return null;
    }

    // 添加公共方法让格子可以被查询探索状态
    public bool IsCellExplored(Vector2Int position)
    {
        GridCell cell = GetCellAt(position);
        return cell != null && cell.IsExplored();
    }
} 