using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target; // 跟随的目标（玩家）
    [SerializeField] private float smoothSpeed = 5f; // 平滑移动速度
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // 相机偏移量
    
    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = true;
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 20f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 100f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;  // 最大放大倍数（数值越小视野越近）
    [SerializeField] private float maxZoom = 8f; // 最大缩小倍数（数值越大视野越远）
    [SerializeField] private float defaultZoom = 5f; // 默认缩放值

    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    private float currentZoom;
    private Vector3 targetPosition; // 添加这个变量来存储目标位置

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found!");
            return;
        }

        if (!cam.orthographic)
        {
            Debug.LogWarning("Camera is not set to Orthographic mode!");
            cam.orthographic = true;
        }

        currentZoom = defaultZoom;
        cam.orthographicSize = currentZoom;
        
        // 初始化目标位置
        if (target != null)
        {
            targetPosition = target.position + offset;
        }
        
        Debug.Log($"Camera initialized with zoom: {currentZoom}");
    }

    private void Update()
    {
        if (target == null) return;

        // 更新目标位置
        targetPosition = target.position + offset;

        // 处理缩放输入
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            // 计算新的缩放值
            float zoomDelta = -scrollInput * zoomSpeed * 5f;
            float previousZoom = currentZoom;
            currentZoom = Mathf.Clamp(currentZoom + zoomDelta, minZoom, maxZoom);
            
            // 只有在缩放值真正改变时才更新相机
            if (previousZoom != currentZoom)
            {
                cam.orthographicSize = currentZoom;
                
                // 立即更新相机位置到目标位置
                Vector3 newPosition = target.position + offset;
                newPosition = GetClampedPosition(newPosition);
                transform.position = newPosition;
                
                velocity = Vector3.zero; // 重置平滑移动的速度
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;
        desiredPosition = GetClampedPosition(desiredPosition);

        // 使用 SmoothDamp 实现更平滑的移动
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref velocity, 
            1f / smoothSpeed
        );
    }

    private Vector3 GetClampedPosition(Vector3 position)
    {
        if (useBoundaries)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            // 计算可移动的实际范围
            float effectiveMinX = minX + halfWidth;
            float effectiveMaxX = maxX - halfWidth;
            float effectiveMinY = minY + halfHeight;
            float effectiveMaxY = maxY - halfHeight;

            // 如果可视范围大于地图，则将相机固定在中心
            if (effectiveMaxX - effectiveMinX <= 0)
            {
                position.x = (minX + maxX) * 0.5f;
            }
            else
            {
                position.x = Mathf.Clamp(position.x, effectiveMinX, effectiveMaxX);
            }

            if (effectiveMaxY - effectiveMinY <= 0)
            {
                position.y = (minY + maxY) * 0.5f;
            }
            else
            {
                position.y = Mathf.Clamp(position.y, effectiveMinY, effectiveMaxY);
            }
        }

        position.z = offset.z;
        return position;
    }

    // 用于在编辑器中可视化相机边界
    private void OnDrawGizmosSelected()
    {
        if (!useBoundaries) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0),
            new Vector3(maxX - minX, maxY - minY, 0)
        );
    }
} 