using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour
{
    [Header("�ƾ� ������ (���� ����)")]
    public List<Sprite> cutSceneImages;
    public List<string> cutSceneTexts;

    [Header("������ ���� ���� (�� ��° �ƾ� ��?)")]
    public int choiceCutIndex = 4;

    [Header("�б�1 (��: ������1)")]
    public List<Sprite> branch1Images;
    public List<string> branch1Texts;

    [Header("�б�2 (��: ������2)")]
    public List<Sprite> branch2Images;
    public List<string> branch2Texts;

    [Header("UI ����")]
    public Image cutSceneImage;
    public TextMeshProUGUI cutSceneText;
    public GameObject choicePanel;
    public Button choice1Btn;
    public Button choice2Btn;
    // ... ������ �� �ø� �� ����

    private void Start()
    {
        choicePanel.SetActive(false);
        StartCoroutine(PlayCommonCutScenes());
    }

    // ���� �ƾ� ����
    private IEnumerator PlayCommonCutScenes()
    {
        for (int i = 0; i < cutSceneImages.Count && i < cutSceneTexts.Count; i++)
        {
            cutSceneImage.sprite = cutSceneImages[i];
            yield return StartCoroutine(TextTyper.TypeText(cutSceneText, cutSceneTexts[i], 0.04f, () => false));

            if (i == choiceCutIndex - 1)
            {
                // ������ ����
                yield return StartCoroutine(ShowChoice());
                yield break; // �б�� �Ѿ
            }
        }
        // ���� ������ ���� �ٷ� ������ �ϴ� ������ ���⼭ �� ó��
    }

    // ������ ����
    private IEnumerator ShowChoice()
    {
        choicePanel.SetActive(true);
        bool chosen = false;
        int branch = 0;

        // ������ ��ư �̺�Ʈ ���
        choice1Btn.onClick.AddListener(() => { chosen = true; branch = 1; });
        choice2Btn.onClick.AddListener(() => { chosen = true; branch = 2; });

        // �÷��̾ ��ư ���� ������ ���
        yield return new WaitUntil(() => chosen);

        choicePanel.SetActive(false);

        // �б� ���� ����
        if (branch == 1)
            yield return StartCoroutine(PlayBranchCutScene(branch1Images, branch1Texts));
        else if (branch == 2)
            yield return StartCoroutine(PlayBranchCutScene(branch2Images, branch2Texts));
        // ... �� ���� ������ Ȯ�� ����

        // �б� �ƾ� ���� �� �߰� ����/�� ��ȯ ��
    }

    // �б� �ƾ� ����
    private IEnumerator PlayBranchCutScene(List<Sprite> images, List<string> texts)
    {
        for (int i = 0; i < images.Count && i < texts.Count; i++)
        {
            cutSceneImage.sprite = images[i];
            yield return StartCoroutine(TextTyper.TypeText(cutSceneText, texts[i], 0.04f, () => false));
        }
    }
}
