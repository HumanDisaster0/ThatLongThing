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

    // 트리거 1 진입 시 호출
    public void TriggerPoint1()
    {
        player.Freeze = true;
        cameraController.ZoomToTarget(focusTarget1);

        List<string> lines = new List<string>
        {
            "여기는 첫 번째 지점이야.",
            "이곳에서 이동을 배울 수 있어."
        };

        StartCoroutine(TutorialSequence(lines));
    }

    // 트리거 2 진입 시 호출
    public void TriggerPoint2()
    {
        player.Freeze = true;
        cameraController.ZoomToTarget(focusTarget2);

        List<string> lines = new List<string>
        {
            "두 번째 지점이야.",
            "이제 점프하는 법을 알아보자!"
        };

        StartCoroutine(TutorialSequence(lines));
    }

    // 공통 연출 시퀀스
    IEnumerator TutorialSequence(List<string> lines)
    {
        yield return dialogue.ShowSequence(lines);            // 말풍선 출력
        cameraController.ResetZoom();                         // 줌 아웃
        yield return new WaitForSeconds(0.5f);                // 줌 아웃 시간 고려
        player.Freeze = false;                                // 입력 해제
    }
}
