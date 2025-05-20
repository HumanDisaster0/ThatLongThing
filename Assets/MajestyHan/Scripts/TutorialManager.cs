using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public TutorialCameraController cameraController;
    public PlayerController player;

    public Transform focusTarget1;
    public Transform focusTarget2;

    public void TriggerPointA()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_A());
    }

    public void TriggerPointB()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_B());
    }


    public void TriggerPointC()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_C());
    }

    public void TriggerPointD()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_D());
    }
    public void TriggerPointE()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_E());
    }

    public void TriggerPointF()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_F());
    }



    public void TriggerPointG()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_G());
    }


    //이후 미사용 대화
    public void TriggerPointH()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_H());
    }

    public void TriggerPointI()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_I());
    }

    public void TriggerPointJ()
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);
        cameraController.ZoomToTarget(focusTarget1);

        StartCoroutine(Cutscene_J());
    }

    //=====================================================================================================================
    //대화 A
    //=====================================================================================================================
    IEnumerator Cutscene_A()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
         "<color=#3f3f3f>환영해! 던전은 처음이지?",
         "<color=#3f3f3f>데비 넌 지금부터 모험가 길드 소속의 탐사원으로서 근무하게 되었어.",
         "<color=#3f3f3f>첫날은 내가 자세히 설명을 해줄 테니까 잘 듣도록 해!"
    });

        // 데비 대사
        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
          "<color=#3f3f3f>네, 선배님."
    });

        // 다시 선배
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>우선 지도를 열어볼래? [M]키를 누르면 돼."
    });

        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }

    //=====================================================================================================================
    //대화 B
    //=====================================================================================================================
    IEnumerator Cutscene_B()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>잘 보고 있지? \n우리 탐험가들의 지도에는 이 던전 안의 함정들이 미리 기록되어 있어.",
        "<color=#3f3f3f>어디에 위치해 있는지, 어떻게 작동하는지 쓰여 있는 거야.",
        "<color=#3f3f3f>예를 들어 우리 바로 앞에 있는 함정의 경우 \n아래로 화살표가 있으니 이 위에 올라가면 바닥이 아래로 꺼진다는 거지.",
        "<color=#3f3f3f>확인을 해야 하니까 한번 직접 위에 올라가 봐."
    });
        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }

    //=====================================================================================================================
    //대화 C
    //=====================================================================================================================
    IEnumerator Cutscene_C()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>잘했어! \n그럼 이 함정은 제대로 작동하고 있는 거야." ,
        "<color=#3f3f3f>하지만 가끔 지도와 다르게 작동하고 있는 함정이 있는데, \n그건 지도에 따로 마우스 왼쪽클릭으로 표기를 해줘야 해.",
        "<color=#3f3f3f>한번 클릭하면 함정이 제대로 발동하는 거고, \n두번 클릭하면 함정이 오작동을 하고 있다는 표시야." ,
        "<color=#3f3f3f>그럼 다음 함정을 확인해 보자."
    });

        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
    //=====================================================================================================================
    //대화 D
    //=====================================================================================================================
    IEnumerator Cutscene_D()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>마법을 사용하면 마법 범위 내에서 함정을 탐지할 수 있어.",
        "<color=#3f3f3f>한 번 해볼래? \n마법은 [Q]키를 누르면 사용할 수 있어."
    });
        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
    //=====================================================================================================================
    //대화 E
    //=====================================================================================================================
    IEnumerator Cutscene_E()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>어라, 네 마법은 조금 특이한 것 같네?",
        "<color=#3f3f3f>뭐, 그래도 탐지는 잘 되니까 됐어. \n이제 저 함정 위로 올라가 볼래?"
    });


        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
    //=====================================================================================================================
    //대화 F
    //=====================================================================================================================
    IEnumerator Cutscene_F()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>이 함정의 경우는 지도에는 위로 올라간다고 되어 있는데, \n작동을 안 하네." ,
        "<color=#3f3f3f>그런 경우에는 오류로 판단해서 지도에 표기를 해야 해." ,
        "<color=#3f3f3f>지도를 펼쳐서 오작동한 함정 위를 클릭해 봐."
    });        

        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
    //=====================================================================================================================
    //대화 G
    //=====================================================================================================================
    IEnumerator Cutscene_G()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>좋아, 거의 다 왔어. 마지막으로 여기엔 돌이 있으니 이렇게!"
        });

        //(무빙으로 돌을 피하는 선배!@#$)    
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>자 봤지? \n이런 식으로 던전 내부의 모든 함정의 작동을 확인하고, \n기록하는 것이 우리의 일이야.",
        "<color=#3f3f3f>함정은 던전에 들어갈 때마다 랜덤으로 바뀌게 되니 항상 꼼꼼하게 확인해야 해!",
        "<color=#3f3f3f>이제 이 던전의 함정은 다 기록했으니, \n포탈을 위쪽 방향키로 타서 모험가 길드로 돌아가 보고를 하는 일만 남았어.",
    });

        //(앞으로 먼저 나아가는 선배)!@#$

        // 데비 대사
        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>선배님, 위에 함정이 하나 더 있는 것 같은데요?"
    });
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>뭐? 그럴 리가. 내 마법 탐지 결과에는 이미 작동한 함정이라는데."
    });

        //(심하게 흔들리는 천장!@#$)


        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>안돼, 선배님!"
    });

        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "<color=#3f3f3f>응?"
    });
        //(돌이 떨어진다!@#$)

        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
    //=====================================================================================================================
    //대화 H -- 이후 미사용
    //=====================================================================================================================
    IEnumerator Cutscene_H()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "선배: 여긴 신입이 처음 오는 던전이야.",
        "잘 적응하도록 도와줄게."
    });

        // 데비 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "데비: 감사합니다! 잘 부탁드려요."
    });

        // 다시 선배
        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "선배: 자, 그럼 출발하자!"
    });

        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
    //=====================================================================================================================
    //대화 I
    //=====================================================================================================================
    IEnumerator Cutscene_I()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "선배: 여긴 신입이 처음 오는 던전이야.",
        "잘 적응하도록 도와줄게."
    });

        // 데비 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "데비: 감사합니다! 잘 부탁드려요."
    });

        // 다시 선배
        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "선배: 자, 그럼 출발하자!"
    });

        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
    //=====================================================================================================================
    //대화 J
    //=====================================================================================================================
    IEnumerator Cutscene_J()
    {
        // 줌인 + 레터박스 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 선배 대사
        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "선배: 여긴 신입이 처음 오는 던전이야.",
        "잘 적응하도록 도와줄게."
    });

        // 데비 대사
        DialogueManager.Instance.SetBubbleStyle(0);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "데비: 감사합니다! 잘 부탁드려요."
    });

        // 다시 선배
        DialogueManager.Instance.SetBubbleStyle(1);
        yield return DialogueManager.Instance.ShowSequence(new List<string>
    {
        "선배: 자, 그럼 출발하자!"
    });

        // 줌아웃
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
    //=====================================================================================================================    
    IEnumerator RunCutscene(Transform focusTarget, int styleIndex, List<string> lines)
    {
        player.SkipInput = true;
        player.SetVelocity(Vector2.zero);

        // 줌인 + 레터박스 자동 연출
        cameraController.ZoomToTarget(focusTarget);

        // 레터박스와 카메라 연출이 모두 끝날 때까지 대기
        yield return new WaitUntil(() => cameraController.IsZoomFullyReady);

        // 대사 출력
        DialogueManager.Instance.SetBubbleStyle(styleIndex);
        yield return DialogueManager.Instance.ShowSequence(lines);

        // 줌아웃 + 레터박스 닫기
        cameraController.ResetZoom();
        yield return new WaitUntil(() => !cameraController.IsZoomFullyReady);

        player.SkipInput = false;
    }
}
