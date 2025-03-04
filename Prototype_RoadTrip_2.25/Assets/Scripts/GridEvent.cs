using UnityEngine;
using TMPro;

public class GridEvent : MonoBehaviour
{
    public enum EventCategory
    {
        Check,      // 检定类
        Crisis      // 危机类
    }

    public enum EventType
    {
        Road,       // 道路型
        Weather,    // 天气型
        Personnel   // 人事型
    }

    public EventCategory category;
    public EventType type;
    public Vector2Int gridPosition;
    private bool isExplored = false;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro typeText;

    [Header("Colors")]
    [SerializeField] private Color checkColor = new Color(0.3f, 0.8f, 0.3f, 0.7f);  // 检定类 - 绿色
    [SerializeField] private Color crisisColor = new Color(0.8f, 0.3f, 0.3f, 0.7f); // 危机类 - 红色

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 创建显示事件类型的文本
        typeText = GetComponentInChildren<TextMeshPro>();
        if (typeText == null)
        {
            GameObject textObj = new GameObject("EventTypeText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = new Vector3(0, 0, -0.1f);
            
            typeText = textObj.AddComponent<TextMeshPro>();
            typeText.alignment = TextAlignmentOptions.Center;
            typeText.fontSize = 3;
            typeText.color = Color.white;
        }
        
        spriteRenderer.enabled = false;
        if (typeText != null)
        {
            typeText.enabled = false;
        }
    }

    public void Initialize(Vector2Int position, EventCategory cat, EventType t)
    {
        gridPosition = position;
        category = cat;
        type = t;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        
        // 设置颜色
        spriteRenderer.color = (category == EventCategory.Check) ? checkColor : crisisColor;
        
        // 设置事件类型文本
        if (typeText != null)
        {
            switch (type)
            {
                case EventType.Road:
                    typeText.text = "R";
                    break;
                case EventType.Weather:
                    typeText.text = "W";
                    break;
                case EventType.Personnel:
                    typeText.text = "P";
                    break;
            }
        }
    }

    public void SetExplored()
    {
        if (!isExplored)
        {
            isExplored = true;
            spriteRenderer.enabled = true;
            if (typeText != null)
            {
                typeText.enabled = true;
            }
        }
    }

    public bool IsExplored()
    {
        return isExplored;
    }
} 