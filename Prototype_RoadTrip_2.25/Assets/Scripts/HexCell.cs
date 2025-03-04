using UnityEngine;
using UnityEditor;

public class GridCell : MonoBehaviour
{
    public Vector2Int coordinates; // 在网格中的坐标
    private SpriteRenderer spriteRenderer;
    private bool isExplored = false;
    private bool isPlayerVisited = false;  // 记录玩家是否访问过
    private GridEvent gridEvent;  // 新增：事件引用

    [SerializeField] private Sprite unexploredSprite;
    [SerializeField] private Sprite exploredSprite;
    
    [Header("Colors")]
    public Color visitedPathColor = new Color(1f, 1f, 0f, 0.3f);  // 默认淡黄色
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Initialize(Vector2Int coords, Sprite unexplored, Sprite explored)
    {
        coordinates = coords;
        unexploredSprite = unexplored;
        exploredSprite = explored;
        spriteRenderer.sprite = unexploredSprite;
    }

    public void SetExplored()
    {
        if (!isExplored)
        {
            isExplored = true;
            spriteRenderer.sprite = exploredSprite;
            
            // 如果有事件，也设置为已探索
            if (gridEvent != null)
            {
                gridEvent.SetExplored();
            }
        }
    }

    public bool IsExplored()
    {
        return isExplored;
    }

    public void SetPlayerVisited()
    {
        if (!isPlayerVisited && !HasTown() && !HasEvent())  // 只有不是城镇和事件的格子才改变颜色
        {
            isPlayerVisited = true;
            spriteRenderer.color = visitedPathColor;
        }
    }

    private bool HasTown()
    {
        return GetComponent<Town>() != null;
    }

    // 新增：检查是否有事件
    private bool HasEvent()
    {
        return gridEvent != null;
    }

    // 新增：设置事件
    public void SetEvent(GridEvent evt)
    {
        gridEvent = evt;
    }

    // 新增：获取事件
    public GridEvent GetEvent()
    {
        return gridEvent;
    }

    // 新增：在Gizmos中显示事件类型
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;  // 只在游戏运行时显示

        if (gridEvent != null)
        {
            // 根据事件类型显示不同的标记
            string marker = "";
            switch (gridEvent.type)
            {
                case GridEvent.EventType.Road:
                    marker = "R";
                    break;
                case GridEvent.EventType.Weather:
                    marker = "W";
                    break;
                case GridEvent.EventType.Personnel:
                    marker = "P";
                    break;
            }

            // 根据事件类别设置颜色
            Gizmos.color = (gridEvent.category == GridEvent.EventCategory.Check) ? 
                Color.green : Color.red;

            // 在格子中心绘制文本
            Vector3 position = transform.position;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(position, marker);
            #endif
        }
        else if (!HasTown())
        {
            // 对于没有事件和城镇的格子，显示G/D/E标记
            // 使用哈希函数基于坐标生成一个确定的随机值，这样同一个格子总是显示相同的标记
            int hash = coordinates.GetHashCode();
            System.Random pseudoRandom = new System.Random(hash);
            int resourceType = pseudoRandom.Next(3); // 0, 1, 2
            
            string marker = "";
            switch (resourceType)
            {
                case 0:
                    marker = "G"; // Gas/燃油
                    break;
                case 1:
                    marker = "D"; // Durability/耐久
                    break;
                case 2:
                    marker = "E"; // Energy/精力
                    break;
            }
            
            Gizmos.color = Color.yellow;
            Vector3 position = transform.position;
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(position, marker);
            #endif
        }
    }
} 