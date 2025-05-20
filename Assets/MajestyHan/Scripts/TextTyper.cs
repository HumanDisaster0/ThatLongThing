using System.Collections;
using TMPro;
using UnityEngine;

public static class TextTyper
{
    public static IEnumerator TypeText(TextMeshProUGUI target, string message, float speed = 0.04f)
    {
        target.text = "";

        int i = 0;
        while (i < message.Length)
        {
            // ��ġ �ؽ�Ʈ �±� ����
            if (message[i] == '<')
            {
                int tagCloseIndex = message.IndexOf('>', i);
                if (tagCloseIndex != -1)
                {
                    string tag = message.Substring(i, tagCloseIndex - i + 1);
                    target.text += tag;
                    i = tagCloseIndex + 1;
                    continue;
                }
            }

            target.text += message[i];
            i++;
            yield return new WaitForSeconds(speed);
        }
    }
}

