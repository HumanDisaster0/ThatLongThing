using System;
using System.Collections;
using TMPro;
using UnityEngine;

public static class TextTyper
{
    // 기존 방식도 지원
    // 기존: accelerateInsteadOfSkip 방식
    public static IEnumerator TypeText(TextMeshProUGUI target, string message, float baseSpeed, bool accelerateInsteadOfSkip = false)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int i = 0;

        while (i < message.Length)
        {
            if (!accelerateInsteadOfSkip && (Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0)))
            {
                target.text = message;
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

            SoundManager.instance?.PlayNewBackSound("Text_Write");

            builder.Append(message[i]);
            target.text = builder.ToString();
            i++;

            float delay = baseSpeed;
            if (accelerateInsteadOfSkip && (Input.GetKey(KeyCode.Z) || Input.GetMouseButton(0)))
                delay *= 0.2f;

            yield return new WaitForSeconds(delay);
        }
    }

    // 추가: skipRequested 방식 (기존 DialogueManager 호환)
    public static IEnumerator TypeText(TextMeshProUGUI target, string message, float baseSpeed, Func<bool> isSkipRequested)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        int i = 0;

        while (i < message.Length)
        {
            if (isSkipRequested != null && isSkipRequested())
            {
                target.text = message;
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

            SoundManager.instance?.PlayNewBackSound("Text_Write");

            builder.Append(message[i]);
            target.text = builder.ToString();
            i++;

            yield return new WaitForSeconds(baseSpeed);
        }
    }


}
