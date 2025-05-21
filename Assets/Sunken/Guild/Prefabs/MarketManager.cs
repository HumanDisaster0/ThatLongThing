using TMPro;
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }

    [Header("������ ����")]
    public int doubleJumpPrice = 60;
    public int shieldPrice = 20;
    public int magicSizePrice = 40;

    [SerializeField] private CustomClickable doubleJumpItem;
    [SerializeField] private CustomClickable shieldItem;
    [SerializeField] private CustomClickable magicSizeItem;

    int doubleJumpLevel = 0;
    int shieldLevel = 0;
    int magicSizeLevel = 0;

    int prevGold;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // ������ �ε�� ������ �������� �ʱ�ȭ�մϴ�.
        OnLevelWasLoaded(0);
    }

    void Update()
    {
        // ������ ���¸� ������Ʈ�մϴ�.
        if (GoldManager.Instance.totalGold != prevGold)
        {
            prevGold = GoldManager.Instance.totalGold;
            UpdateItemStatus();
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        // ������ �ε�� ������ �������� �ʱ�ȭ�մϴ�.
        doubleJumpItem = GameObject.Find("Item_a")?.GetComponent<CustomClickable>();
        shieldItem = GameObject.Find("Item_b")?.GetComponent<CustomClickable>();
        magicSizeItem = GameObject.Find("Item_c")?.GetComponent<CustomClickable>();
        prevGold = GoldManager.Instance.totalGold;

        if (doubleJumpItem != null && shieldItem != null && magicSizeItem != null)
        {
            UpdateItemStatus();

            doubleJumpItem.onClick = () =>
            {
                Debug.Log("���� ���� ������ ����");
                PlaySound();
                GoldManager.Instance.totalGold -= doubleJumpPrice;
                doubleJumpLevel++;
                UpdateItemStatus();
                doubleJumpItem.isInteractable = doubleJumpLevel < 1;
            };
            shieldItem.onClick = () =>
            {
                Debug.Log("���� ������ ����");
                PlaySound();
                GoldManager.Instance.totalGold -= shieldPrice;
                shieldLevel++;
                UpdateItemStatus();
            };
            magicSizeItem.onClick = () =>
            {
                Debug.Log("���� ũ�� ������ ����");
                PlaySound();
                GoldManager.Instance.totalGold -= magicSizePrice;
                magicSizeLevel++;
                UpdateItemStatus();
                magicSizeItem.isInteractable = magicSizeLevel < 2;
            };
        }
    }

    void UpdateItemStatus()
    {
        if (doubleJumpItem != null && shieldItem != null && magicSizeItem != null)
        {
            // ������ ������Ʈ��
            if (magicSizeLevel == 1)
                magicSizePrice = 80;

            // ��ư Ȱ��ȭ ��Ʈ��
            doubleJumpItem.isInteractable = doubleJumpLevel < 1 && GoldManager.Instance.totalGold >= doubleJumpPrice;
            shieldItem.isInteractable = GoldManager.Instance.totalGold >= shieldPrice;
            magicSizeItem.isInteractable = magicSizeLevel < 2 && GoldManager.Instance.totalGold >= magicSizePrice;

            // ����ǥ�� ��Ʈ��
            doubleJumpItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + doubleJumpLevel);
            shieldItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + shieldLevel);
            magicSizeItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + magicSizeLevel);

            // ����ǥ�� ��Ʈ��
            Color activeColor = Color.white;
            Color deactiveColor = Color.white;
            ColorUtility.TryParseHtmlString("#FFBF00", out activeColor);
            ColorUtility.TryParseHtmlString("#806000", out deactiveColor);

            doubleJumpItem.transform.GetChild(1).GetComponent<TextMeshPro>().text = (doubleJumpPrice + "G");
            doubleJumpItem.transform.GetChild(1).GetComponent<TextMeshPro>().color = doubleJumpItem.isInteractable ? activeColor : deactiveColor;
            shieldItem.transform.GetChild(1).GetComponent<TextMeshPro>().text = (shieldPrice + "G");
            shieldItem.transform.GetChild(1).GetComponent<TextMeshPro>().color = shieldItem.isInteractable ? activeColor : deactiveColor;
            magicSizeItem.transform.GetChild(1).GetComponent<TextMeshPro>().text = (magicSizePrice + "G");
            magicSizeItem.transform.GetChild(1).GetComponent<TextMeshPro>().color = magicSizeItem.isInteractable ? activeColor : deactiveColor;
        }
    }

    void PlaySound()
    {
        SoundManager.instance?.PlayNewBackSound("Gold_Register");
    }

    bool UseShield()
    {
        if (shieldLevel > 0)
        {
            shieldLevel--;
            return true;
        }
        return false;
    }

    int GetShieldLevel() { return shieldLevel; }

    int GetDoubleJumpLevel()
    {
        return doubleJumpLevel;
    }

    int GetMagicSizeLevel()
    {
        return magicSizeLevel;
    }

    public void PauseMarket(bool _value)
    {
        if (doubleJumpItem != null && shieldItem != null && magicSizeItem != null)
        {
            doubleJumpItem.isInteractable = _value;
            shieldItem.isInteractable = _value;
            magicSizeItem.isInteractable = _value;
        }
    }
}
