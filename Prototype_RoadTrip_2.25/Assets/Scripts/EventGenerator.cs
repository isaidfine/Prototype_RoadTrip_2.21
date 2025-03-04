using UnityEngine;
using System.Collections.Generic;

public class EventGenerator : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField] private GameObject eventPrefab;
    [SerializeField] private int targetY = 10; // 目标点的Y坐标
    
    [Header("Level Design")]
    [SerializeField] private bool useRandomGeneration = false; // 是否使用随机生成
    [SerializeField] private int tunnelWidth = 10; // 隧道宽度
    [SerializeField] private int tunnelStartX = 5; // 隧道起始X坐标
    [SerializeField] private int cellsPerEvent = 3; // 每N个格子放置1个事件
    
    [Header("Event Type Weights")]
    [SerializeField] private int checkEventWeight = 60;
    [SerializeField] private int crisisEventWeight = 40;
    
    [Header("Event Category Weights")]
    [SerializeField] private int roadTypeWeight = 33;
    [SerializeField] private int weatherTypeWeight = 33;
    [SerializeField] private int personnelTypeWeight = 34;
    
    [Header("Level Template")]
    [Tooltip("编辑关卡设计模板: 0=空白, 1=检定道路, 2=检定天气, 3=检定人事, 4=危机道路, 5=危机天气, 6=危机人事")]
    [SerializeField] private LevelRow[] levelTemplateRows;
    
    private GridManager gridManager;
    private System.Random random;
    
    // 在Inspector中可编辑的关卡行
    [System.Serializable]
    public class LevelRow
    {
        [Tooltip("此行的事件布局 (0=空白, 1-6=事件类型)")]
        public int[] cells = new int[20]; // 默认宽度为20
    }
    
    private void Awake()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        random = new System.Random();
        
        // 如果没有设置模板，则创建默认模板
        InitializeTemplateIfNeeded();
    }
    
    // 如果用户没有手动设置模板，则初始化默认模板
    private void InitializeTemplateIfNeeded()
    {
        if (levelTemplateRows == null || levelTemplateRows.Length == 0)
        {
            // 设置默认模板
            levelTemplateRows = new LevelRow[11];
            for (int i = 0; i < 11; i++)
            {
                levelTemplateRows[i] = new LevelRow();
            }
            
            // y=0,1 空行
            
            // y=2
            levelTemplateRows[2].cells[2] = 1;  // 检定道路
            levelTemplateRows[2].cells[10] = 2; // 检定天气
            levelTemplateRows[2].cells[16] = 3; // 检定人事
            
            // y=3 空行
            
            // y=4
            levelTemplateRows[4].cells[1] = 4;  // 危机道路
            levelTemplateRows[4].cells[14] = 5; // 危机天气
            
            // y=5
            levelTemplateRows[5].cells[8] = 6;  // 危机人事
            
            // y=6
            levelTemplateRows[6].cells[4] = 2;  // 检定天气
            levelTemplateRows[6].cells[16] = 1; // 检定道路
            
            // y=7 空行
            
            // y=8
            levelTemplateRows[8].cells[2] = 5;  // 危机天气
            levelTemplateRows[8].cells[9] = 4;  // 危机道路
            
            // y=9
            levelTemplateRows[9].cells[18] = 3; // 检定人事
            
            // y=10 空行
        }
    }
    
    public void GenerateEvents()
    {
        if (gridManager == null || eventPrefab == null)
        {
            Debug.LogError("EventGenerator: Missing required components!");
            return;
        }
        
        if (useRandomGeneration)
        {
            GenerateRandomEvents();
        }
        else
        {
            GenerateTemplateEvents();
        }
    }
    
    // 使用预定义模板生成事件
    private void GenerateTemplateEvents()
    {
        if (levelTemplateRows == null || levelTemplateRows.Length == 0)
        {
            Debug.LogWarning("EventGenerator: No level template defined.");
            return;
        }
        
        int height = Mathf.Min(levelTemplateRows.Length, targetY + 1);
        
        for (int y = 0; y < height; y++)
        {
            if (levelTemplateRows[y] == null) continue;
            
            int width = Mathf.Min(levelTemplateRows[y].cells.Length, GridManager.WIDTH);
            for (int x = 0; x < width; x++)
            {
                int eventType = levelTemplateRows[y].cells[x];
                if (eventType > 0)
                {
                    CreateEventFromTemplate(new Vector2Int(x, y), eventType);
                }
            }
        }
    }
    
    // 创建预定义的事件
    private void CreateEventFromTemplate(Vector2Int position, int eventType)
    {
        GridCell cell = gridManager.GetCellAt(position);
        if (cell == null || cell.GetEvent() != null || cell.GetComponent<Town>() != null)
        {
            return;
        }
        
        // 将模板值转换为事件类型
        // 1-3 = 检定类事件，4-6 = 危机类事件
        GridEvent.EventCategory category = (eventType <= 3) ? 
            GridEvent.EventCategory.Check : GridEvent.EventCategory.Crisis;
        
        // 获取事件子类型
        GridEvent.EventType type;
        switch (eventType % 3)
        {
            case 1: type = GridEvent.EventType.Road; break;
            case 2: type = GridEvent.EventType.Weather; break;
            default: type = GridEvent.EventType.Personnel; break;
        }
        
        // 创建事件对象
        GameObject eventObj = Instantiate(eventPrefab, cell.transform.position, Quaternion.identity);
        eventObj.transform.SetParent(transform);
        GridEvent gridEvent = eventObj.GetComponent<GridEvent>();
        
        if (gridEvent == null)
        {
            Debug.LogError("EventGenerator: Event prefab does not have GridEvent component!");
            Destroy(eventObj);
            return;
        }
        
        // 初始化事件
        gridEvent.Initialize(position, category, type);
        
        // 将事件附加到网格单元格
        cell.SetEvent(gridEvent);
    }
    
    // 随机生成事件
    private void GenerateRandomEvents()
    {
        // 定义隧道区域
        int tunnelEndY = targetY; // 隧道高度

        // 创建一个数组，记录每列的最后一个事件的Y坐标
        int[] lastEventYInColumn = new int[tunnelWidth];
        for (int i = 0; i < tunnelWidth; i++)
        {
            lastEventYInColumn[i] = -3; // 初始化为一个负数，表示之前没有事件
        }

        // 对于每个y行
        for (int y = 1; y < tunnelEndY; y++)
        {
            // 对于每一行，按设定的密度生成事件，并考虑垂直分布
            PlaceEventsInRow(y, tunnelStartX, tunnelWidth, lastEventYInColumn);
        }
    }

    // 在特定行中放置事件，考虑垂直分布
    private void PlaceEventsInRow(int y, int startX, int width, int[] lastEventYInColumn)
    {
        // 计算这一行应该放置的事件数量
        int cellCount = width;
        int eventsToPlace = Mathf.Max(1, cellCount / cellsPerEvent);
        
        // 创建候选位置列表，包含权重信息
        List<KeyValuePair<int, int>> weightedPositions = new List<KeyValuePair<int, int>>();
        
        // 为每个位置分配权重，连续空白区域的权重更高
        for (int x = 0; x < width; x++)
        {
            int actualX = startX + x;
            int verticalGap = y - lastEventYInColumn[x];
            
            // 如果垂直距离大，给更高权重
            int weight = 1;
            if (verticalGap >= 4) weight = 8;      // 超过4格没有事件，极高优先级
            else if (verticalGap >= 3) weight = 4; // 3格没有事件，高优先级 
            else if (verticalGap >= 2) weight = 2; // 2格没有事件，中优先级
            
            // 将位置和权重添加到列表
            for (int i = 0; i < weight; i++)
            {
                weightedPositions.Add(new KeyValuePair<int, int>(actualX, x));
            }
        }
        
        // 随机选择位置放置事件
        HashSet<int> usedXIndices = new HashSet<int>(); // 跟踪已使用的X索引
        HashSet<int> usedXPositions = new HashSet<int>(); // 跟踪已使用的X坐标
        
        for (int i = 0; i < eventsToPlace && weightedPositions.Count > 0; i++)
        {
            // 随机选择一个位置
            int randomIndex = random.Next(weightedPositions.Count);
            KeyValuePair<int, int> selected = weightedPositions[randomIndex];
            int selectedX = selected.Key;      // 实际X坐标
            int selectedXIndex = selected.Value; // X在隧道中的索引
            
            // 如果这个位置已经使用过，尝试找其他位置
            if (usedXPositions.Contains(selectedX))
            {
                // 从列表中移除所有相同位置的项
                weightedPositions.RemoveAll(p => p.Key == selectedX);
                
                // 如果还有剩余位置，重试
                if (weightedPositions.Count > 0)
                {
                    i--; // 重试当前循环
                    continue;
                }
                else
                {
                    break; // 没有可用位置了，退出循环
                }
            }
            
            // 在此位置创建事件
            CreateEventAt(new Vector2Int(selectedX, y));
            
            // 更新这一列的最后事件位置
            lastEventYInColumn[selectedXIndex] = y;
            
            // 记录已使用的X
            usedXIndices.Add(selectedXIndex);
            usedXPositions.Add(selectedX);
            
            // 从列表中移除已使用的位置及其相邻位置
            weightedPositions.RemoveAll(p => 
                p.Key == selectedX || 
                p.Key == selectedX - 1 || 
                p.Key == selectedX + 1);
        }
    }
    
    // 在指定位置创建事件
    private void CreateEventAt(Vector2Int position)
    {
        // 检查该位置是否已经有城镇或其他事件
        GridCell cell = gridManager.GetCellAt(position);
        if (cell == null)
        {
            Debug.LogWarning($"EventGenerator: No cell found at position {position}");
            return;
        }
        
        if (cell.GetEvent() != null || cell.GetComponent<Town>() != null)
        {
            return;
        }
        
        // 确定事件类型（检定/危机）
        GridEvent.EventCategory category = GetRandomEventCategory();
        
        // 确定事件类别（道路/天气/人事）
        GridEvent.EventType type = GetRandomEventType();
        
        // 创建事件对象
        GameObject eventObj = Instantiate(eventPrefab, cell.transform.position, Quaternion.identity);
        eventObj.transform.SetParent(transform);
        GridEvent gridEvent = eventObj.GetComponent<GridEvent>();
        
        if (gridEvent == null)
        {
            Debug.LogError("EventGenerator: Event prefab does not have GridEvent component!");
            Destroy(eventObj);
            return;
        }
        
        // 初始化事件
        gridEvent.Initialize(position, category, type);
        
        // 将事件附加到网格单元格
        cell.SetEvent(gridEvent);
    }
    
    // 根据权重随机选择事件类型
    private GridEvent.EventCategory GetRandomEventCategory()
    {
        int totalWeight = checkEventWeight + crisisEventWeight;
        int randomValue = random.Next(totalWeight);
        
        if (randomValue < checkEventWeight)
        {
            return GridEvent.EventCategory.Check;
        }
        else
        {
            return GridEvent.EventCategory.Crisis;
        }
    }
    
    // 根据权重随机选择事件类别
    private GridEvent.EventType GetRandomEventType()
    {
        int totalWeight = roadTypeWeight + weatherTypeWeight + personnelTypeWeight;
        int randomValue = random.Next(totalWeight);
        
        if (randomValue < roadTypeWeight)
        {
            return GridEvent.EventType.Road;
        }
        else if (randomValue < roadTypeWeight + weatherTypeWeight)
        {
            return GridEvent.EventType.Weather;
        }
        else
        {
            return GridEvent.EventType.Personnel;
        }
    }
} 