using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DummyStageGetGold : MonoBehaviour
{
    public Panel panel;

    [SerializeField] int gold;

    void Start()
    {
        CustomClickable clickable = GetComponent<CustomClickable>();

        clickable.SetClickAction(() =>
        {
            GoldManager.Instance.SetReward(5, 2, 1);

            if (panel != null)
            {
                panel.gameObject.SetActive(false);
                                               
                GoldManager.Instance.getRewardGold(GoldManager.Instance.rewardGold);

                NonePlaySceneManager.Instance.SetSceneState(NonePlaySceneManager.npSceneState.GUILDMAIN);
                                
                GuildRoomManager.Instance.SetReturned();

                SceneManager.LoadScene("GuildMain");

                Debug.Log($"rewardGold:{GoldManager.Instance.rewardGold}");

            }
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }

   

}
