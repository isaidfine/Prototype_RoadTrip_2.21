using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GridManager gridManager;
    private TownGenerator townGenerator;
    private Vector2Int currentGridPosition;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int exploreRadius = 3;

    private Vector3 targetPosition;
    private bool isMoving = false;

    private void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        townGenerator = FindFirstObjectByType<TownGenerator>();
        
        currentGridPosition = new Vector2Int(10, 0);
        transform.position = CalculateWorldPosition(currentGridPosition);
        targetPosition = transform.position;
        
        ExploreCurrentArea();
    }

    private void Update()
    {
        if (isMoving)
        {
            // 如果正在移动，继续移动到目标位置
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // 检查是否到达目标位置
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
        else
        {
            // 如果没有在移动，检查输入
            HandleInput();
        }

        ExploreCurrentArea();
    }

    private void HandleInput()
    {
        Vector2Int newPosition = currentGridPosition;

        if (Input.GetKeyDown(KeyCode.W))
            newPosition.y += 1;
        if (Input.GetKeyDown(KeyCode.S))
            newPosition.y -= 1;
        if (Input.GetKeyDown(KeyCode.A))
            newPosition.x -= 1;
        if (Input.GetKeyDown(KeyCode.D))
            newPosition.x += 1;

        if (newPosition != currentGridPosition && IsValidMove(newPosition))
        {
            currentGridPosition = newPosition;
            targetPosition = CalculateWorldPosition(currentGridPosition);
            isMoving = true;
        }
    }

    private bool IsValidMove(Vector2Int position)
    {
        return townGenerator != null && townGenerator.IsPositionInBounds(position);
    }

    private Vector3 CalculateWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x, gridPosition.y, 0);
    }

    private void ExploreCurrentArea()
    {
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y)
        );
        
        // 同时更新两个系统
        if (gridManager != null)
        {
            gridManager.ExploreArea(currentPos, exploreRadius);
        }
        
        if (townGenerator != null)
        {
            townGenerator.ExploreArea(currentPos, exploreRadius);
        }
    }
} 