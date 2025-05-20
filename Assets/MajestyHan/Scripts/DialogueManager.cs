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
    
    public void SetInputEnabled(bool enabled) => allowInput = enabled;
    private bool allowInput = true;

    private List<string> currentLines;
    private int currentIndex;
    private bool isPlaying = false;
    private bool waitingForInput = false;
    private Coroutine typingCoroutine;
    private Action onComplete;

    private bool skipRequested = false;
    private string currentTypingLine;

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
        skipRequested = false;
        currentTypingLine = line;
        yield return StartCoroutine(TextTyper.TypeText(dialogueText, line, 0.04f, () => skipRequested));
        waitingForInput = true;
    }

    public void NextDialogue()
    {
        if (typingCoroutine != null && !waitingForInput)
        {
            // Skip 요청 플래그만 켜주면, 코루틴 안에서 전체 출력하고 끝냄
            skipRequested = true;
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
        if (!isPlaying || !allowInput) return;

        // 타이핑 도중 스킵 처리
        if (!waitingForInput && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            NextDialogue(); // -> skipRequested = true 가 됨
            return;
        }

        // 타이핑 끝난 후 다음 대사 넘김
        if (waitingForInput && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
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
            Debug.LogWarning($"말풍선 스타일 인덱스 {index}가 범위를 벗어났습니다.");
        }
    }
}
