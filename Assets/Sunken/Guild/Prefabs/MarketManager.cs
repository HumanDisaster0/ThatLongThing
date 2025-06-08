using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }

    [Header("������ ����")]
    public int[] doubleJumpPrice = { 60 };
    public int[] shieldPrice = { 20 };
    public int[] magicSizePrice = { 40 , 80 };

    [SerializeField] private CustomClickable doubleJumpItem;
    [SerializeField] private CustomClickable shieldItem;
    [SerializeField] private CustomClickable magicSizeItem;

    int doubleJumpLevel = 0;
    int shieldLevel = 0;
    int magicSizeLevel = 0;

    int prevGold = 100;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // ������ �ε�� ������ �������� �ʱ�ȭ�մϴ�.
        SceneManager.sceneLoaded += OnSceneLoaded;

        Instance = this;
        DontDestroyOnLoad(gameObject);
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Title")
        {
            doubleJumpLevel = 0;
            shieldLevel = 0;
            magicSizeLevel = 1;
        }
        // ������ �ε�� ������ �������� �ʱ�ȭ�մϴ�.
        //doubleJumpItem = GameObject.Find("Item_a")?.GetComponent<CustomClickable>();
        //shieldItem = GameObject.Find("Item_b")?.GetComponent<CustomClickable>();
        //magicSizeItem = GameObject.Find("Item_c")?.GetComponent<CustomClickable>(); 
        doubleJumpItem = GameObject.Find("MarketLoader")?.GetComponent<MarketLoader>().doubleJumpItem;
        shieldItem = GameObject.Find("MarketLoader")?.GetComponent<MarketLoader>().shieldItem;
        magicSizeItem = GameObject.Find("MarketLoader")?.GetComponent<MarketLoader>().magicSizeItem;

        if (doubleJumpItem != null && shieldItem != null && magicSizeItem != null)
        {
            UpdateItemStatus();

            doubleJumpItem.onClick = () =>
            {
                //Debug.Log("���� ���� ������ ����");
                PlaySound();
                GoldManager.Instance.totalGold -= doubleJumpPrice[doubleJumpLevel];
                doubleJumpLevel++;
                UpdateItemStatus();
                doubleJumpItem.isInteractable = doubleJumpLevel < doubleJumpPrice.Length;
                GoldManager.Instance.isChanged = true;
            };
            shieldItem.onClick = () =>
            {
                //Debug.Log("���� ������ ����");
                PlaySound();
                GoldManager.Instance.totalGold -= shieldPrice[shieldLevel];
                shieldLevel++;
                UpdateItemStatus();
                shieldItem.isInteractable = shieldLevel < shieldPrice.Length;
                GoldManager.Instance.isChanged = true;
            };
            magicSizeItem.onClick = () =>
            {
                //Debug.Log("���� ũ�� ������ ����");
                PlaySound();
                GoldManager.Instance.totalGold -= magicSizePrice[magicSizeLevel];
                magicSizeLevel++;
                UpdateItemStatus();
                magicSizeItem.isInteractable = magicSizeLevel < magicSizePrice.Length;
                GoldManager.Instance.isChanged = true;
            };
        }
    }

    public void UpdateItemStatus()
    {
        //Debug.Log("UpdateItemStatus");

        if (doubleJumpItem != null && shieldItem != null && magicSizeItem != null)
        {
            // ��ư Ȱ��ȭ ��Ʈ��
            doubleJumpItem.isInteractable = doubleJumpLevel < doubleJumpPrice.Length && GoldManager.Instance.totalGold >= doubleJumpPrice[doubleJumpLevel];
            shieldItem.isInteractable = shieldLevel < shieldPrice.Length && GoldManager.Instance.totalGold >= shieldPrice[shieldLevel];
            //shieldItem.isInteractable = false;
            magicSizeItem.isInteractable = magicSizeLevel < magicSizePrice.Length && GoldManager.Instance.totalGold >= magicSizePrice[magicSizeLevel];

            // ����ǥ�� ��Ʈ��
            doubleJumpItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + doubleJumpLevel);
            shieldItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + shieldLevel);
            magicSizeItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + magicSizeLevel);

            // ����ǥ�� ��Ʈ��
            Color activeColor = Color.white;
            Color deactiveColor = Color.white;
            ColorUtility.TryParseHtmlString("#FFBF00", out activeColor);
            ColorUtility.TryParseHtmlString("#806000", out deactiveColor);

            //doubleJumpItem.transform.GetChild(1).GetComponent<TextMeshPro>().text = (doubleJumpPrice[doubleJumpLevel] + "G");
            SetPriceText(doubleJumpItem.transform.GetChild(1).GetComponent<TextMeshPro>(), doubleJumpPrice, doubleJumpLevel);
            doubleJumpItem.transform.GetChild(1).GetComponent<TextMeshPro>().color = doubleJumpItem.isInteractable ? activeColor : deactiveColor;
            //shieldItem.transform.GetChild(1).GetComponent<TextMeshPro>().text = (shieldPrice[shieldLevel] + "G");
            SetPriceText(shieldItem.transform.GetChild(1).GetComponent<TextMeshPro>(), shieldPrice, shieldLevel);
            shieldItem.transform.GetChild(1).GetComponent<TextMeshPro>().color = shieldItem.isInteractable ? activeColor : deactiveColor;
            //magicSizeItem.transform.GetChild(1).GetComponent<TextMeshPro>().text = (magicSizePrice[magicSizeLevel] + "G");
            SetPriceText(magicSizeItem.transform.GetChild(1).GetComponent<TextMeshPro>(), magicSizePrice, magicSizeLevel);
            magicSizeItem.transform.GetChild(1).GetComponent<TextMeshPro>().color = magicSizeItem.isInteractable ? activeColor : deactiveColor;

            // �ֵ�ƿ� ��Ʈ��
            doubleJumpItem.transform.GetChild(2).gameObject.SetActive(doubleJumpLevel >= doubleJumpPrice.Length ? true : false);
            shieldItem.transform.GetChild(2).gameObject.SetActive(shieldLevel >= shieldPrice.Length ? true : false);
            magicSizeItem.transform.GetChild(2).gameObject.SetActive(magicSizeLevel >= magicSizePrice.Length ? true : false);
        }
    }

    void SetPriceText(TextMeshPro tmp, int[] price, int level)
    {
        if(price.Length <= level)
            tmp.text = (price[level - 1] + "G");
        else
            tmp.text = (price[level] + "G");
    }

    void PlaySound()
    {
        //SoundManager.instance?.PlayNewBackSound("c");
    }

    //public bool UseShield()
    //{
    //    if (shieldLevel > 0)
    //    {
    //        shieldLevel--;
    //        return true;
    //    }
    //    return false;
    //}

    public int GetShieldLevel() { return shieldLevel; }

    public int GetDoubleJumpLevel()
    {
        return doubleJumpLevel;
    }

    public int GetMagicSizeLevel()
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }    
}
