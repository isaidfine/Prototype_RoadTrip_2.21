using UnityEngine;
using System;

public class PlayerResources : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;
    
    public float CurrentFuel { get; private set; }
    public float CurrentStamina { get; private set; }
    public float CurrentCarDurability { get; private set; }
    public int Coins { get; private set; }
    public float MaxFuel { get; private set; }
    public int StepCount { get; private set; }  // 新增：计步器

    public event Action<float, float> OnFuelChanged;
    public event Action<float, float> OnStaminaChanged;
    public event Action<float, float> OnCarDurabilityChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnStepCountChanged;  // 新增：计步器变化事件

    private void Start()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        MaxFuel = gameConfig.maxFuel;
        CurrentFuel = MaxFuel;
        CurrentStamina = gameConfig.maxStamina;
        CurrentCarDurability = gameConfig.maxCarDurability;
        Coins = 0;
        StepCount = 0;

        // 触发初始事件
        OnFuelChanged?.Invoke(CurrentFuel, MaxFuel);
        OnStaminaChanged?.Invoke(CurrentStamina, gameConfig.maxStamina);
        OnCarDurabilityChanged?.Invoke(CurrentCarDurability, gameConfig.maxCarDurability);
        OnCoinsChanged?.Invoke(Coins);
        OnStepCountChanged?.Invoke(StepCount);
    }

    public bool CanMove()
    {
        return CurrentFuel > 0 && CurrentStamina > 0 && CurrentCarDurability > 0;
    }

    public void ConsumeMovementResources()
    {
        ModifyFuel(-gameConfig.fuelConsumptionPerMove);
        ModifyStamina(-gameConfig.staminaConsumptionPerMove);
        ModifyCarDurability(-gameConfig.carDurabilityConsumptionPerMove);
    }

    public void ModifyFuel(float amount)
    {
        float newFuel = Mathf.Clamp(CurrentFuel + amount, 0, MaxFuel);
        if (newFuel != CurrentFuel)
        {
            CurrentFuel = newFuel;
            OnFuelChanged?.Invoke(CurrentFuel, MaxFuel);
        }
    }

    public void ModifyStamina(float amount)
    {
        float newStamina = Mathf.Clamp(CurrentStamina + amount, 0, gameConfig.maxStamina);
        if (newStamina != CurrentStamina)
        {
            CurrentStamina = newStamina;
            OnStaminaChanged?.Invoke(CurrentStamina, gameConfig.maxStamina);
        }
    }

    public void ModifyCarDurability(float amount)
    {
        float newDurability = Mathf.Clamp(CurrentCarDurability + amount, 0, gameConfig.maxCarDurability);
        if (newDurability != CurrentCarDurability)
        {
            CurrentCarDurability = newDurability;
            OnCarDurabilityChanged?.Invoke(CurrentCarDurability, gameConfig.maxCarDurability);
        }
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    public bool CanRollDice()
    {
        return CurrentStamina >= 1;  // 检查是否有足够的疲劳值投骰子
    }

    public bool CanMoveSteps(int steps)
    {
        return CurrentFuel >= steps;  // 检查是否有足够的燃油走指定步数
    }

    public void ConsumeDiceRoll()
    {
        ModifyStamina(-1);  // 投骰子消耗1点疲劳值
    }

    public void ConsumeMovementBySteps(int steps)
    {
        ModifyFuel(-steps);  // 每步消耗1点燃油
        ModifyCarDurability(-gameConfig.carDurabilityConsumptionPerMove);  // 仍然消耗耐久
    }

    public void IncreaseFuelCapacity(float amount)
    {
        MaxFuel += amount;
        OnFuelChanged?.Invoke(CurrentFuel, MaxFuel);
    }

    // 新增：增加步数的方法
    public void IncrementStepCount()
    {
        StepCount++;
        OnStepCountChanged?.Invoke(StepCount);
        Debug.Log($"Steps taken: {StepCount}");
    }
} 