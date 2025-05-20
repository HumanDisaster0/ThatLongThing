using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SettlementPanelScript : MonoBehaviour
{
    public GameObject Panel; // 외부에서 연결 가능한 패널 객체

    private TextMeshPro text1;
    private TextMeshPro text2;
    private TextMeshPro text3;
    private TextMeshPro text4;
    private TextMeshPro textTax;

    private CustomClickable closeButton;

    void Awake()
    {
        // 자식 오브젝트에서 TextMeshPro 컴포넌트를 찾음
        text1 = transform.GetChild(2).GetComponent<TextMeshPro>();
        text2 = transform.GetChild(3).GetComponent<TextMeshPro>();
        text3 = transform.GetChild(4).GetComponent<TextMeshPro>();
        text4 = transform.GetChild(5).GetComponent<TextMeshPro>();
        textTax = transform.GetChild(6).GetComponent<TextMeshPro>();

        Transform closeBtnTransform = transform.Find("CloseBtn");
        if (closeBtnTransform != null)
        {
            closeButton = closeBtnTransform.GetComponent<CustomClickable>();
            if (closeButton != null)
            {
                closeButton.SetClickAction(OnCloseButtonClicked);
            }
            else
            {
                Debug.LogWarning("CloseBtn에 CustomClickable 컴포넌트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("CloseBtn 오브젝트를 찾을 수 없습니다.");
        }
    }

    void Start()
    {
        if (GoldManager.Instance != null)
        {
            text1.text = $"{GoldManager.Instance.findTrapCount} (+{GoldManager.Instance.findTrapCount * 100})";
            text2.text = $"{GoldManager.Instance.deadCount} (-{GoldManager.Instance.deadCount * 20})";
            text3.text = $"{GoldManager.Instance.ejectionCount} (-{GoldManager.Instance.ejectionCount *100})";
            text4.text = $"{GoldManager.Instance.totalGold} ({GoldManager.Instance.rewardGold} 획득)";

            //text1.text = GoldManager.Instance.findTrapCount.ToString();
            //text2.text = GoldManager.Instance.deadCount.ToString();
            //text3.text = GoldManager.Instance.ejectionCount.ToString();
            //text4.text = GoldManager.Instance.rewardGold.ToString();
        }
    }

    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false); // 현재 SettlementPanel 비활성화
        GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.IDLE);

    }

    void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == "GuildMain")
        {
            if (GoldManager.Instance != null)
            {
                text1.text = GoldManager.Instance.findTrapCount.ToString();
                text2.text = GoldManager.Instance.deadCount.ToString();
                text3.text = GoldManager.Instance.ejectionCount.ToString();
                text4.text = GoldManager.Instance.rewardGold.ToString();
            }
        }
    }
}