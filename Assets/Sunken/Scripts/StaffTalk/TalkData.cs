using System;

[Serializable]
public class TalkData
{
    public int staffEmote;
    public string staffComment;

    public TalkData(int staffEmote, string staffComment)
    {
        this.staffEmote = staffEmote;
        this.staffComment = staffComment;
    }
}


