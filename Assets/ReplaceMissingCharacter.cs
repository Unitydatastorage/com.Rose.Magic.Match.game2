using TMPro;
using UnityEngine;

public class ReplaceMissingCharacter : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;

    void Start()
    {
        if (textMeshPro != null)
        {
            string originalText = textMeshPro.text;
            string updatedText = originalText.Replace("\u201C", "\"");
            textMeshPro.text = updatedText;
        }
    }
}