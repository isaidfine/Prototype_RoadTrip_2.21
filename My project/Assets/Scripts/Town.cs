using UnityEngine;

public class Town : MonoBehaviour
{
    public enum TownType
    {
        Core,       // 核心城镇
        Satellite,  // 卫星城镇
        Normal      // 普通城镇（散布城镇）
    }

    public TownType townType;
    public Vector2Int gridPosition;
    
    [SerializeField] private Color coreColor = new Color(1f, 0.5f, 0f); // 橙色
    [SerializeField] private Color normalColor = new Color(0.3f, 0.7f, 1f); // 浅蓝色
    
    private bool isExplored = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Town missing SpriteRenderer component!");
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        // 初始时隐藏
        spriteRenderer.enabled = false;
    }

    public void Initialize(Vector2Int position, TownType type)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer is null in Initialize!");
            return;
        }

        gridPosition = position;
        townType = type;
        
        // 设置外观
        switch (townType)
        {
            case TownType.Core:
                transform.localScale = new Vector3(1f, 2f, 1f);
                spriteRenderer.sprite = spriteRenderer.sprite ?? CreateDefaultSprite();
                spriteRenderer.color = coreColor;
                // 调整位置，使其对齐格子
                transform.position = new Vector3(
                    position.x,
                    position.y + 0.5f, // 向上偏移0.5个单位，使其中心与两个格子的交界处对齐
                    transform.position.z
                );
                break;
            
            case TownType.Satellite:
                transform.localScale = Vector3.one;
                spriteRenderer.sprite = spriteRenderer.sprite ?? CreateDefaultSprite();
                spriteRenderer.color = new Color(0.3f, 0.8f, 0.3f); // 绿色
                break;
            
            case TownType.Normal:
                transform.localScale = Vector3.one;
                spriteRenderer.sprite = spriteRenderer.sprite ?? CreateDefaultSprite();
                spriteRenderer.color = normalColor;
                break;
        }
    }

    private Sprite CreateDefaultSprite()
    {
        // 创建一个默认的白色方块精灵
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.white;
        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
    }

    public void SetExplored()
    {
        if (!isExplored)
        {
            isExplored = true;
            spriteRenderer.enabled = true;
        }
    }

    public bool IsExplored()
    {
        return isExplored;
    }

    private void OnDestroy()
    {
        // 清理动态创建的精灵
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // 如果是我们动态创建的精灵，则销毁它
            if (spriteRenderer.sprite.texture.name == "")
            {
                Destroy(spriteRenderer.sprite.texture);
                Destroy(spriteRenderer.sprite);
            }
        }
    }
} 