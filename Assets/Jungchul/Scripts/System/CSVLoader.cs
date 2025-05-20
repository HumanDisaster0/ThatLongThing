using System.Collections.Generic;
using UnityEngine;

public static class CSVLoader
{
    public static List<QuestionData> LoadQuestions()
    {
        TextAsset csv = Resources.Load<TextAsset>("missionCheckText");
        string[] lines = csv.text.Split('\n');

        Dictionary<string, QuestionData> questionMap = new Dictionary<string, QuestionData>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split('/');

            if (cols.Length < 5) continue;

            string problemName = cols[0].Trim();
            string choice = cols[1].Trim();
            string characterComment = cols[2].Trim();
            string npcReply = cols[3].Trim();
            string correctMark = cols[4].Trim();

            if (!questionMap.ContainsKey(problemName))
            {
                questionMap[problemName] = new QuestionData
                {
                    problemName = problemName,
                    choices = new string[3],
                    characterComment = new string[3],
                    npcReplies = new string[3],
                    correctIndex = -1
                };
            }

            var q = questionMap[problemName];

            int slot = -1;
            for (int j = 0; j < 3; j++)
            {
                if (string.IsNullOrEmpty(q.choices[j]))
                {
                    slot = j;
                    break;
                }
            }

            if (slot != -1)
            {
                q.choices[slot] = choice;
                q.characterComment[slot] = characterComment;
                q.npcReplies[slot] = npcReply;

                if (correctMark == "O")
                    q.correctIndex = slot;
            }
        }

        return new List<QuestionData>(questionMap.Values);
    }
}
