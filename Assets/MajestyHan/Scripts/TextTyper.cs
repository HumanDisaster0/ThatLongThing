using System.Collections;
using TMPro;
using UnityEngine;

public static class TextTyper
{
    public static IEnumerator TypeText(TextMeshProUGUI target, string message, float speed, System.Func<bool> isSkipRequested)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int i = 0;

        while (i < message.Length)
        {
            if (isSkipRequested())
            {
                target.text = message; // ÀüÃ¼ ¹®Àå µ¤¾î¾º¿ì±â
                yield break;
            }

            if (message[i] == '<')
            {
                int tagCloseIndex = message.IndexOf('>', i);
                if (tagCloseIndex != -1)
                {
                    string tag = message.Substring(i, tagCloseIndex - i + 1);
                    builder.Append(tag);
                    target.text = builder.ToString();
                    i = tagCloseIndex + 1;
                    continue;
                }
            }

            builder.Append(message[i]);
            target.text = builder.ToString();
            i++;
            yield return new WaitForSeconds(speed);
        }
    }

}

