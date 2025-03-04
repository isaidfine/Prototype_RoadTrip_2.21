using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private GridManager gridManager;
    private TownGenerator townGenerator;
    private PlayerResources playerResources;
    private Vector2Int currentGridPosition;
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int exploreRadius = 3;

    private Vector3 targetPosition;
    private bool isMoving = false;

    public Vector2Int CurrentGridPosition => currentGridPosition;

    private void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        townGenerator = FindFirstObjectByType<TownGenerator>();
        playerResources = GetComponent<PlayerResources>();
        
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
                
                // 移动完成后，根据当前格子类型消耗资源
                ConsumeResourceBasedOnCellType();
            }
        }
        else
        {
            // 按空格消耗疲劳值（模拟投骰子）
            if (Input.GetKeyDown(KeyCode.Space) && playerResources.CurrentStamina >= 1)
            {
                playerResources.ModifyStamina(-1);
            }

            // 处理移动输入
            HandleInput();
            
            // 处理资源调试键
            HandleDebugKeys();
        }

        ExploreCurrentArea();
    }
    
    private void HandleDebugKeys()
    {
        // 燃油调整键 - 1减少gas, 2增加gas
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerResources.ModifyFuel(-1);
            Debug.Log("Removed 1 fuel (debug)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerResources.ModifyFuel(1);
            Debug.Log("Added 1 fuel (debug)");
        }
        
        // 耐久度调整键 - 3减少dur, 4增加dur
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            playerResources.ModifyCarDurability(-1);
            Debug.Log("Removed 1 durability (debug)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            playerResources.ModifyCarDurability(1);
            Debug.Log("Added 1 durability (debug)");
        }
        
        // 精力调整键 - 5减少eng, 6增加eng
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            playerResources.ModifyStamina(-1);
            Debug.Log("Removed 1 stamina (debug)");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            playerResources.ModifyStamina(1);
            Debug.Log("Added 1 stamina (debug)");
        }
    }
    
    private void ConsumeResourceBasedOnCellType()
    {
        if (gridManager == null) return;
        
        GridCell currentCell = gridManager.GetCellAt(currentGridPosition);
        if (currentCell == null) return;
        
        // 如果格子有事件或城镇，不消耗资源
        if (currentCell.HasEvent() || currentCell.HasTown()) return;
        
        // 根据格子的哈希值确定资源类型，与 OnDrawGizmos 中的逻辑保持一致
        int hash = currentGridPosition.GetHashCode();
        System.Random pseudoRandom = new System.Random(hash);
        int resourceType = pseudoRandom.Next(3); // 0, 1, 2
        
        // 创建一个新的随机数生成器，用于决定消耗的资源量
        System.Random consumptionRandom = new System.Random();
        bool consumeMoreResources = consumptionRandom.Next(2) == 0; // 50% 概率
        
        switch (resourceType)
        {
            case 0: // Gas/燃油 - 50%概率消耗1点，50%概率消耗2点
                int fuelAmount = consumeMoreResources ? -2 : -1;
                playerResources.ModifyFuel(fuelAmount);
                Debug.Log($"Consumed {-fuelAmount} fuel in cell type G");
                break;
            case 1: // Durability/耐久 - 50%概率消耗1点，50%概率消耗2点
                int durabilityAmount = consumeMoreResources ? -2 : -1;
                playerResources.ModifyCarDurability(durabilityAmount);
                Debug.Log($"Consumed {-durabilityAmount} durability in cell type D");
                break;
            case 2: // Energy/精力 - 50%概率消耗1点，50%概率消耗2点
                int staminaAmount = consumeMoreResources ? -2 : -1;
                playerResources.ModifyStamina(staminaAmount);
                Debug.Log($"Consumed {-staminaAmount} stamina in cell type E");
                break;
        }
    }

    private void HandleInput()
    {
        if (isMoving) return;

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
            // 检查是否有足够的燃油 - 不再默认消耗燃油，只需要有燃油即可移动
            if (playerResources.CurrentFuel > 0)
            {
                Debug.Log($"Player moving from {currentGridPosition} to {newPosition}");
                
                // 标记当前格子为已访问
                if (gridManager != null)
                {
                    GridCell currentCell = gridManager.GetCellAt(currentGridPosition);
                    if (currentCell != null)
                    {
                        currentCell.SetPlayerVisited();
                    }
                }
                
                currentGridPosition = newPosition;
                targetPosition = CalculateWorldPosition(currentGridPosition);
                isMoving = true;
                // 移除默认的燃油消耗，改为在达到目标格子后根据格子类型消耗
                playerResources.IncrementStepCount(); // 增加步数
            }
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