using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI dialogueText;
    public RectTransform bubbleBackground;

    private List<string> currentLines;
    private int currentIndex;
    private bool waitingForNext;

    public IEnumerator ShowSequence(List<string> lines)
    {
        panel.SetActive(true);
        currentLines = lines;
        currentIndex = 0;
        waitingForNext = false;

        dialogueText.text = currentLines[currentIndex];
        waitingForNext = true;

        while (currentIndex < currentLines.Count)
        {
            yield return null;
        }

        panel.SetActive(false);
    }

    public void NextDialogue()
    {
        if (!waitingForNext) return;

        currentIndex++;

        if (currentIndex >= currentLines.Count)
        {
            waitingForNext = false;
            return;
        }

        dialogueText.text = currentLines[currentIndex];
    }

    public void FlipBubble(bool flip)
    {
        Vector3 scale = bubbleBackground.localScale;
        scale.x = Mathf.Abs(scale.x) * (flip ? -1f : 1f);
        bubbleBackground.localScale = scale;
    }

    void Update()
    {
        // 입력 기반으로 넘어가게 할 경우 여기에 작성
        if (waitingForNext && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            NextDialogue();
        }
    }
}
