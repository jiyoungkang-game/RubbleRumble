using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask pickupLayerMask;

    public int InteractUIState;

    private void Awake()
    {
        interactRange = 3;
    }

    private void Update()
    {
        ReturnCurrentInteract();  // raycast Ȱ���� ������ Ž��
    }

    private int ReturnCurrentInteract()
    {
        if (ToolManager.Instance.currentTool != 0)  // �Ǽ��� �ƴ� ���
        {
            InteractUIState = 0;    // ��ȣ�ۿ� ��Ȱ��ȭ
            return InteractUIState;
        }

        Ray ray = new Ray(transform.position + Vector3.up, transform.forward * interactRange);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, pickupLayerMask))  // �� �� �ִ� ������Ʈ�� Ž���Ǹ�
        {
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green);
            InteractUIState = 1;    // ��ȣ�ۿ� E Ȱ��ȭ
            return InteractUIState;
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);
            InteractUIState = 0;    // ��ȣ�ۿ� ��Ȱ��ȭ
            return InteractUIState;
        }
    }
}
