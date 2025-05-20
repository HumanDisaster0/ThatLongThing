using System;

[Serializable]
public class QuestionData
{
    public string problemName;
    public string[] choices;           // 3°³
    public string[] characterComment;  // 3°³
    public string[] npcReplies;        // 3°³
    public int correctIndex;           // 0, 1, 2
}
