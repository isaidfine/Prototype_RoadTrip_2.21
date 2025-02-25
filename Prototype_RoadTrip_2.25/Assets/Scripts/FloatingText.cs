using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private float duration;
    private float speed;
    private float elapsed = 0;
    private TextMeshPro textComponent;

    public void Initialize(float duration, float speed)
    {
        this.duration = duration;
        this.speed = speed;
        textComponent = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        
        // 向上移动
        transform.position += Vector3.up * speed * Time.deltaTime;
        
        // 淡出效果
        if (textComponent != null)
        {
            float alpha = 1 - (elapsed / duration);
            Color color = textComponent.color;
            color.a = alpha;
            textComponent.color = color;
        }

        // 销毁
        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
} 