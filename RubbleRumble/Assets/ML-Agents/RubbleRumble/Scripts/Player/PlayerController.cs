using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private ToolManager toolManager;
    private WorkBench workBench;
    private PlayerHand playerHand;
    private PlayerInteract playerInteract;

    private Transform rightHandTransform; // �����⳪ ������ ���� �÷��̾��� ������ ��ġ (Transform)

    public GameObject unfoldedBoxPrefab; // ������ �ڽ� ������ (Unity Inspector���� ���� �ʿ�)
    private GameObject heldObject;
    // private GameObject workbench; // CS0414: ������ ���� - ���ŵ�
    //private GameObject boxOnWorkbench; // �۾��� ���� �ö� �ִ� �Ϲ� �ڽ�
    //private GameObject unfoldedBoxOnWorkbench; // �۾��� ���� �ִ� ������ �ڽ�
    private GameObject trashOnWorkbench;

    private bool isHoldingTrash = false; // �÷��̾ �����⸦ ��� �ִ��� ���θ� ����
    // private bool isBoxUnfolded = false; // CS0414: ������ ���� - ���ŵ�
    private bool isNearWorkbench = false; // �÷��̾ �۾��� ��ó�� �ִ��� ����
    private bool isNearRecyclingBin = false; // �÷��̾ �и������� ��ó�� �ִ��� ����
    private bool isUnfolding = false; // �ڽ��� �������� ������ ���θ� ����

    private const float UNFOLD_DURATION = 2f; // �ڽ��� ��ġ�� �� �ʿ��� �ð� (2�ʷ� ����)
    private float qKeyHoldTime = 0f; // Q Ű�� ������ �ִ� �ð��� ���� (�� ����)

    private Coroutine recycleCoroutine;

    private int interactUIState;

    void Awake()
    {
        // �÷��̾��� Animator���� ������ ��(Bone)�� Transform�� ������
        rightHandTransform = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        // ������ ��ġ�� �չٴ� �������� �ణ ���� (0.15 ���� �̵�)
        if (rightHandTransform != null) // Null üũ �߰�
        {
            rightHandTransform.position = rightHandTransform.position + rightHandTransform.forward * 0.15f;
        } else {
             Debug.LogError("RightHand Transform�� ã�� �� �����ϴ�. Animator�� HumanBodyBones ������ Ȯ���ϼ���.");
        }


        toolManager = GameObject.Find("Managers").GetComponent<ToolManager>();
        //workBench = GameObject.Find("Workbench").GetComponent<WorkBench>();
        workBench = FindFirstObjectByType<WorkBench>();
        playerHand = GameObject.Find("Player").GetComponent<PlayerHand>();
        playerInteract = GameObject.Find("Player").GetComponent<PlayerInteract>();

        if (workBench == null)
        {
            Debug.LogError("WorkBench ������Ʈ�� ã�� �� �����ϴ�.");
        }
        if (playerHand == null)
        {
            Debug.LogError("PlayerHand ������Ʈ�� ã�� �� �����ϴ�.");
        }
         if (playerInteract == null)
        {
            Debug.LogError("PlayerInteract ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    private void Update()
    {
        // ���� ���� : 1=�Ǽ�, 2=���ڷ�, 3=��ɷ�
        if (Input.GetKeyDown(KeyCode.Alpha1)) toolManager.EquipTool(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) toolManager.EquipTool(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) toolManager.EquipTool(2);

        if (workBench != null) // Null üũ �߰�
        {
             isUnfolding = workBench.IsRecycling;    // ��Ȱ�� ������ ���� ����
        }


        if (isNearWorkbench && !isUnfolding && workBench != null) // workBench Null üũ �߰�
        {
            // ������: ���� ���� ��� (������ ��� �ִ���, �۾��� �� �ڽ� ���� ��)
            // Debug.Log("�۾��� ��ó - isHoldingTrash: " + isHoldingTrash + ", heldObject: " + (heldObject != null ? heldObject.name : "null") + ", trashOnWorkbench: " + (trashOnWorkbench != null ? trashOnWorkbench.name : "null"));

            // �۾��� ��ó���� ������ �ڽ��� E Ű�� �ݱ�
            if (trashOnWorkbench != null && Input.GetKeyDown(KeyCode.E) && !isHoldingTrash)
            {
                Debug.Log("E Ű ���� - ������ �ڽ� �ݱ�");
                //playerHand.PickUpTrash(heldObject); // ������ �ڽ��� �տ� ��� �Լ� ȣ��
                playerHand.PickUpTrash(trashOnWorkbench); // ������ �ڽ��� �տ� ��� �Լ� ȣ��
                heldObject = trashOnWorkbench; // trashOnWorkbench�� gameObject�� �ƴ϶� trashOnWorkbench ��ü�� �Ҵ��ؾ� �� �� �ֽ��ϴ�. PlayerHand ���� Ȯ�� �ʿ�
                trashOnWorkbench = null; // �۾��뿡�� ����
                isHoldingTrash = true;
            }

            // Q Ű�� ó�� ������ ��: �ڽ��� �۾��뿡 �ø��� ����
            if (Input.GetKeyDown(KeyCode.Q) && isHoldingTrash && heldObject != null && heldObject.CompareTag("Box") && trashOnWorkbench == null)
            {
                Debug.Log("Q Ű ���� - �ڽ��� �۾��뿡 �ø�");
                playerHand.PlaceTrashOnWorkbench(workBench, heldObject); // �ڽ��� �۾��� ���� �ø��� �Լ� ȣ��
                trashOnWorkbench = heldObject; // �۾��� �� ������Ʈ�� ����
                heldObject = null; // �տ��� ����
                isHoldingTrash = false;
            }

            // Q Ű�� ��� ������ ���� ��: �۾��� �� �ڽ��� ��ġ�� ���� �غ�
            if (Input.GetKey(KeyCode.Q) && trashOnWorkbench != null && trashOnWorkbench.CompareTag("Box")) // Box �±��϶��� ��ġ�� �õ�
            {
                qKeyHoldTime += Time.deltaTime; // Q Ű ���� �ð� ����
                // Debug.Log("Q Ű ������ �� - qKeyHoldTime: " + qKeyHoldTime);
                if (qKeyHoldTime >= UNFOLD_DURATION) // 2�� �̻� ������ �ڽ� ��ħ ����
                {
                    Debug.Log("Q Ű 2�� �̻� - �ڽ� ��ħ ����");
                    if (recycleCoroutine == null) // �ڷ�ƾ �ߺ� ���� ����
                    {
                        recycleCoroutine = StartCoroutine(workBench.RecycleAction(trashOnWorkbench)); // �ڽ� ��ġ�� �ڷ�ƾ ����
                        trashOnWorkbench = null; // �۾��뿡�� ���� �ڽ� ���� ����
                        qKeyHoldTime = 0f; // Ȧ�� �ð� �ʱ�ȭ (�ڷ�ƾ ���� ��)
                    }
                }
            }

            // Q Ű�� ���� ��: ���� �ð� �ʱ�ȭ (�ڷ�ƾ�� ���۵��� �ʾ��� ���)
            if (Input.GetKeyUp(KeyCode.Q))
            {
                //Debug.Log("Q Ű �� - qKeyHoldTime �ʱ�ȭ");
                qKeyHoldTime = 0f;
                // �ڷ�ƾ�� ���������� ���� ��(2�� ���� ���� ��)�� Ư���� ������ �ʿ� ����
                // if (recycleCoroutine != null && !isUnfolding) // �ڷ�ƾ�� ����Ǿ����� ���� IsRecycling�� true�� �Ǳ� ��? -> �� ���Ǻ��ٴ� �Ʒ��� ����
                // {
                //     StopCoroutine(recycleCoroutine);
                //     recycleCoroutine = null;
                // }
            }
        } else if (!isNearWorkbench && recycleCoroutine != null) // �۾��� ����� �ڷ�ƾ ����
        {
             StopCoroutine(recycleCoroutine);
             recycleCoroutine = null;
             isUnfolding = false; // Ȯ���� �ϱ� ���� ���� ����
             Debug.Log("�۾��� ����� ��ġ�� �ڷ�ƾ ����");
        }


        // �и������� ��ó���� E Ű�� ������ ����
        if (isNearRecyclingBin && isHoldingTrash && heldObject != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E Ű ���� - �и������� ������ ����");
            ReturnTrashToPool(); // �����⸦ �и����������� ������ �Լ� ȣ��
        }

        ReturnCurrentInteract();    // ��ȣ�ۿ� UI ���� ����
    }

    // Ʈ���� ���� �ȿ� �ӹ����� ���� ȣ��Ǵ� �Լ�
    void OnTriggerStay(Collider other)
    {
        // �����⸦ ��� ���� �ʰ�, �Ǽ�(�ε��� 0)�� ���� ������Ʈ �ݱ� ����
        if (!isHoldingTrash && toolManager.currentTool == 0 && (other.CompareTag("Can") || other.CompareTag("Box") || other.CompareTag("UnfoldedBox")))
        {
            // PlayerHand���� �̹� �ֿ� �� �ִ� ������Ʈ���� Ȯ���ϴ� ������ ���� �� �����Ƿ�, ���⼭�� ������ �ϰ� EŰ �Է��� Update���� ó���ϴ� ���� �� �Ϲ����� �� ����
            // ���� ���⼭ �ٷ� �ݴ� ������ �����Ѵٸ� heldObject �Ҵ� �ʿ�
             if (Input.GetKey(KeyCode.E)) // E Ű�� ������ �ݱ� ����
            {
                 heldObject = other.gameObject; // EŰ ������ �������� �Ҵ�
                 Debug.Log("E Ű ���� - ������Ʈ �ݱ�: " + heldObject.name);
                 playerHand.PickUpTrash(heldObject); // ������Ʈ�� �տ� ��� �Լ� ȣ��
                 isHoldingTrash = true;
                 // �ֿ� �Ŀ��� Ʈ���� �� �ٸ� ������Ʈ�� ��ȣ�ۿ� ���� ���� heldObject�� null��? -> PlayerHand���� �����ϴ� ���� ���� �� ����
                 // heldObject = null; // �ֿ� �� �ʱ�ȭ (������)
            } else {
                 // EŰ�� ������ ���� ���¿����� ������ ��� ǥ�� (UI �뵵 ��)
                 // playerInteract.ShowInteractHint(other.gameObject); // ����
            }
        }

        // �۾��� �ֺ� ������ ������ ��
        if (other.CompareTag("WorkbenchArea"))
        {
            if (!isNearWorkbench) // ó�� ������ ���� �α� ��� �� ���� Ȯ��
            {
                isNearWorkbench = true; // �۾��� ��ó �÷��� Ȱ��ȭ
                Debug.Log("�۾��� ��ó ����");
                 if (workBench != null) { // null üũ
                    trashOnWorkbench = workBench.CheckOnWorkbench(); // �۾��� �� ������Ʈ Ȯ��
                 } else {
                    Debug.LogError("WorkBench ������ null�Դϴ�.");
                 }
            } else {
                 // ��� �ӹ����� ���� �۾��� �� ���� ���� (�ʿ� ��)
                 // trashOnWorkbench = workBench?.CheckOnWorkbench(); // Optional chaining
            }
        }

        // �и������� ��ó�� ������ ��
        if (other.CompareTag("RecyclingBin"))
        {
            if (!isNearRecyclingBin) // ó�� ������ ���� �α� ���
            {
                 isNearRecyclingBin = true; // �и������� ��ó �÷��� Ȱ��ȭ
                 Debug.Log("�и������� ��ó ����");
            }
        }
    }

    // Ʈ���� ������ ��� �� ȣ��Ǵ� �Լ�
    void OnTriggerExit(Collider other)
    {
        // ���� ��� ������Ʈ�� Ʈ���Ÿ� ����� ���� �ʱ�ȭ (���� ���� ���)
        // if (other.gameObject == heldObject) // �� ������ �̹� �ֿ� ��쿡�� �ش�� �� �־� ������
        // {
        //     // heldObject = null; // ���⼭ null ó���ϸ� ��� �ִ� ���µ� ������ �� ����
        //     // Debug.Log("Ʈ���� ��� - heldObject �ʱ�ȭ?");
        // }

        // �۾��� �ֺ� ������ ����� ���� ���� �ʱ�ȭ
        if (other.CompareTag("WorkbenchArea"))
        {
            isNearWorkbench = false;
            // workbench = null; // CS0414: ���ŵ�
            trashOnWorkbench = null; // �۾��� ����� ���� �ʱ�ȭ
            qKeyHoldTime = 0f;
            Debug.Log("�۾��� ���");
            if (recycleCoroutine != null) // �۾��� ��� �� �ڷ�ƾ ���� ���̸� ����
            {
                 Debug.Log("�۾��� ����� ��ġ�� �ڷ�ƾ ���� (OnTriggerExit)");
                 StopCoroutine(recycleCoroutine);
                 recycleCoroutine = null;
                 isUnfolding = false; // ���� Ȯ���� ����
            }
        }

        // �и������� ������ ����� �÷��� �ʱ�ȭ
        if (other.CompareTag("RecyclingBin"))
        {
            isNearRecyclingBin = false;
            Debug.Log("�и������� ���");
        }
    }

    // �и������忡�� �����⸦ �����ϴ� �Լ�
    private void ReturnTrashToPool()
    {
        if (isHoldingTrash && heldObject != null) // �����⸦ ��� ���� ��
        {
            Obstacle obstacle = heldObject.GetComponent<Obstacle>();
            bool removed = false;

            if (heldObject.CompareTag("UnfoldedBox") || heldObject.CompareTag("Can")) // ������ �ڽ� �Ǵ� ĵ�̸�
            {
                if (obstacle != null)
                {
                    obstacle.CleanObstacle(); // Obstacle�� ���� ���� ���
                    Debug.Log(heldObject.tag + " �и����� �Ϸ�");
                    removed = true;
                }
                else
                {
                    Debug.LogWarning(heldObject.name + "�� Obstacle ������Ʈ�� �����ϴ�. Destroy�� ��ü�մϴ�.");
                    Destroy(heldObject); // ����
                    removed = true;
                }
                // isBoxUnfolded = false; // CS0414: ���ŵ�
            }
            else if (heldObject.CompareTag("Box")) // �Ϲ� �ڽ���
            {
                Debug.Log("�������� ���� �ڽ��� �и����� �Ұ�");
                // �������� �ʰ� ����
            }
            else
            {
                 Debug.LogWarning("�� �� ���� �±��� ������: " + heldObject.tag);
                 // �ʿ��ϴٸ� ���⼭�� ���� ���� �߰�
            }


            if (removed)
            {
                 heldObject = null; // ��� �ִ� ������Ʈ �ʱ�ȭ
                 isHoldingTrash = false; // ������ ��� ���� �÷��� ��Ȱ��ȭ
            }
        }
    }

    // �ȳ��� ��ȣ�ۿ� �̺�Ʈ �����ϴ� �Լ�
    public int ReturnCurrentInteract()
    {
         int newState = 0; // �⺻ ����: ��Ȱ��ȭ

        if (isNearWorkbench)    // �۾��� ��ó�� �ְ�
        {
            if (trashOnWorkbench != null)   // �۾��뿡 �����Ⱑ �ְ�
            {
                 if (trashOnWorkbench.CompareTag("Box")) // ���� �ڽ�
                 {
                     if (Input.GetKey(KeyCode.Q)) // Q ������ ������
                     {
                         newState = 3;    // Ȧ���� Ȱ��ȭ
                     } else {
                         newState = 2;    // ��ȣ�ۿ� Q �ȳ� (����/��ġ�� ����)
                     }
                 }
                 else if (trashOnWorkbench.CompareTag("UnfoldedBox"))// �۾��� ���� ��ģ ���ڰ� ������
                 {
                     newState = 1;    // ��ȣ�ۿ� E �ȳ� (�ݱ�)
                 }
                 // ĵ �� �ٸ� ������Ʈ�� �۾��뿡 ���� ��� ���� 0 ����
            }
            else if (isHoldingTrash && heldObject != null) // �۾���� �����, ��� �ִ� ������ ������
            {
                 if (heldObject.CompareTag("Box"))    // ���� ���ڸ� ��� ������
                 {
                     newState = 2;    // ��ȣ�ۿ� Q �ȳ� (����)
                 }
                  // �ٸ� �� ��� ���� �� �۾��뿡�� �� �� �ִ� ��ȣ�ۿ� ���� (���� 0 ����)
            }
             // �۾��� ��ó�̰� �ƹ��͵� �ȵ�� �ְ� �۾��뵵 ��������� ���� 0 ����
        }
        else if (isNearRecyclingBin && isHoldingTrash && heldObject != null)   // �и������� ��ó�� �ְ� �����⸦ ��� �ִ� ���
        {
            if (heldObject.CompareTag("Box"))    // ���� �ڽ��� ��� ������ �и����� �Ұ�
                newState = 0;    // ��ȣ�ۿ� UI ��Ȱ��ȭ
            else if (heldObject.CompareTag("UnfoldedBox") || heldObject.CompareTag("Can")) // ������ �ڽ��� ĵ
                newState = 1;    // ��ȣ�ۿ� E �ȳ� (������)
        }
        else if (!isHoldingTrash && toolManager.currentTool == 0) // �Ǽ��̰� �ƹ��͵� �� ��� ���� ��
        {
           
             // Collider[] nearbyItems = Physics.OverlapSphere(transform.position, 1.5f, LayerMask.GetMask("Interactable")); // ����
             // if (nearbyItems.Length > 0) {
             //     bool canPickUp = false;
             //     foreach(var itemCollider in nearbyItems) {
             //         if (itemCollider.CompareTag("Can") || itemCollider.CompareTag("Box") || itemCollider.CompareTag("UnfoldedBox")) {
             //              canPickUp = true;
             //              break;
             //         }
             //     }
             //     if(canPickUp) newState = 1; // ��ȣ�ۿ� E �ȳ� (�ݱ�)
             // }
             // ---> �� �κ��� PlayerInteract ��ũ��Ʈ���� ó���ϴ� ���� �� ������ �� ����.
             // ---> PlayerController�� ���� �÷��׸� �����ϰ�, PlayerInteract�� ���� UI ���� ����.
        }


        // ���� ���� ����
        if (playerInteract != null)
        {
            playerInteract.InteractUIState = newState;
        }
        return newState;
    }
    public float GetHoldingTime()
    {
        return qKeyHoldTime;
    }

    public float GetUnfoldDuration()
    {
        return UNFOLD_DURATION;
    }
}