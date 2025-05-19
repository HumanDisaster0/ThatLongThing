using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public DialogueManager dialogue;
    public TutorialCameraController cameraController;
    public Transform focusTarget1;
    public Transform focusTarget2;
    public PlayerController player;

    // Ʈ���� 1 ���� �� ȣ��
    public void TriggerPoint1()
    {
        player.Freeze = true;
        cameraController.ZoomToTarget(focusTarget1);

        List<string> lines = new List<string>
        {
            "����� ù ��° �����̾�.",
            "�̰����� �̵��� ��� �� �־�."
        };

        StartCoroutine(TutorialSequence(lines));
    }

    // Ʈ���� 2 ���� �� ȣ��
    public void TriggerPoint2()
    {
        player.Freeze = true;
        cameraController.ZoomToTarget(focusTarget2);

        List<string> lines = new List<string>
        {
            "�� ��° �����̾�.",
            "���� �����ϴ� ���� �˾ƺ���!"
        };

        StartCoroutine(TutorialSequence(lines));
    }

    // ���� ���� ������
    IEnumerator TutorialSequence(List<string> lines)
    {
        yield return dialogue.ShowSequence(lines);            // ��ǳ�� ���
        cameraController.ResetZoom();                         // �� �ƿ�
        yield return new WaitForSeconds(0.5f);                // �� �ƿ� �ð� ���
        player.Freeze = false;                                // �Է� ����
    }
}
