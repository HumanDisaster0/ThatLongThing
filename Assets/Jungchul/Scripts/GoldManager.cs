using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoldManager : MonoBehaviour
{
    //�������� 3��
    //60 20 40->80
    public static GoldManager Instance { get; private set; }

    public int totalGold = 0;

    public int rewardGold = 0;

    public int findTrapCount = 0;
    public int deadCount = 0;
    public int ejectionCount = 0;
    public int rdc = 0;

    public int Tax = 60;

    public bool isChanged = false;

    public GameObject GoldText;


    private void Awake()
    {
        // �̱��� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // �ߺ� ����
            return;
        }
        Tax = 60;
        Instance = this;
        DontDestroyOnLoad(gameObject); // �� ��ȯ �� �ı� ����
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Title")
        {
            ResetAllExceptTax();
        }
    }

    private void Update()
    {
        if(isChanged)
        {
            GoldText = GameObject.Find("TextDrawer");
            if (GoldText != null)
            {
                var gt = GoldText.GetComponent<TextDrawer>();
                gt.TextRefresh();
            }
            isChanged = false;
        }

    }



    public void calRewardGold()
    {
        rdc = deadCount > 5 ? 5 : deadCount;
        rewardGold =  findTrapCount * 5 - rdc * 2 - ejectionCount * 10;
    }

    public void getRewardGold()
    {
        totalGold += rewardGold;      

        //rewardGold = 0;
    }

    public void MinusGold(int amount)
    {
        totalGold -= amount;
    }

    public void PlusGold(int amount)
    {
        totalGold += amount;
    }

    public void SetReward(int ftc, int dc, int ec)
    {
        GoldManager.Instance.findTrapCount = ftc;
        GoldManager.Instance.deadCount = dc;
        GoldManager.Instance.rdc = dc > 5 ? 5 : dc;
        GoldManager.Instance.ejectionCount = ec;
        //GoldManager.Instance.rewardGold = GoldManager.Instance.calRewardGold();
        //GoldManager.Instance.Tax = -60;
        ////calRewardGold �ӽð� : findTrapCount * 100 + deadCount * 20 + ejectionCount * 200;
    }

    public void ClearReward()
    {
        GoldManager.Instance.findTrapCount = 0;
        GoldManager.Instance.deadCount = 0;
        GoldManager.Instance.ejectionCount = 0;
        GoldManager.Instance.rdc = 0;
        GoldManager.Instance.rewardGold = 0;
    }

    private void ResetAllExceptTax()
    {
        totalGold = 0;
        rewardGold = 0;
        findTrapCount = 0;
        deadCount = 0;
        rdc = 0;
        ejectionCount = 0;
        // Tax�� ����
    }

}
