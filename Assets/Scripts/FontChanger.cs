using UnityEngine;
using UnityEngine.UI;
using TMPro; // Подключаем TextMeshPro

public class FontChanger : MonoBehaviour
{
    public Font newFont; // Новый шрифт для стандартных Text компонентов
    public TMP_FontAsset newTMPFont; // Новый шрифт для TextMeshPro компонентов

    void Start()
    {
        // Изменение шрифта для всех Text компонентов
        Text[] allTextComponents = FindObjectsOfType<Text>();
        foreach (Text textComponent in allTextComponents)
        {
            textComponent.font = newFont;
        }

        // Изменение шрифта для всех TextMeshProUGUI компонентов
        TextMeshProUGUI[] allTMPComponents = FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI tmpComponent in allTMPComponents)
        {
            tmpComponent.font = newTMPFont;
        }
    }
}