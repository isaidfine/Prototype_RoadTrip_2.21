using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector2Int coordinates; // 在网格中的坐标
    private SpriteRenderer spriteRenderer;
    private bool isExplored = false;
    private bool isPlayerVisited = false;  // 新增：记录玩家是否访问过

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
        }
    }

    public bool IsExplored()
    {
        return isExplored;
    }

    // 新增：设置玩家访问状态
    public void SetPlayerVisited()
    {
        if (!isPlayerVisited && !HasTown())  // 只有不是城镇的格子才改变颜色
        {
            isPlayerVisited = true;
            spriteRenderer.color = visitedPathColor;
        }
    }

    // 新增：检查是否有城镇
    private bool HasTown()
    {
        // 检查这个格子上是否有城镇组件
        return GetComponent<Town>() != null;
    }
} 