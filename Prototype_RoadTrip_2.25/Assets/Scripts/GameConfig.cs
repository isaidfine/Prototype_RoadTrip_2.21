using UnityEngine;
using System;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Player Resources")]
    public float maxFuel = 15f;
    public float maxStamina = 5f;
    public float maxCarDurability = 10f;
    
    [Header("Resource Consumption")]
    public float fuelConsumptionPerMove = 0.5f;
    public float staminaConsumptionPerMove = 0.2f;
    public float carDurabilityConsumptionPerMove = 0.3f;

    [Header("Fruit Settings")]
    public float noRewardChance = 0.2f;
    public float rewardChancePerFruit = 0.4f;
    [Range(10, 100)]
    public int baseFruitValue = 30;
    public float fruitValueVariance = 0.2f; // 价格浮动范围 ±20%

    [Header("UI Settings")]
    public float floatingTextDuration = 1f;
    public float floatingTextSpeed = 1f;
    public Color coinTextColor = Color.yellow;
    
    [Header("Animation Settings")]
    public float lotteryAnimationDuration = 1f;
    public int lotteryAnimationSpins = 10;

    [Header("Shop Settings")]
    public int fuelPrice = 5;  // 改为5金币/点燃油

    [Header("Upgrade Settings")]
    public int baseFuelCapacityUpgradeCost = 150;  // 略微提高，因为燃油便宜了
    public float upgradeCostMultiplier = 1.5f;     // 保持不变
    public int baseVehicleUpgradeCost = 300;       // 略微提高

    [Serializable]
    public class SectionFruits
    {
        public string sectionName;
        public FruitData[] availableFruits = new FruitData[5];
        [Range(0.5f, 2f)]
        public float valueMultiplier = 1f;
    }

    public SectionFruits[] sectionFruits;
} 