using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TalkCSVLoader
{
    public static List<TalkData> LoadTalk()
    {
        TextAsset talkText = Resources.Load<TextAsset>("talkText");

        if (talkText == null)
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�!");
            return new List<TalkData>();
        }

        string[] lines = talkText.text.Split('\n');
        List<TalkData> talkData = new List<TalkData>();

        foreach (string line in lines)
        {
            string[] cols = line.Split('/');

            if (cols.Length < 2)
                continue;

            int staffEmote;
            string cleanedText = cols[0].Replace("\"", "").Replace("/", "");
            if (!int.TryParse(cleanedText.Trim(), out staffEmote))
            {
                Debug.LogWarning($"�߸��� ���� ����: {cols[0].Trim()}");
                continue;
            }

            string staffComment = cols[1].Replace("\"", "").Replace("/", "").Trim();
            talkData.Add(new TalkData(staffEmote, staffComment));
        }

        return new List<TalkData>(talkData);
    }
}