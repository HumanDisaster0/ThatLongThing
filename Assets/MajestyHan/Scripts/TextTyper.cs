using System.Collections;
using TMPro;
using UnityEngine;

public static class TextTyper
{
    public static IEnumerator TypeText(TextMeshProUGUI target, string message, float speed = 0.04f)
    {
        target.text = "";

        foreach (char c in message)
        {
            target.text += c;
            yield return new WaitForSeconds(speed);
        }
    }
}
