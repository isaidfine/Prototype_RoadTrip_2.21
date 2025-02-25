using UnityEngine;
using System.Collections.Generic;

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

    [Header("Fruit System")]
    private FruitData[] lotteryFruits = new FruitData[2];  // 奖池中的两种水果
    private FruitData buyingFruit;  // 当前收购的水果
    
    // 改为私有变量，不在 Inspector 中显示
    private SpriteRenderer[] lotteryFruitIcons;  // 移除 [SerializeField]
    private SpriteRenderer buyingFruitIcon;      // 移除 [SerializeField]
    
    private UIManager uiManager;
    private PlayerResources playerResources;

    private int sectionIndex;

    private GameObject iconsContainer;  // 添加引用以便清理

    private ShopUIManager shopUIManager;

    private bool isPlayerInTrigger = false;  // 新增：跟踪玩家是否在触发器内

    private FruitData lastSoldFruit;  // 新增：记录上次卖出的水果

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

        // 在 Awake 中就获取引用
        uiManager = FindFirstObjectByType<UIManager>();
        playerResources = FindFirstObjectByType<PlayerResources>();
        
        // 初始化数组
        lotteryFruitIcons = new SpriteRenderer[2];
        
        // 创建水果图标对象，但初始时隐藏它们
        CreateFruitIcons();
        SetFruitIconsVisible(false);

        // 修改获取 ShopUIManager 的方式
        shopUIManager = GameObject.FindFirstObjectByType<ShopUIManager>();
        if (shopUIManager == null)
        {
            Debug.LogError("ShopUIManager not found in scene!");
        }
        else
        {
            Debug.Log("ShopUIManager found successfully");
        }
    }

    public void Initialize(Vector2Int position, TownType type)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer is null in Initialize!");
            return;
        }

        gridPosition = position;
        Debug.Log($"Town initialized at position: {position}, type: {type}");
        townType = type;
        
        // 设置外观和碰撞器
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;

        switch (townType)
        {
            case TownType.Core:
                transform.localScale = new Vector3(1f, 2f, 1f);
                spriteRenderer.sprite = spriteRenderer.sprite ?? CreateDefaultSprite();
                spriteRenderer.color = coreColor;
                // 调整位置和碰撞器大小
                transform.position = new Vector3(
                    position.x,
                    position.y + 0.5f,
                    transform.position.z
                );
                collider.size = new Vector2(1f, 2f);
                collider.offset = new Vector2(0f, 0f);
                Debug.Log($"Core town position set to: {transform.position}");
                break;
            
            case TownType.Satellite:
            case TownType.Normal:
                transform.localScale = Vector3.one;
                spriteRenderer.sprite = spriteRenderer.sprite ?? CreateDefaultSprite();
                spriteRenderer.color = (type == TownType.Satellite) ? 
                    new Color(0.3f, 0.8f, 0.3f) : normalColor;
                transform.position = new Vector3(position.x, position.y, transform.position.z);
                collider.size = Vector2.one;
                collider.offset = Vector2.zero;
                Debug.Log($"Normal/Satellite town position set to: {transform.position}");
                break;
        }

        // 计算所在区域
        sectionIndex = GameManager.Instance.GetSectionIndex(position.y);
        
        Debug.Log($"Initializing town at {position} with section index {sectionIndex}");
        
        // 如果已经被探索，则初始化水果
        if (isExplored)
        {
            InitializeFruits();
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
            
            // 当地块被探索时，初始化水果
            InitializeFruits();
        }
    }

    public bool IsExplored()
    {
        return isExplored;
    }

    private void OnDestroy()
    {
        // 清理动态创建的精灵和游戏对象
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            // 如果是我们动态创建的精灵，则销毁它
            if (spriteRenderer.sprite.texture.name == "")
            {
                Destroy(spriteRenderer.sprite.texture);
                Destroy(spriteRenderer.sprite);
            }
        }

        // 清理水果图标
        if (iconsContainer != null)
        {
            Destroy(iconsContainer);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isExplored) return;
        
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered trigger area of {gameObject.name} at position {gridPosition}");
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player exited trigger area of {gameObject.name}");
            isPlayerInTrigger = false;
            UpdateShopUIVisibility(false);
        }
    }

    public void CreateFruitIcons()
    {
        // 如果已经存在，先清理掉
        if (iconsContainer != null)
        {
            Destroy(iconsContainer);
        }

        // 创建一个容器对象来存放所有水果图标
        iconsContainer = new GameObject("FruitIcons");
        iconsContainer.transform.parent = this.transform;
        iconsContainer.transform.localPosition = Vector3.zero;

        // 初始化数组
        lotteryFruitIcons = new SpriteRenderer[2];

        // 创建奖池水果图标
        for (int i = 0; i < 2; i++)
        {
            GameObject iconObj = new GameObject($"LotteryFruitIcon_{i}");
            iconObj.transform.parent = iconsContainer.transform;
            iconObj.transform.localPosition = new Vector3(-0.4f + i * 0.4f, 0.4f, -0.1f);
            lotteryFruitIcons[i] = iconObj.AddComponent<SpriteRenderer>();
            lotteryFruitIcons[i].color = new Color(1, 1, 1, 0.7f);
            lotteryFruitIcons[i].sortingLayerName = "UI";
            lotteryFruitIcons[i].sortingOrder = 2;
            lotteryFruitIcons[i].enabled = false;
        }

        // 创建收购水果图标
        GameObject buyingIconObj = new GameObject("BuyingFruitIcon");
        buyingIconObj.transform.parent = iconsContainer.transform;
        buyingIconObj.transform.localPosition = new Vector3(0.4f, -0.4f, -0.1f);
        buyingFruitIcon = buyingIconObj.AddComponent<SpriteRenderer>();
        buyingFruitIcon.color = new Color(1, 1, 1, 0.7f);
        buyingFruitIcon.sortingLayerName = "UI";
        buyingFruitIcon.sortingOrder = 2;
        buyingFruitIcon.enabled = false;
    }

    private void SetFruitIconsVisible(bool visible)
    {
        if (iconsContainer == null) return;

        foreach (var icon in lotteryFruitIcons)
        {
            if (icon != null) icon.enabled = visible;
        }
        if (buyingFruitIcon != null) buyingFruitIcon.enabled = visible;
    }

    public void SetLotteryFruits(FruitData[] fruits)
    {
        if (fruits == null)
        {
            Debug.LogError("Trying to set null lottery fruits!");
            return;
        }

        lotteryFruits = fruits;
        Debug.Log($"Setting lottery fruits sprites for {fruits.Length} fruits"); // 添加调试日志
        
        // 更新图标
        for (int i = 0; i < 2; i++)
        {
            if (lotteryFruitIcons[i] != null && lotteryFruits[i] != null)
            {
                lotteryFruitIcons[i].sprite = lotteryFruits[i].icon;
                Debug.Log($"Set sprite for lottery icon {i}: {(lotteryFruits[i].icon != null ? "success" : "null sprite")}"); // 添加调试日志
            }
        }
    }

    public void RefreshBuyingFruit()
    {
        // 创建一个可用水果的列表
        List<FruitData> availableFruits = new List<FruitData>();
        var sectionFruits = GameManager.Instance.GameConfig.sectionFruits[sectionIndex].availableFruits;
        
        // 添加所有不在奖池中且不是上次卖出的水果的水果
        foreach (var fruit in sectionFruits)
        {
            // 检查是否在奖池中
            bool isLotteryFruit = false;
            for (int i = 0; i < lotteryFruits.Length; i++)
            {
                if (lotteryFruits[i] == fruit)
                {
                    isLotteryFruit = true;
                    break;
                }
            }

            bool isLastSoldFruit = (lastSoldFruit != null && fruit == lastSoldFruit);
            
            if (!isLotteryFruit && !isLastSoldFruit)
            {
                availableFruits.Add(fruit);
            }
        }
        
        if (availableFruits.Count > 0)
        {
            // 随机选择一个新的水果
            int randomIndex = UnityEngine.Random.Range(0, availableFruits.Count);
            buyingFruit = availableFruits[randomIndex];
            
            if (buyingFruitIcon != null)
            {
                buyingFruitIcon.sprite = buyingFruit.icon;
                buyingFruitIcon.enabled = true;
            }
            Debug.Log($"Refreshed buying fruit to: {buyingFruit.fruitName}");
        }
        else
        {
            Debug.LogWarning("No available fruits to select from!");
        }
    }

    private void Update()
    {
        // 检查玩家是否在城镇上并更新UI状态
        if (isPlayerInTrigger && isExplored)
        {
            bool shouldShowUI = IsPlayerOnTownGrid();
            UpdateShopUIVisibility(shouldShowUI);
        }

        // 当玩家在城镇上并按下回车时尝试出售水果
        if (Input.GetKeyDown(KeyCode.Return) && isPlayerInTrigger && IsPlayerOnTownGrid())
        {
            SellFruit();
        }
    }

    private void SellFruit()
    {
        if (buyingFruit != null)
        {
            // 计算实际价值（考虑区域倍率）
            int actualValue = Mathf.RoundToInt(
                buyingFruit.baseValue * GameManager.Instance.GameConfig.sectionFruits[sectionIndex].valueMultiplier
            );
            
            // 在现实中确认玩家有这个水果后，增加金币
            playerResources.AddCoins(actualValue);
            
            // 显示获得金币的动画
            if (uiManager != null)
            {
                uiManager.ShowFloatingText($"+{actualValue}", 
                    transform.position, GameManager.Instance.GameConfig.coinTextColor);
            }

            // 记录本次卖出的水果
            lastSoldFruit = buyingFruit;
            
            // 刷新收购水果
            RefreshBuyingFruit();
            
            Debug.Log($"Sold fruit for {actualValue} coins, last sold fruit: {lastSoldFruit.fruitName}");
        }
    }

    // 新增一个方法来处理水果的初始化
    private void InitializeFruits()
    {
        if (lotteryFruits == null || lotteryFruits[0] == null)
        {
            var fruits = GameManager.Instance.GetRandomLotteryFruits(sectionIndex);
            Debug.Log($"Setting lottery fruits: {(fruits != null ? fruits.Length : 0)} fruits");
            SetLotteryFruits(fruits);
        }
        
        if (buyingFruit == null)
        {
            RefreshBuyingFruit();
        }
        
        SetFruitIconsVisible(true);
    }

    private bool IsPlayerOnTownGrid()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return false;

        Vector2Int playerPos = player.CurrentGridPosition;
        
        // 对于核心城镇，检查两个格子
        if (townType == TownType.Core)
        {
            return playerPos == gridPosition || playerPos == new Vector2Int(gridPosition.x, gridPosition.y + 1);
        }
        
        return playerPos == gridPosition;
    }

    private void UpdateShopUIVisibility(bool show)
    {
        if (shopUIManager != null)
        {
            if (show)
            {
                Debug.Log($"Showing shop UI for {townType} at position {gridPosition}");
                shopUIManager.ShowShopUI(townType == TownType.Core);
            }
            else
            {
                Debug.Log($"Hiding shop UI for {townType}");
                shopUIManager.HideUI();
            }
        }
    }
} 