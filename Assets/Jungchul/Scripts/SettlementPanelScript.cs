using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class SettlementPanelScript : MonoBehaviour
{
    public GameObject Panel;

    private TextMeshPro text1;
    private TextMeshPro text2;
    private TextMeshPro text3;
    private TextMeshPro text4;
    private TextMeshPro textTax;
    private TextMeshPro textWeek;

    private CustomClickable closeButton;

    private GameObject settlementBg;
    private GameObject taxBg;

    public GameObject gc;

    public int grade;

    public GameObject gradeBox;
    public GameObject faceBox;

    public Sprite grAP;
    public Sprite grA;
    public Sprite grB;
    public Sprite grC;
    public Sprite grF;

    public Sprite fAP;
    public Sprite fA;
    public Sprite fB;
    public Sprite fC;
    public Sprite fF;




    void Awake()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }


        settlementBg = transform.Find("settlment_Bg")?.gameObject;
        taxBg = transform.Find("tax_bg")?.gameObject;

        if (settlementBg != null)
        {
            text1 = settlementBg.transform.Find("text1").GetComponent<TextMeshPro>();
            text2 = settlementBg.transform.Find("text2").GetComponent<TextMeshPro>();
            text3 = settlementBg.transform.Find("text3").GetComponent<TextMeshPro>();
            text4 = settlementBg.transform.Find("text4").GetComponent<TextMeshPro>();
        }
        else
        {
            Debug.LogError("settlment_Bg�� ã�� �� �����ϴ�.");
        }

        if (taxBg != null)
        {
            textTax = taxBg.transform.Find("textTax").GetComponent<TextMeshPro>();
            textWeek = taxBg.transform.Find("textWeek").GetComponent<TextMeshPro>();
        }
        else
        {
            Debug.LogError("tax_bg�� ã�� �� �����ϴ�.");
        }

        Transform closeBtnTransform = transform.Find("CloseBtn");
        if (closeBtnTransform != null)
        {
            closeButton = closeBtnTransform.GetComponent<CustomClickable>();
            if (closeButton != null)
                closeButton.SetClickAction(OnCloseButtonClicked);
            else
                Debug.LogWarning("CloseBtn�� CustomClickable ������Ʈ�� �����ϴ�.");
        }
        else
        {
            Debug.LogWarning("CloseBtn ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    void Start()
    {
        RefreshTexts();
    }

    public void RefreshTexts()
    {
        int rg = GoldManager.Instance.rewardGold;

        if (GoldManager.Instance != null)
        {
            text1.text = $"{GoldManager.Instance.findTrapCount} (+{GoldManager.Instance.findTrapCount * 5})";
            text2.text = $"{GoldManager.Instance.deadCount} (-{GoldManager.Instance.rdc * 2})";
            text3.text = $"{GoldManager.Instance.ejectionCount} (-{GoldManager.Instance.ejectionCount * 10})";

            if (rg >= 10)
            {
                grade = 1;
            }
            else if (rg >= 6)
            {
                grade = 2;
            }
            else if (rg >= 2)
            {
                grade = 3;
            }
            else if (rg >= 0)
            {
                grade = 4;
            }
            else if (rg < 0)
            {
                grade = 5;
            }

            text4.text = $"{GoldManager.Instance.totalGold} ({GoldManager.Instance.rewardGold} ȹ��)";
            textTax.text = $"-({GoldManager.Instance.Tax} )";
            textWeek.text = GuildRoomManager.Instance.week.ToString();

            var gradeRenderer = gradeBox?.GetComponent<SpriteRenderer>();
            var faceRenderer = faceBox?.GetComponent<SpriteRenderer>();

            if (gradeRenderer != null && faceRenderer != null)
            {
                switch (grade)
                {
                    case 1:
                        gradeRenderer.sprite = grAP;
                        faceRenderer.sprite = fAP;
                        break;
                    case 2:
                        gradeRenderer.sprite = grA;
                        faceRenderer.sprite = fA;
                        break;
                    case 3:
                        gradeRenderer.sprite = grB;
                        faceRenderer.sprite = fB;
                        break;
                    case 4:
                        gradeRenderer.sprite = grC;
                        faceRenderer.sprite = fC;
                        break;
                    case 5:
                    default:
                        gradeRenderer.sprite = grF;
                        faceRenderer.sprite = fF;
                        break;
                }
            }
            else
            {
                Debug.LogWarning("gradeBox �Ǵ� faceBox�� SpriteRenderer ������Ʈ�� �����ϴ�.");
            }
        }
    }

    public void OnCloseButtonClicked()
    {

        gameObject.SetActive(false);

        if (GuildRoomManager.Instance.trollCheck)
        {
            GuildRoomManager.Instance.cState = GuildRoomManager.counterState.TROLL;
            GuildRoomManager.Instance.trollCheck = false;
        }
        else
        {
            GuildRoomManager.Instance.cState = GuildRoomManager.counterState.QUIZ;
        }


    }

    void OnEnable()
    {
        if (GuildRoomManager.Instance.day == 4)
        {
            Vector3 tPosition = new Vector3(-3.52f, gameObject.transform.position.y, 0f);
            MoveSettlementBg(tPosition);
            taxBg.SetActive(true);
            textTax.gameObject.SetActive(true);
            GoldManager.Instance.MinusGold(60);
        }
        else
        {
            Vector3 sPosition = new Vector3(0.00f, gameObject.transform.position.y, 0f);
            MoveSettlementBg(sPosition);
            taxBg.SetActive(false);
            textTax.gameObject.SetActive(false);
        }

        if (SceneManager.GetActiveScene().name == "GuildMain")
        {
            RefreshTexts();
        }
    }

    // 
    public void MoveSettlementBg(Vector3 newPosition)
    {
        if (settlementBg != null)
            settlementBg.transform.localPosition = newPosition;
    }

    public void MoveTaxBg(Vector3 newPosition)
    {
        if (taxBg != null)
            taxBg.transform.localPosition = newPosition;
    }
}
