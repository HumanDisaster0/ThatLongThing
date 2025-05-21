using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour
{
    [Header("컷씬 데이터 (공통 진행)")]
    public List<Sprite> cutSceneImages;
    public List<string> cutSceneTexts;

    [Header("선택지 등장 시점 (몇 번째 컷씬 뒤?)")]
    public int choiceCutIndex = 4;

    [Header("분기1 (예: 선택지1)")]
    public List<Sprite> branch1Images;
    public List<string> branch1Texts;

    [Header("분기2 (예: 선택지2)")]
    public List<Sprite> branch2Images;
    public List<string> branch2Texts;

    [Header("UI 연결")]
    public Image cutSceneImage;
    public TextMeshProUGUI cutSceneText;
    public GameObject choicePanel;
    public Button choice1Btn;
    public Button choice2Btn;
    // ... 선택지 더 늘릴 수 있음

    private void Start()
    {
        choicePanel.SetActive(false);
        StartCoroutine(PlayCommonCutScenes());
    }

    // 공통 컷씬 연출
    private IEnumerator PlayCommonCutScenes()
    {
        for (int i = 0; i < cutSceneImages.Count && i < cutSceneTexts.Count; i++)
        {
            cutSceneImage.sprite = cutSceneImages[i];
            yield return StartCoroutine(TextTyper.TypeText(cutSceneText, cutSceneTexts[i], 0.04f, () => false));

            if (i == choiceCutIndex - 1)
            {
                // 선택지 등장
                yield return StartCoroutine(ShowChoice());
                yield break; // 분기로 넘어감
            }
        }
        // 만약 선택지 없이 바로 끝나야 하는 구조면 여기서 끝 처리
    }

    // 선택지 연출
    private IEnumerator ShowChoice()
    {
        choicePanel.SetActive(true);
        bool chosen = false;
        int branch = 0;

        // 선택지 버튼 이벤트 등록
        choice1Btn.onClick.AddListener(() => { chosen = true; branch = 1; });
        choice2Btn.onClick.AddListener(() => { chosen = true; branch = 2; });

        // 플레이어가 버튼 누를 때까지 대기
        yield return new WaitUntil(() => chosen);

        choicePanel.SetActive(false);

        // 분기 연출 시작
        if (branch == 1)
            yield return StartCoroutine(PlayBranchCutScene(branch1Images, branch1Texts));
        else if (branch == 2)
            yield return StartCoroutine(PlayBranchCutScene(branch2Images, branch2Texts));
        // ... 더 많은 선택지 확장 가능

        // 분기 컷씬 끝난 뒤 추가 연출/씬 전환 등
    }

    // 분기 컷씬 연출
    private IEnumerator PlayBranchCutScene(List<Sprite> images, List<string> texts)
    {
        for (int i = 0; i < images.Count && i < texts.Count; i++)
        {
            cutSceneImage.sprite = images[i];
            yield return StartCoroutine(TextTyper.TypeText(cutSceneText, texts[i], 0.04f, () => false));
        }
    }
}
