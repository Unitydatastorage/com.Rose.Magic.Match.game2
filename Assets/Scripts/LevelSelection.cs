using UnityEngine;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    public GameObject levelButtonPrefab; // Префаб кнопки уровня
    public Transform levelsPanel; // Панель для размещения кнопок
    public int numberOfLevels = 28; // Количество уровней

    void Start()
    {
        GenerateLevelButtons();
    }

    void GenerateLevelButtons()
    {
        for (int i = 1; i <= numberOfLevels; i++)
        {
            GameObject button = Instantiate(levelButtonPrefab, levelsPanel);
            button.GetComponentInChildren<Text>().text = "Level " + i;

            int levelIndex = i; // Локальная копия переменной для замыкания
            button.GetComponent<Button>().onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
        }
    }

    void OnLevelButtonClicked(int levelIndex)
    {
        Debug.Log("Level " + levelIndex + " button clicked!");
        // Здесь можно добавить логику для загрузки уровня
    }
}
