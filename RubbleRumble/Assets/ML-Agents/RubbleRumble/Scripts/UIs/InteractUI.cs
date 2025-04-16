using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractUI : MonoBehaviour
{
    private Transform camTransform;
    private Transform playerTransform;
    private float fixedYPosition;
    private Quaternion fixedRotation;

    [SerializeField] private GameObject interactTxtObj;
    [SerializeField] private GameObject holdingBarObj;
    [SerializeField] private TextMeshProUGUI interactTxt;
    [SerializeField] private Image holdingBarImg;

    [SerializeField] private PlayerController playerController ;
    [SerializeField] private PlayerInteract playerInteract ;
    [SerializeField] private Mop mop;
    private float unfoldDuration; // �ڽ��� ��ġ�� �� �ʿ��� �ð�

    private void Awake()
    {
        SetUITransform();

        holdingBarImg.fillAmount = 0;
        interactTxtObj.SetActive(false);
        holdingBarObj.SetActive(false);
    }

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        playerInteract = player.GetComponent<PlayerInteract>();
        playerController = player.GetComponent<PlayerController>();
        unfoldDuration = playerController.GetUnfoldDuration();
    }

    private void Update()
    {
        // �÷��̾� �̵��� ���� UI �̵�
        float playerX = playerTransform.position.x;
        float playerZ = playerTransform.position.z;
        transform.position = new Vector3(playerX, fixedYPosition, playerZ);

        transform.rotation = fixedRotation; // ȸ���� ����

        UpdateInteractUI(playerInteract.InteractUIState);
    }

    private void SetUITransform()
    {
        camTransform = Camera.main.transform;
        playerTransform = GameObject.FindWithTag("Player").transform;

        fixedYPosition = transform.position.y;  // ������ y�� ��

        // ȸ���� ����
        fixedRotation = camTransform.rotation;
        transform.rotation = fixedRotation;
    }

    private void UpdateInteractUI(int interactUIState)
    {
        switch (interactUIState)
        {
            case 3: // Q�� Ȧ���ϰ� �ִ� ���
                {
                    interactTxtObj.SetActive(false);    // ��ȣ�ۿ� �ؽ�Ʈ ��Ȱ��ȭ
                    holdingBarObj.SetActive(true);      // Ȧ���� Ȱ��ȭ
                    if (ToolManager.Instance.currentTool == 0)  // �Ǽ����� �۾��ϰ� ������
                        holdingBarImg.fillAmount = playerController.GetHoldingTime() / unfoldDuration;  // Ȧ���� ����
                    else
                    {
                        mop = FindObjectOfType<Mop>();
                        holdingBarImg.fillAmount = mop.GetHoldingTime() / unfoldDuration;  // Ȧ���� ����
                    }
                    break;
                }
            case 2: // Q�� ������ �ϴ� ���
                {
                    interactTxtObj.SetActive(true);     // ��ȣ�ۿ� �ؽ�Ʈ Ȱ��ȭ
                    holdingBarObj.SetActive(false);     // Ȧ���� ��Ȱ��ȭ
                    interactTxt.text = "[Q]";           // QŰ �������� �ȳ�
                    break;
                }
            case 1: // E�� ������ �ϴ� ���
                {
                    interactTxtObj.SetActive(true);     // ��ȣ�ۿ� �ؽ�Ʈ Ȱ��ȭ
                    holdingBarObj.SetActive(false);     // Ȧ���� ��Ȱ��ȭ
                    interactTxt.text = "[E]";           // EŰ �������� �ȳ�
                    break;
                }
            case 0:
                {
                    interactTxtObj.SetActive(false);     // ��ȣ�ۿ� �ؽ�Ʈ ��Ȱ��ȭ
                    holdingBarObj.SetActive(false);     // Ȧ���� ��Ȱ��ȭ
                    break;
                }
            default:
                {
                    Debug.Log("InteractUI Error");
                    break;
                }
        }
    }
}
