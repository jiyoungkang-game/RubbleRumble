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
        ReturnCurrentInteract();  // raycast 활용한 쓰레기 탐지
    }

    private int ReturnCurrentInteract()
    {
        if (ToolManager.Instance.currentTool != 0)  // 맨손이 아닌 경우
        {
            InteractUIState = 0;    // 상호작용 비활성화
            return InteractUIState;
        }

        Ray ray = new Ray(transform.position + Vector3.up, transform.forward * interactRange);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, pickupLayerMask))  // 들 수 있는 오브젝트가 탐지되면
        {
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green);
            InteractUIState = 1;    // 상호작용 E 활성화
            return InteractUIState;
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);
            InteractUIState = 0;    // 상호작용 비활성화
            return InteractUIState;
        }
    }
}
