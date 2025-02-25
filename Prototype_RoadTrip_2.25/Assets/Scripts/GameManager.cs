using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;
    public GameConfig GameConfig => gameConfig;

    private static GameManager instance;
    public static GameManager Instance => instance;

    private System.Random random = new System.Random();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public FruitData[] GetRandomLotteryFruits(int sectionIndex)
    {
        if (sectionIndex >= gameConfig.sectionFruits.Length)
        {
            Debug.LogError($"Invalid section index: {sectionIndex}");
            return null;
        }

        FruitData[] result = new FruitData[2];
        var sectionFruits = gameConfig.sectionFruits[sectionIndex].availableFruits;
        List<FruitData> availableFruits = new List<FruitData>(sectionFruits);
        
        // 随机选择两个不同的水果
        for (int i = 0; i < 2; i++)
        {
            if (availableFruits.Count == 0) break;
            int index = random.Next(availableFruits.Count);
            result[i] = availableFruits[index];
            availableFruits.RemoveAt(index);
        }
        
        return result;
    }

    public FruitData GetRandomFruit(int sectionIndex, FruitData[] excludeFruits)
    {
        if (sectionIndex >= gameConfig.sectionFruits.Length)
        {
            Debug.LogError($"Invalid section index: {sectionIndex}");
            return null;
        }

        var sectionFruits = gameConfig.sectionFruits[sectionIndex].availableFruits;
        List<FruitData> availableFruits = new List<FruitData>(sectionFruits);
        
        // 移除已在奖池中的水果
        foreach (var fruit in excludeFruits)
        {
            availableFruits.Remove(fruit);
        }
        
        if (availableFruits.Count > 0)
        {
            return availableFruits[random.Next(availableFruits.Count)];
        }
        
        return null;
    }

    public int GetSectionIndex(int yPosition)
    {
        return Mathf.Clamp(yPosition / (GridManager.HEIGHT / gameConfig.sectionFruits.Length), 
            0, gameConfig.sectionFruits.Length - 1);
    }

    private void Update()
    {
        // 按R键重置游戏
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }

        // 添加测试功能：按F1添加一点油
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var playerResources = FindFirstObjectByType<PlayerResources>();
            if (playerResources != null)
            {
                playerResources.ModifyFuel(1);
                Debug.Log("Added 1 fuel for testing");
            }
        }
    }

    private void ResetGame()
    {
        // 重新加载当前场景
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
} 