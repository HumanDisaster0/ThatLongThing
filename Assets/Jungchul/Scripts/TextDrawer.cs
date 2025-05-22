using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextDrawer : MonoBehaviour
{
    public TextMeshPro GoldText;
    public TextMeshPro DayText;
    public TextMeshPro TaxText;


    // Start is called before the first frame update

    private void Awake()
    {
        GoldText = transform.GetChild(0).GetComponent<TextMeshPro>();
        DayText = transform.GetChild(1).GetComponent<TextMeshPro>();
        TaxText = transform.GetChild(2).GetComponent<TextMeshPro>();
    }
    void Start()
    {
        string temp = GuildRoomManager.Instance.day.ToString();
        GoldText.text = GoldManager.Instance.totalGold.ToString();
        DayText.text = $"D-{4-GuildRoomManager.Instance.day}";
        TaxText.text = GoldManager.Instance.Tax.ToString();        
    }

    public void TextRefresh()
    {
        string temp = GuildRoomManager.Instance.day.ToString();
        GoldText.text = GoldManager.Instance.totalGold.ToString();
        //DayText.text = GuildRoomManager.Instance.day.ToString();
        DayText.text = $"D-{4 - GuildRoomManager.Instance.day}";
        TaxText.text = GoldManager.Instance.Tax.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
