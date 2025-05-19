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
    private bool isPlaying = false;
    private bool waitingForInput = false;

    private void Awake()
    {
        panel.SetActive(false);
    }

    public IEnumerator ShowSequence(List<string> lines)
    {
        panel.SetActive(true);
        isPlaying = true;

        currentLines = lines;
        currentIndex = 0;

        ShowCurrentLine();

        while (isPlaying)
            yield return null;

        panel.SetActive(false);
    }

    private void ShowCurrentLine()
    {
        if (currentIndex >= currentLines.Count)
        {
            isPlaying = false;
            return;
        }

        dialogueText.text = currentLines[currentIndex];
        waitingForInput = true;
    }

    public void NextDialogue()
    {
        if (!waitingForInput) return;

        waitingForInput = false;
        currentIndex++;
        ShowCurrentLine();
    }

    public void FlipBubble(bool flip)
    {
        Vector3 scale = bubbleBackground.localScale;
        scale.x = Mathf.Abs(scale.x) * (flip ? -1f : 1f);
        bubbleBackground.localScale = scale;
    }

    void Update()
    {
        if (isPlaying && waitingForInput && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            NextDialogue();
        }
    }
}
