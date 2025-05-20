using System;

[Serializable]
public class QuestionData
{
    public string problemName;
    public string[] choices;           // 3��
    public string[] characterComment;  // 3��
    public string[] npcReplies;        // 3��
    public int correctIndex;           // 0, 1, 2
}
