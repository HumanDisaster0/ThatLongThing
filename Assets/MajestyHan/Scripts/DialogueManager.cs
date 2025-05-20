using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI dialogueText;
    public RectTransform bubbleBackground;

    private List<string> currentLines;
    private int currentIndex;
    private bool isPlaying = false;
    private bool waitingForInput = false;
    private Coroutine typingCoroutine;

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

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(currentLines[currentIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        waitingForInput = false;

        yield return null;

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.04f);
        }

        waitingForInput = true;
    }

    public void NextDialogue()
    {
        // 타이핑 중이라면 전체 문장 즉시 출력
        if (typingCoroutine != null && !waitingForInput)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentLines[currentIndex];
            typingCoroutine = null;
            waitingForInput = true;
            return;
        }

        // 아직 대기 중이 아니라면 무시
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
