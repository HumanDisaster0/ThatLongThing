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
        player.SkipInput = true;
        cameraController.ZoomToTarget(focusTarget1);

        List<string> lines = new List<string>
        {
           "이제 모든 교육과정이 끝났어요. ",
           "당신도 이제 정식으로 의뢰를 받을 수 있는 ", 
           "어엿한 탐험가가 되었답니다."
        };

        StartCoroutine(TutorialSequence(lines));
    }

    // 트리거 2 진입 시 호출
    public void TriggerPoint2()
    {
        player.SkipInput = true;
        cameraController.ZoomToTarget(focusTarget2);

        List<string> lines = new List<string>
        {
            "던전을 나오면 모험가 길드로 와서 ",
            "기록에 대한 보고를 하면 추후 결과에 ",
            "따라 보상이 지급된답니다."
        };

        StartCoroutine(TutorialSequence(lines));
    }

    // 공통 연출 시퀀스
    IEnumerator TutorialSequence(List<string> lines)
    {
        // 줌인 완료까지 대기
        yield return new WaitUntil(() => cameraController.IsZoomed);

        // 말풍선 출력
        yield return dialogue.ShowSequence(lines);

        // 줌아웃 시작
        cameraController.ResetZoom();

        // 줌아웃 완료까지 대기
        yield return new WaitUntil(() => !cameraController.IsZoomed);

        // 입력 다시 허용
        player.SkipInput = false;
    }
}
