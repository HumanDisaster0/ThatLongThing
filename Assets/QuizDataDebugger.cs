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
            Debug.Log($"[문제 {i}] 문제이름: {q.problemName}, 정답 인덱스: {q.correctIndex}");

            for (int j = 0; j < 3; j++)
            {
                Debug.Log($"  선택지 {j + 1}: {q.choices[j]}");
                Debug.Log($"    주인공 대사: {q.characterComment[j]}");
                Debug.Log($"    NPC 대사: {q.npcReplies[j]}");
            }
        }
    }
}
