using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Resource Bars")]
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Slider carDurabilitySlider;
    
    [Header("Resource Texts")]
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI carDurabilityText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Floating Text")]
    [SerializeField] private GameObject floatingTextPrefab;
    
    [Header("Step Counter")]
    [SerializeField] private TextMeshProUGUI stepCountText;

    private PlayerResources playerResources;
    private GameConfig gameConfig;

    private void Start()
    {
        playerResources = FindFirstObjectByType<PlayerResources>();
        gameConfig = GameManager.Instance.GameConfig;

        // 订阅事件
        playerResources.OnFuelChanged += UpdateFuelUI;
        playerResources.OnStaminaChanged += UpdateStaminaUI;
        playerResources.OnCarDurabilityChanged += UpdateCarDurabilityUI;
        playerResources.OnCoinsChanged += UpdateCoinsUI;
        playerResources.OnStepCountChanged += UpdateStepCountUI;
    }

    private void UpdateFuelUI(float current, float max)
    {
        fuelSlider.value = current / max;
        fuelText.text = $"{Mathf.Round(current)}/{max}";
    }

    private void UpdateStaminaUI(float current, float max)
    {
        staminaSlider.value = current / max;
        staminaText.text = $"{Mathf.Round(current)}/{max}";
    }

    private void UpdateCarDurabilityUI(float current, float max)
    {
        carDurabilitySlider.value = current / max;
        carDurabilityText.text = $"{Mathf.Round(current)}/{max}";
    }

    private void UpdateCoinsUI(int coins)
    {
        coinsText.text = coins.ToString();
    }

    private void UpdateStepCountUI(int steps)
    {
        if (stepCountText != null)
        {
            stepCountText.text = $"Steps: {steps}";
        }
    }

    public void ShowFloatingText(string text, Vector3 worldPosition, Color color)
    {
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingText prefab is not set in UIManager!");
            return;
        }

        GameObject floatingText = Instantiate(floatingTextPrefab, worldPosition, Quaternion.identity);
        TextMeshPro textComponent = floatingText.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
            
            var floatingTextComponent = floatingText.GetComponent<FloatingText>();
            if (floatingTextComponent != null)
            {
                floatingTextComponent.Initialize(GameManager.Instance.GameConfig.floatingTextDuration, 
                    GameManager.Instance.GameConfig.floatingTextSpeed);
            }
        }
    }
} 