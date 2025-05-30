using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }

    [Header("아이템 가격")]
    public int doubleJumpPrice = 60;
    public int shieldPrice = 20;
    public int magicSizePrice = 40;

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

        // 레벨이 로드될 때마다 아이템을 초기화합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // 아이템 상태를 업데이트합니다.
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
            magicSizeLevel = 0;

            doubleJumpPrice = 60;
            shieldPrice = 20;
            magicSizePrice = 40;
        }
        // 레벨이 로드될 때마다 아이템을 초기화합니다.
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
                //Debug.Log("더블 점프 아이템 구매");
                PlaySound();
                GoldManager.Instance.totalGold -= doubleJumpPrice;
                doubleJumpLevel++;
                UpdateItemStatus();
                doubleJumpItem.isInteractable = doubleJumpLevel < 1;
                GoldManager.Instance.isChanged = true;
            };
            shieldItem.onClick = () =>
            {
                //Debug.Log("방패 아이템 구매");
                PlaySound();
                GoldManager.Instance.totalGold -= shieldPrice;
                shieldLevel++;
                UpdateItemStatus();
                GoldManager.Instance.isChanged = true;
            };
            magicSizeItem.onClick = () =>
            {
                //Debug.Log("마법 크기 아이템 구매");
                PlaySound();
                GoldManager.Instance.totalGold -= magicSizePrice;
                magicSizeLevel++;
                UpdateItemStatus();
                magicSizeItem.isInteractable = magicSizeLevel < 2;
                GoldManager.Instance.isChanged = true;
            };
        }
    }

    public void UpdateItemStatus()
    {
        //Debug.Log("UpdateItemStatus");

        if (doubleJumpItem != null && shieldItem != null && magicSizeItem != null)
        {
            // 아이템 가격컨트롤
            if (magicSizeLevel == 1)
                magicSizePrice = 80;

            // 버튼 활성화 컨트롤
            doubleJumpItem.isInteractable = doubleJumpLevel < 1 && GoldManager.Instance.totalGold >= doubleJumpPrice;
            //shieldItem.isInteractable = GoldManager.Instance.totalGold >= shieldPrice;
            shieldItem.isInteractable = false;
            magicSizeItem.isInteractable = magicSizeLevel < 2 && GoldManager.Instance.totalGold >= magicSizePrice;

            // 레벨표시 컨트롤
            doubleJumpItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + doubleJumpLevel);
            shieldItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + shieldLevel);
            magicSizeItem.transform.GetChild(0).GetComponent<TextMeshPro>().text = ("LV " + magicSizeLevel);

            // 가격표시 컨트롤
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

            // 솔드아웃 컨트롤
            doubleJumpItem.transform.GetChild(2).gameObject.SetActive(doubleJumpLevel >= 1 ? true : false);
            shieldItem.transform.GetChild(2).gameObject.SetActive(shieldLevel >= 0 ? true : false);
            magicSizeItem.transform.GetChild(2).gameObject.SetActive(magicSizeLevel >= 2 ? true : false);
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
