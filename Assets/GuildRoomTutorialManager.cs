using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuildRoomTutorialManager : MonoBehaviour
{
    public GameObject GuildRoomTutorialPanel;

    public GameObject imageA;
    public GameObject imageB;
    public GameObject imageC;

    private int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!GuildRoomManager.Instance.isTutorialChecked)
        {
            GuildRoomTutorialPanel.SetActive(true);
            ShowCurrentImage();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!GuildRoomManager.Instance.isTutorialChecked && GuildRoomTutorialPanel.activeSelf && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            currentIndex++;
            ShowCurrentImage();
        }
    }

    private void ShowCurrentImage()
    {
        imageA.SetActive(currentIndex == 0);
        imageB.SetActive(currentIndex == 1);
        imageC.SetActive(currentIndex == 2);

        if (currentIndex >= 3)
        {
            GuildRoomTutorialPanel.SetActive(false);
            GuildRoomManager.Instance.isTutorialChecked = true;
        }
    }
}



