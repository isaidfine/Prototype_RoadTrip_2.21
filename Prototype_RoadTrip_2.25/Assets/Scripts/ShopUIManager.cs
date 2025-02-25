using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    [Header("Shop Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button buyFuelButton;
    [SerializeField] private TextMeshProUGUI fuelPriceText;
    
    [Header("Upgrade Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button fuelCapacityUpgradeButton;
    [SerializeField] private Button vehicleUpgradeButton;
    [SerializeField] private TextMeshProUGUI fuelCapacityUpgradeCostText;
    [SerializeField] private TextMeshProUGUI vehicleUpgradeCostText;
    [SerializeField] private TextMeshProUGUI currentFuelCapacityText;
    [SerializeField] private TextMeshProUGUI currentVehicleLevelText;

    private PlayerResources playerResources;
    private GameConfig gameConfig;
    private int vehicleLevel = 1;
    private int fuelCapacityUpgradeCount = 0;

    private void Start()
    {
        // 检查所有必要的引用
        if (shopPanel == null)
        {
            Debug.LogError("Shop Panel reference is missing!");
            return;
        }
        if (upgradePanel == null)
        {
            Debug.LogError("Upgrade Panel reference is missing!");
            return;
        }
        if (buyFuelButton == null)
        {
            Debug.LogError("Buy Fuel Button reference is missing!");
            return;
        }

        playerResources = FindFirstObjectByType<PlayerResources>();
        if (playerResources == null)
        {
            Debug.LogError("PlayerResources not found!");
            return;
        }

        gameConfig = GameManager.Instance.GameConfig;
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig not found!");
            return;
        }

        // 初始化UI
        InitializeUI();
        
        // 默认隐藏面板
        shopPanel.SetActive(false);
        upgradePanel.SetActive(false);

        // 订阅金币变化事件
        playerResources.OnCoinsChanged += (coins) => UpdateUpgradeUI();
        
        Debug.Log("ShopUIManager initialized successfully");
    }

    private void InitializeUI()
    {
        // 设置燃油价格文本
        fuelPriceText.text = $"Price: {gameConfig.fuelPrice}";

        // 设置按钮监听
        buyFuelButton.onClick.AddListener(BuyFuel);
        fuelCapacityUpgradeButton.onClick.AddListener(UpgradeFuelCapacity);
        vehicleUpgradeButton.onClick.AddListener(UpgradeVehicle);

        UpdateUpgradeUI();
    }

    public void ShowShopUI(bool inCity)
    {
        Debug.Log($"ShowShopUI called, inCity: {inCity}");
        Debug.Log($"Shop Panel null? {shopPanel == null}");
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            Debug.Log($"Set shopPanel active to true, current state: {shopPanel.activeSelf}");
        }
        
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(inCity);
            Debug.Log($"Set upgradePanel active to {inCity}, current state: {upgradePanel.activeSelf}");
        }
        
        UpdateUpgradeUI();
    }

    public void HideUI()
    {
        shopPanel.SetActive(false);
        upgradePanel.SetActive(false);
    }

    private void BuyFuel()
    {
        if (playerResources.Coins >= gameConfig.fuelPrice && 
            playerResources.CurrentFuel < playerResources.MaxFuel)
        {
            playerResources.AddCoins(-gameConfig.fuelPrice);
            playerResources.ModifyFuel(1);
        }
    }

    private void UpgradeFuelCapacity()
    {
        int cost = CalculateFuelCapacityUpgradeCost();
        if (playerResources.Coins >= cost)
        {
            playerResources.AddCoins(-cost);
            fuelCapacityUpgradeCount++;
            playerResources.IncreaseFuelCapacity(1);
            UpdateUpgradeUI();
        }
    }

    private void UpgradeVehicle()
    {
        int cost = CalculateVehicleUpgradeCost();
        if (playerResources.Coins >= cost)
        {
            playerResources.AddCoins(-cost);
            vehicleLevel++;
            UpdateUpgradeUI();
        }
    }

    private void UpdateUpgradeUI()
    {
        Debug.Log("UpdateUpgradeUI called");
        
        if (playerResources == null || gameConfig == null)
        {
            Debug.LogError($"Missing references - PlayerResources: {playerResources != null}, GameConfig: {gameConfig != null}");
            return;
        }

        // 更新升级费用显示
        int fuelUpgradeCost = CalculateFuelCapacityUpgradeCost();
        int vehicleUpgradeCost = CalculateVehicleUpgradeCost();

        fuelCapacityUpgradeCostText.text = $"Cost: {fuelUpgradeCost}";
        vehicleUpgradeCostText.text = $"Cost: {vehicleUpgradeCost}";

        // 更新当前状态显示
        currentFuelCapacityText.text = $"Current: {playerResources.MaxFuel}";
        currentVehicleLevelText.text = $"Level: {vehicleLevel}";

        // 检查按钮状态条件
        bool canBuyFuel = playerResources.Coins >= gameConfig.fuelPrice && 
                         playerResources.CurrentFuel < playerResources.MaxFuel;
        bool canUpgradeFuelCapacity = playerResources.Coins >= fuelUpgradeCost;
        bool canUpgradeVehicle = playerResources.Coins >= vehicleUpgradeCost;

        // 设置按钮状态
        buyFuelButton.interactable = canBuyFuel;
        fuelCapacityUpgradeButton.interactable = canUpgradeFuelCapacity;
        vehicleUpgradeButton.interactable = canUpgradeVehicle;

        // 添加调试日志
        Debug.Log($"Button states - Buy Fuel: {canBuyFuel}, " +
                  $"Upgrade Fuel Capacity: {canUpgradeFuelCapacity}, " +
                  $"Upgrade Vehicle: {canUpgradeVehicle}");
    }

    private int CalculateFuelCapacityUpgradeCost()
    {
        return Mathf.RoundToInt(gameConfig.baseFuelCapacityUpgradeCost * 
            Mathf.Pow(gameConfig.upgradeCostMultiplier, fuelCapacityUpgradeCount));
    }

    private int CalculateVehicleUpgradeCost()
    {
        return Mathf.RoundToInt(gameConfig.baseVehicleUpgradeCost * 
            Mathf.Pow(gameConfig.upgradeCostMultiplier, vehicleLevel - 1));
    }
} 