using UnityEngine;

[System.Serializable]
public class FruitData
{
    public string fruitName;
    public Sprite icon;
    public int baseValue;

    public FruitData(string name, Sprite icon, int value)
    {
        this.fruitName = name;
        this.icon = icon;
        this.baseValue = value;
    }
} 