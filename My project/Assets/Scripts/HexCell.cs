using UnityEngine;

public class GridCell : MonoBehaviour
{
    public Vector2Int coordinates; // 在网格中的坐标
    private SpriteRenderer spriteRenderer;
    private bool isExplored = false;

    [SerializeField] private Sprite unexploredSprite;
    [SerializeField] private Sprite exploredSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
} 