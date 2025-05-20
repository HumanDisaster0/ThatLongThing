using System.Collections.Generic;
using UnityEngine;

public class QuizDataDebugger : MonoBehaviour
{
    void Start()
    {
        List<QuestionData> questions = CSVLoader.LoadQuestions();

        for (int i = 0; i < questions.Count; i++)
        {
            var q = questions[i];
            Debug.Log($"[���� {i}] �����̸�: {q.problemName}, ���� �ε���: {q.correctIndex}");

            for (int j = 0; j < 3; j++)
            {
                Debug.Log($"  ������ {j + 1}: {q.choices[j]}");
                Debug.Log($"    ���ΰ� ���: {q.characterComment[j]}");
                Debug.Log($"    NPC ���: {q.npcReplies[j]}");
            }
        }
    }
}
