
using UnityEngine;

using UnityEngine.UI;


public class GuildRoomObject : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public GameObject EnterPopup;

    public Material normalMaterial;
    public Material outlineMaterial;
    public float detectionDistance = 0.7f;

    private GameObject activeButton;

    public bool isHighlighted = false;

    public bool isInteractable = true;

    [SerializeField] AvatarController avatarController;

    [SerializeField] public string code;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        detectionDistance = 0.5f;
        //isHighlighted = false;
        //EnterPopup.SetActive(false);
    }

    public void Update()
    {
        float distance = 0f;
        if (avatarController != null)
        {
            distance = Mathf.Abs(avatarController.transform.position.x - transform.position.x);
        }

        if (distance < detectionDistance)
        {
            if (isInteractable)
            {
                spriteRenderer.material = outlineMaterial;
                isHighlighted = true;
                print(gameObject.name + " is highlited!!!!");

                if (activeButton == null && EnterPopup != null)
                {
                    activeButton = Instantiate(EnterPopup, transform.position, Quaternion.identity);
                    activeButton.transform.position = transform.position + new Vector3(0, 1.0f, 0);
                }
            }
        }
        else
        {
            if (isHighlighted)
            {
                spriteRenderer.material = normalMaterial;
                print(gameObject.name + " ����");
                isHighlighted = false;

                if (activeButton != null)
                {
                    Destroy(activeButton);
                    activeButton = null;
                }
            }
        }

        if (isHighlighted)
        {

            if (Input.GetKeyDown(KeyCode.J))
            {
                string objName = gameObject.name;

                // �̸��� ������� ���� ����
                if (objName.Contains("Settlement"))
                {
                    GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.SETTLEMENT);
                }
                else if (objName.Contains("Mission"))
                {
                    GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.MISSIONBOARD);
                }
                else if (objName.Contains("Pokedex"))
                {
                    GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.POKEDEX);
                }
                else if (objName.Contains("DoorOut"))
                {
                    GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.DOOROUT);
                }
            }
        }

    }

    public void UpdateState(Vector3 avatarPosition)
    {
        //if (!gameObject.scene.IsValid()) return;

        //float distance = Mathf.Abs(avatarPosition.x - transform.position.x);

        //if (distance < detectionDistance)
        //{
        //    if (!isHighlighted && isInteractable)
        //    {
        //        spriteRenderer.material = outlineMaterial;
        //        Debug.Log($"���� ��Ƽ����: {spriteRenderer.material.name}");
        //        isHighlighted = true;
        //        print(gameObject.name + " is highlited!!!!");

        //        if (activeButton == null && EnterPopup != null)
        //        {
        //            activeButton = Instantiate(EnterPopup, transform.position, Quaternion.identity);
        //            activeButton.transform.position = transform.position + new Vector3(0, 1.0f, 0);
        //        }
        //    }
        //}
        //else
        //{
        //    if (isHighlighted)
        //    {
        //        spriteRenderer.material = normalMaterial;
        //        print(gameObject.name + " ����");
        //        isHighlighted = false;

        //        if (activeButton != null)
        //        {
        //            Destroy(activeButton);
        //            activeButton = null;
        //        }
        //    }
        //}

        //if (isHighlighted)
        //{
        //    print(gameObject.name + "  ���̶���~");

        //    if (Input.GetKeyDown(KeyCode.J))
        //    {
        //        string objName = gameObject.name;

        //        // �̸��� ������� ���� ����
        //        if (objName.Contains("Settlement"))
        //        {
        //            GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.SETTLEMENT);
        //        }
        //        else if (objName.Contains("Mission"))
        //        {
        //            GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.MISSIONBOARD);
        //        }
        //        else if (objName.Contains("Pokedex"))
        //        {
        //            GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.POKEDEX);
        //        }
        //        else if (objName.Contains("DoorOut"))
        //        {
        //            GuildRoomManager.Instance.SetRoomState(GuildRoomManager.viewState.DOOROUT);
        //        }
        //    }
        //}

    }

}