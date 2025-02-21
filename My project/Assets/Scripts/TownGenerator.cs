using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TownGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject townPrefab;
    [SerializeField] private LineRenderer pathPrefab;
    
    [Header("Map Settings")]
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 100;
    [SerializeField] [Range(2, 10)] private int numberOfZones = 3; // 添加区域数量配置
    
    [Header("Generation Settings")]
    [SerializeField] private int[] coreTownsPerZone = new int[3] { 2, 2, 1 };
    [SerializeField] private int[] satelliteRadius = new int[3] { 4, 5, 6 };
    [SerializeField] private float[] satelliteDensity = new float[3] { 0.4f, 0.3f, 0.2f };
    [SerializeField] private float scatteredTownDensity = 0.05f;
    [SerializeField] private int maxSatellitesPerCore = 8;
    [SerializeField] private int minDistanceBetweenScatteredTowns = 4;
    [SerializeField] private int minDistanceFromCore = 6;
    
    // 将常量改为属性
    private int WIDTH => width;
    private int HEIGHT => height;
    private int ZONES => numberOfZones;
    private int ZONE_HEIGHT => HEIGHT / ZONES;

    private List<Town> allTowns = new List<Town>();
    private List<LineRenderer> pathLines = new List<LineRenderer>();

    private GridManager gridManager; // 添加引用

    private void OnValidate()
    {
        // 确保数组长度与区域数量匹配
        if (coreTownsPerZone.Length != ZONES)
        {
            System.Array.Resize(ref coreTownsPerZone, ZONES);
            // 为新增的元素设置默认值
            for (int i = 0; i < ZONES; i++)
            {
                if (i >= coreTownsPerZone.Length)
                {
                    coreTownsPerZone[i] = 1;
                }
            }
        }

        if (satelliteRadius.Length != ZONES)
        {
            System.Array.Resize(ref satelliteRadius, ZONES);
            for (int i = 0; i < ZONES; i++)
            {
                if (i >= satelliteRadius.Length)
                {
                    satelliteRadius[i] = 4 + i;
                }
            }
        }

        if (satelliteDensity.Length != ZONES)
        {
            System.Array.Resize(ref satelliteDensity, ZONES);
            for (int i = 0; i < ZONES; i++)
            {
                if (i >= satelliteDensity.Length)
                {
                    satelliteDensity[i] = 0.2f - (i * 0.05f);
                }
            }
        }

        // 确保高度能被区域数量整除
        height = (height / numberOfZones) * numberOfZones;
    }

    private void Start()
    {
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        if (townPrefab == null)
        {
            Debug.LogError("Town prefab is not assigned!");
            return;
        }

        if (pathPrefab == null)
        {
            Debug.LogError("Path prefab is not assigned!");
            return;
        }

        GenerateTowns();
        ConnectNearbyTowns();
    }

    private void GenerateTowns()
    {
        // 从下往上生成每个区域
        for (int zone = 0; zone < ZONES; zone++)
        {
            GenerateZone(zone);
        }
    }

    private void GenerateZone(int zone)
    {
        int zoneStartY = zone * ZONE_HEIGHT;
        int zoneEndY = (zone + 1) * ZONE_HEIGHT;

        // 生成核心城镇
        List<Vector2Int> coreTownPositions = new List<Vector2Int>();
        for (int i = 0; i < coreTownsPerZone[zone]; i++)
        {
            Vector2Int position = GetValidCoreTownPosition(zone, coreTownPositions);
            if (position != Vector2Int.zero)
            {
                CreateTown(position, Town.TownType.Core);
                coreTownPositions.Add(position);
            }
        }

        // 在核心城镇周围生成卫星城镇
        foreach (var corePos in coreTownPositions)
        {
            GenerateSatelliteTowns(corePos, zone);
        }

        // 生成散布的城镇
        GenerateScatteredTowns(zoneStartY, zoneEndY);
    }

    private Vector2Int GetValidCoreTownPosition(int zone, List<Vector2Int> existingPositions)
    {
        int zoneStartY = zone * ZONE_HEIGHT;
        int zoneEndY = (zone + 1) * ZONE_HEIGHT;
        
        // 计算这个城镇应该在区域的哪个部分
        int townIndex = existingPositions.Count;
        int townsInZone = coreTownsPerZone[zone];
        
        // 计算理想的Y坐标范围
        float sectionHeight = ZONE_HEIGHT / (float)townsInZone;
        float targetY = zoneStartY + (townIndex + 0.5f) * sectionHeight;
        
        // 在理想Y坐标附近的范围内寻找位置
        int yRange = Mathf.RoundToInt(sectionHeight * 0.25f); // 允许在理想位置上下25%范围内浮动
        int minY = Mathf.RoundToInt(targetY - yRange);
        int maxY = Mathf.RoundToInt(targetY + yRange);
        
        // 特殊处理第一个区域
        if (zone == 0)
        {
            minY = Mathf.Max(minY, 10);
            maxY = Mathf.Min(maxY, 20);
        }

        int maxAttempts = 50;
        while (maxAttempts > 0)
        {
            // 在区域宽度范围内随机选择X坐标
            int x = Random.Range(4, WIDTH - 4);
            // 在计算出的Y范围内随机选择Y坐标
            int y = Random.Range(minY, maxY + 1);
            Vector2Int newPos = new Vector2Int(x, y);

            if (IsValidCoreTownPosition(newPos, existingPositions))
            {
                return newPos;
            }
            maxAttempts--;
        }
        
        // 如果找不到合适的位置，使用理想Y坐标
        return new Vector2Int(WIDTH / 2, Mathf.RoundToInt(targetY));
    }

    private bool IsValidCoreTownPosition(Vector2Int pos, List<Vector2Int> existingPositions)
    {
        // 检查是否太靠近边界
        if (pos.x < 4 || pos.x > WIDTH - 4)
            return false;

        // 检查与现有核心城镇的距离
        foreach (var existing in existingPositions)
        {
            // 检查水平距离
            int xDist = Mathf.Abs(pos.x - existing.x);
            if (xDist < WIDTH / 3) // 确保水平间距至少是地图宽度的1/3
                return false;
            
            // 检查总距离
            if (ManhattanDistance(pos, existing) < 6)
                return false;
        }
        
        return true;
    }

    private void GenerateSatelliteTowns(Vector2Int corePosition, int zone)
    {
        int radius = satelliteRadius[zone];
        float density = satelliteDensity[zone];
        int satelliteCount = 0;
        
        // 简化的扇区划分：4个主要方向
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // 右
            new Vector2Int(-1, 0),  // 左
            new Vector2Int(0, 1),   // 上
            new Vector2Int(0, -1)   // 下
        };

        // 在每个主要方向上尝试生成卫星城镇
        foreach (var dir in directions)
        {
            if (satelliteCount >= maxSatellitesPerCore) break;

            // 在2-radius范围内尝试生成
            for (int dist = 2; dist <= radius; dist++)
            {
                if (Random.value > density) continue;

                Vector2Int basePos = corePosition + dir * dist;
                
                // 在基础位置周围尝试几个偏移位置
                for (int offset = -1; offset <= 1; offset++)
                {
                    Vector2Int pos = basePos;
                    if (dir.x != 0) // 水平方向上的基础位置
                        pos.y += offset;
                    else // 垂直方向上的基础位置
                        pos.x += offset;

                    if (IsValidPosition(pos) && !IsTownNearby(pos, 2))
                    {
                        CreateTown(pos, Town.TownType.Satellite);
                        satelliteCount++;
                        break;
                    }
                }

                if (satelliteCount >= maxSatellitesPerCore) break;
            }
        }

        // 如果主要方向没有生成足够的卫星城镇，尝试在对角线方向生成
        if (satelliteCount < maxSatellitesPerCore)
        {
            Vector2Int[] diagonals = new Vector2Int[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(-1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, -1)
            };

            foreach (var dir in diagonals)
            {
                if (satelliteCount >= maxSatellitesPerCore) break;

                for (int dist = 2; dist <= radius; dist++)
                {
                    if (Random.value > density) continue;

                    Vector2Int pos = corePosition + dir * dist;
                    if (IsValidPosition(pos) && !IsTownNearby(pos, 2))
                    {
                        CreateTown(pos, Town.TownType.Satellite);
                        satelliteCount++;
                        break;
                    }
                }
            }
        }
    }

    private float EvaluatePosition(Vector2Int pos, Vector2Int corePos, List<Town> existingTowns)
    {
        float score = 0;
        
        // 计算与核心城镇的距离得分
        float distToCore = ManhattanDistance(pos, corePos);
        score += 1.0f / (distToCore + 1); // 距离越近分数越高
        
        // 计算与其他卫星城镇的距离得分
        float minDistToOthers = float.MaxValue;
        foreach (var town in existingTowns)
        {
            if (town.townType == Town.TownType.Satellite)
            {
                float dist = ManhattanDistance(pos, town.gridPosition);
                minDistToOthers = Mathf.Min(minDistToOthers, dist);
            }
        }
        score += minDistToOthers * 0.5f; // 距离其他卫星城镇越远分数越高
        
        return score;
    }

    private void GenerateScatteredTowns(int startY, int endY)
    {
        int gridSpacing = minDistanceBetweenScatteredTowns; // 基础网格间距
        List<Vector2Int> possiblePositions = new List<Vector2Int>();
        
        // 使用网格布局生成候选位置
        for (int y = startY; y < endY; y += gridSpacing)
        {
            for (int x = 2; x < WIDTH - 2; x += gridSpacing)
            {
                // 在网格点周围添加带随机偏移的位置
                for (int i = 0; i < 3; i++) // 每个网格点尝试3个随机位置
                {
                    int offsetX = Random.Range(-1, 2); // -1到1的随机偏移
                    int offsetY = Random.Range(-1, 2);
                    Vector2Int pos = new Vector2Int(
                        Mathf.Clamp(x + offsetX, 2, WIDTH - 3),
                        Mathf.Clamp(y + offsetY, startY, endY - 1)
                    );
                    
                    if (IsValidScatteredTownPosition(pos))
                    {
                        possiblePositions.Add(pos);
                    }
                }
            }
        }

        // 随机选择位置生成散布城镇，但保持最小间距
        while (possiblePositions.Count > 0)
        {
            if (Random.value < scatteredTownDensity)
            {
                // 选择一个位置，偏好与其他城镇保持较远距离
                int bestIndex = 0;
                float bestMinDistance = 0;

                // 随机取样几个位置，选择最优的
                for (int i = 0; i < 3; i++)
                {
                    int testIndex = Random.Range(0, possiblePositions.Count);
                    float minDist = float.MaxValue;

                    foreach (var town in allTowns)
                    {
                        float dist = ManhattanDistance(possiblePositions[testIndex], town.gridPosition);
                        minDist = Mathf.Min(minDist, dist);
                    }

                    if (minDist > bestMinDistance)
                    {
                        bestMinDistance = minDist;
                        bestIndex = testIndex;
                    }
                }

                Vector2Int selectedPos = possiblePositions[bestIndex];
                CreateTown(selectedPos, Town.TownType.Normal);

                // 移除周围的可能位置
                possiblePositions.RemoveAll(p => 
                    ManhattanDistance(p, selectedPos) < minDistanceBetweenScatteredTowns);
            }
            else
            {
                // 移除一个随机位置
                possiblePositions.RemoveAt(Random.Range(0, possiblePositions.Count));
            }
        }
    }

    private bool IsValidScatteredTownPosition(Vector2Int pos)
    {
        // 检查是否在任何核心城镇的影响范围内
        foreach (var town in allTowns)
        {
            if (town.townType == Town.TownType.Core)
            {
                if (ManhattanDistance(pos, town.gridPosition) < minDistanceFromCore)
                {
                    return false;
                }
            }
        }

        // 检查是否在任何卫星城镇的最小距离内
        foreach (var town in allTowns)
        {
            if (ManhattanDistance(pos, town.gridPosition) < minDistanceBetweenScatteredTowns)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsTownNearby(Vector2Int pos, int distance)
    {
        foreach (var town in allTowns)
        {
            if (ManhattanDistance(pos, town.gridPosition) < distance)
            {
                return true;
            }
        }
        return false;
    }

    private void ConnectNearbyTowns()
    {
        for (int i = 0; i < allTowns.Count; i++)
        {
            for (int j = i + 1; j < allTowns.Count; j++)
            {
                Town town1 = allTowns[i];
                Town town2 = allTowns[j];

                if (ManhattanDistance(town1.gridPosition, town2.gridPosition) <= 6)
                {
                    LineRenderer line = CreatePathLine(town1.transform.position, town2.transform.position);
                    line.gameObject.SetActive(false); // 初始时隐藏
                    
                    // 存储连接信息以便后续显示
                    connections.Add(new TownConnection
                    {
                        town1 = town1,
                        town2 = town2,
                        line = line
                    });
                }
            }
        }
    }

    private LineRenderer CreatePathLine(Vector3 start, Vector3 end)
    {
        LineRenderer line = Instantiate(pathPrefab, Vector3.zero, Quaternion.identity, transform);
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.sortingOrder = 1; // 确保在城镇后面显示
        
        // 设置材质和颜色
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        line.endColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        
        pathLines.Add(line);
        return line;
    }

    private int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < WIDTH && pos.y >= 0 && pos.y < HEIGHT;
    }

    // 添加一个新的类来存储连接信息
    private class TownConnection
    {
        public Town town1;
        public Town town2;
        public LineRenderer line;
    }

    private List<TownConnection> connections = new List<TownConnection>();

    // 修改探索方法以更新连线显示
    public void ExploreArea(Vector2Int center, int radius)
    {
        // 检查每个城镇所在的地块是否被探索
        foreach (Town town in allTowns)
        {
            // 获取城镇所在的格子
            GridCell cell = gridManager.GetCellAt(town.gridPosition);
            if (cell != null && cell.IsExplored())
            {
                town.SetExplored();
            }
        }

        // 更新连线显示
        foreach (var connection in connections)
        {
            if (connection.town1.IsExplored() && connection.town2.IsExplored())
            {
                connection.line.gameObject.SetActive(true);
            }
        }
    }

    private void CreateTown(Vector2Int position, Town.TownType type)
    {
        if (townPrefab == null)
        {
            Debug.LogError("Cannot create town: prefab is null!");
            return;
        }

        Vector3 worldPos = new Vector3(position.x, position.y, 0);
        GameObject townObj = Instantiate(townPrefab, worldPos, Quaternion.identity, transform);
        
        Town town = townObj.GetComponent<Town>();
        if (town == null)
        {
            Debug.LogError("Town component missing from prefab!");
            return;
        }

        town.Initialize(position, type);
        allTowns.Add(town);
    }

    // 添加公共方法用于检查位置有效性
    public bool IsPositionInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < WIDTH &&
               position.y >= 0 && position.y < HEIGHT;
    }

    private void OnDrawGizmos()
    {
        foreach (var town in allTowns)
        {
            if (town == null) continue;
            
            // 绘制城镇位置
            switch (town.townType)
            {
                case Town.TownType.Core:
                    // 核心城镇 - 橙色
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                    Gizmos.DrawWireCube(town.transform.position + Vector3.up * 0.5f, new Vector3(1f, 2f, 0.1f));
                    break;
                
                case Town.TownType.Satellite:
                    // 卫星城镇 - 绿色
                    Gizmos.color = new Color(0.3f, 0.8f, 0.3f, 0.5f);
                    Gizmos.DrawWireCube(town.transform.position, new Vector3(1f, 1f, 0.1f));
                    break;
                
                case Town.TownType.Normal:
                    // 普通城镇 - 蓝色
                    Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.5f);
                    Gizmos.DrawWireCube(town.transform.position, new Vector3(1f, 1f, 0.1f));
                    break;
            }
        }

        // 绘制连接线
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // 半透明灰色
        foreach (var connection in connections)
        {
            if (connection.town1 != null && connection.town2 != null)
            {
                Gizmos.DrawLine(connection.town1.transform.position, connection.town2.transform.position);
            }
        }

        // 绘制区域分隔线
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f); // 半透明黄色
        for (int i = 1; i < ZONES; i++)
        {
            float y = i * ZONE_HEIGHT;
            Vector3 start = new Vector3(-1, y, 0);
            Vector3 end = new Vector3(WIDTH + 1, y, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    private void OnDisable()
    {
        // 清理所有生成的对象
        foreach (var town in allTowns)
        {
            if (town != null)
            {
                if (Application.isPlaying)
                    Destroy(town.gameObject);
                else
                    DestroyImmediate(town.gameObject);
            }
        }
        allTowns.Clear();

        foreach (var line in pathLines)
        {
            if (line != null)
            {
                if (Application.isPlaying)
                    Destroy(line.gameObject);
                else
                    DestroyImmediate(line.gameObject);
            }
        }
        pathLines.Clear();

        connections.Clear();
    }
} 