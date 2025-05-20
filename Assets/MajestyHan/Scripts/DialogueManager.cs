using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Bubble Style")]
    public List<Sprite> bubbleStyles;
    public Image bubbleImage;
    private int currentStyleIndex = 0;

    [Header("UI Reference")]
    public GameObject panel;
    public TextMeshProUGUI dialogueText;
    public RectTransform bubbleBackground;

    private List<string> currentLines;
    private int currentIndex;
    private bool isPlaying = false;
    private bool waitingForInput = false;
    private Coroutine typingCoroutine;
    private Action onComplete;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        panel.SetActive(false);
    }

    public IEnumerator ShowSequence(List<string> lines, int styleIndex = 0, Action onCompleteCallback = null)
    {
        panel.SetActive(true);
        isPlaying = true;
        onComplete = onCompleteCallback;

        currentLines = lines;
        currentIndex = 0;
        ShowCurrentLine();

        while (isPlaying)
            yield return null;

        panel.SetActive(false);
        onComplete?.Invoke();
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
        waitingForInput = false;
        yield return StartCoroutine(TextTyper.TypeText(dialogueText, line, 0.04f));
        waitingForInput = true;
    }

    public void NextDialogue()
    {
        if (typingCoroutine != null && !waitingForInput)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentLines[currentIndex];
            typingCoroutine = null;
            waitingForInput = true;
            return;
        }

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

    public void SetBubbleStyle(int index)
    {
        if (index >= 0 && index < bubbleStyles.Count)
        {
            currentStyleIndex = index;
            bubbleImage.sprite = bubbleStyles[index];
        }
        else
        {
            Debug.LogWarning($"¸»Ç³¼± ½ºÅ¸ÀÏ ÀÎµ¦½º {index}°¡ ¹üÀ§¸¦ ¹þ¾î³µ½À´Ï´Ù.");
        }
    }
}
