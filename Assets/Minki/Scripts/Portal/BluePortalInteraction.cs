using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluePortalInteraction : MonoBehaviour
{
    GameObject m_playerPin;
    MapMatchCheckPinCreator m_pinCreator;
    MapOnOffControl m_mapOnOffControl;
    MapPinMatchChecker m_pinMatchChecker;

    public void Start()
    {
        m_playerPin = GameObject.FindFirstObjectByType<MinimapPlayerPos>(FindObjectsInactive.Include).gameObject;
        m_pinCreator = GameObject.FindFirstObjectByType<MapMatchCheckPinCreator>(FindObjectsInactive.Include);
        m_mapOnOffControl = GameObject.FindFirstObjectByType<MapOnOffControl>(FindObjectsInactive.Include);
        m_pinMatchChecker = GameObject.FindFirstObjectByType<MapPinMatchChecker>(FindObjectsInactive.Include);
    }

    public void EnterPortal()
    {
        //�̴ϸ� �ѱ�
        m_mapOnOffControl.ShowMinimap();
        m_pinCreator.UpdateSibling();

        //����� ��ư ����
        GameObject.Find("�����").SetActive(false);

        //�÷��̾� �� ������
        m_playerPin.gameObject.SetActive(false);

        //��ġüũ �� ����
        m_pinCreator.CreatePins(m_pinMatchChecker.CreateMatchData());

        //�̴ϸ� �� ��ȣ�ۿ� ����(��Ŭ�� ����!)
        MapPinSetter.IsPinSetterActive = false;

        //�̴ϸ� �� �� ���� �ڷ�ƾ ���� ���
        m_mapOnOffControl.OnMiniMapHide += StartNextEvent;
    }

    private void OnDestroy()
    {
        if(m_mapOnOffControl != null)
        {
            m_mapOnOffControl.OnMiniMapHide -= StartNextEvent;
        }
    }

    public void StartNextEvent()
    {
        StartCoroutine(GoGuildRoom());
    }

    IEnumerator GoGuildRoom()
    {
        //�̴ϸ� Ŭ�� ����
        m_mapOnOffControl.activeControl = false;

        //�̴ϸ� �� ��ȣ�ۿ� Ǯ��
        MapPinSetter.IsPinSetterActive = true;

        yield return null;

        var timer = 0.0f;

        while(timer < 1.25f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        StageManager.instance.EndStage();
    }
}
