using System.Collections;
using UnityEngine;

// ����: �� TrashHandler ��ũ��Ʈ�� PlayerController�� ����� ���� �ߺ��˴ϴ�.
// PlayerController �ϳ��� �����ϰų�, ������ ��Ȯ�� �и��ϴ� �����丵�� ����غ� �� �ֽ��ϴ�.
// ���⼭�� ������ �ذ��ϴ� �������� �����մϴ�.
public class TrashHandler : MonoBehaviour
{
    // private Player player; // Player Ŭ������ �����Ƿ� �ּ� ó�� �Ǵ� ����
    private Transform rightHand; // �����⳪ ������ ���� �÷��̾��� ������ ��ġ (Transform)
    private GameObject heldObject; // �÷��̾ ���� ��� �ִ� ������Ʈ (�����⳪ �ڽ� ��)
    private bool isHoldingTrash = false; // �÷��̾ �����⸦ ��� �ִ��� ���θ� ����
    // private bool isBoxUnfolded = false; // CS0414: ������ ���� - ���ŵ�

    public GameObject unfoldedBoxPrefab; // ������ �ڽ� ������ (Unity Inspector���� ���� �ʿ�)
    private bool isNearWorkbench = false; // �÷��̾ �۾��� ��ó�� �ִ��� ����
    private bool isNearRecyclingBin = false; // �÷��̾ �и������� ��ó�� �ִ��� ����
    private float qKeyHoldTime = 0f; // Q Ű�� ������ �ִ� �ð��� ���� (�� ����)
    private const float UNFOLD_DURATION = 2f; // �ڽ��� ��ġ�� �� �ʿ��� �ð� (2�ʷ� ����)
    private bool isUnfolding = false; // �ڽ��� �������� ������ ���θ� ����
    private GameObject workbench; // ���� ������ �۾��� ������Ʈ ����
    private GameObject boxOnWorkbench; // �۾��� ���� �ö� �ִ� �Ϲ� �ڽ�
    private GameObject unfoldedBoxOnWorkbench; // �۾��� ���� �ִ� ������ �ڽ�

    // ������Ʈ�� ó�� ������ �� ȣ��Ǵ� �ʱ�ȭ �Լ�
    void Awake()
    {
        // ���� ������Ʈ���� Player ������Ʈ�� ������ ���� ����
        // player = GetComponent<Player>(); // Player Ŭ���� �����Ƿ� �ּ� ó��
        // �÷��̾��� Animator���� ������ ��(Bone)�� Transform�� ������
        Animator animator = GetComponentInChildren<Animator>(); // Animator ���� ã��
         if (animator != null)
         {
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
             // ������ ��ġ�� �չٴ� �������� �ణ ���� (0.15 ���� �̵�)
             if (rightHand != null)
             {
                rightHand.position = rightHand.position + rightHand.forward * 0.15f;
             } else {
                Debug.LogError("RightHand Transform�� ã�� �� �����ϴ�. Animator�� HumanBodyBones ������ Ȯ���ϼ���.");
             }
         } else {
             Debug.LogError("Animator ������Ʈ�� ã�� �� �����ϴ�.");
         }


        // ������ �ڽ� �������� Inspector���� �������� �ʾ����� ��� �޽��� ���
        if (unfoldedBoxPrefab == null)
        {
            Debug.LogWarning("������ �ڽ� ������(UnfoldedBoxPrefab)�� �������� �ʾҽ��ϴ�. Inspector���� �������ּ���.");
        }
    }

    // �� �����Ӹ��� ȣ��Ǵ� ������Ʈ �Լ�
    void Update()
    {
        // �۾��� ��ó�� �ְ�, �ڽ��� �������� ���� �ƴ� �� Ű �Է� ó��
        if (isNearWorkbench && !isUnfolding)
        {
            // ������: ���� ���� ��� (������ ��� �ִ���, �۾��� �� �ڽ� ���� ��)
            // Debug.Log("�۾��� ��ó - isHoldingTrash: " + isHoldingTrash + ", heldObject: " + (heldObject != null ? heldObject.name : "null") + ", boxOnWorkbench: " + (boxOnWorkbench != null ? boxOnWorkbench.name : "null") + ", unfoldedBoxOnWorkbench: " + (unfoldedBoxOnWorkbench != null ? unfoldedBoxOnWorkbench.name : "null"));

            // Q Ű�� ó�� ������ ��: �ڽ��� �۾��뿡 �ø��� ����
            if (Input.GetKeyDown(KeyCode.Q) && isHoldingTrash && heldObject != null && heldObject.CompareTag("Box") && boxOnWorkbench == null && unfoldedBoxOnWorkbench == null) // �۾��밡 ������� Ȯ��
            {
                Debug.Log("Q Ű ���� - �ڽ��� �۾��뿡 �ø�");
                PlaceBoxOnWorkbench(); // �ڽ��� �۾��� ���� �ø��� �Լ� ȣ��
            }

            // Q Ű�� ��� ������ ���� ��: �۾��� �� �ڽ��� ��ġ�� ���� �غ�
            if (Input.GetKey(KeyCode.Q) && boxOnWorkbench != null)
            {
                qKeyHoldTime += Time.deltaTime; // Q Ű ���� �ð� ����
                // Debug.Log("Q Ű ������ �� - qKeyHoldTime: " + qKeyHoldTime);
                if (qKeyHoldTime >= UNFOLD_DURATION) // 2�� �̻� ������ �ڽ� ��ħ ����
                {
                     if (!isUnfolding) // �ߺ� ���� ����
                     {
                        Debug.Log("Q Ű 2�� �̻� - �ڽ� ��ħ ����");
                        // heldObject = boxOnWorkbench; // �۾��� �� �ڽ��� ������� ���� (UnfoldBoxOnWorkbench �ڷ�ƾ ������ ó����)
                        StartCoroutine(UnfoldBoxOnWorkbench()); // �ڽ� ��ġ�� �ڷ�ƾ ����
                        qKeyHoldTime = 0f; // �ڷ�ƾ ���� �� �ʱ�ȭ
                     }
                }
            }

            // Q Ű�� ���� ��: ���� �ð� �ʱ�ȭ
            if (Input.GetKeyUp(KeyCode.Q))
            {
                // Debug.Log("Q Ű �� - qKeyHoldTime �ʱ�ȭ");
                qKeyHoldTime = 0f;
            }

            // �۾��� ��ó���� ������ �ڽ��� E Ű�� �ݱ�
            if (unfoldedBoxOnWorkbench != null && Input.GetKeyDown(KeyCode.E) && !isHoldingTrash)
            {
                Debug.Log("E Ű ���� - ������ �ڽ� �ݱ�");
                PickUpUnfoldedBox(); // ������ �ڽ��� �տ� ��� �Լ� ȣ��
            }
        }

        // �и������� ��ó���� E Ű�� ������ ����
        if (isNearRecyclingBin && isHoldingTrash && heldObject != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E Ű ���� - �и������� ������ ����");
            ReturnTrashToPool(); // �����⸦ �и����������� ������ �Լ� ȣ��
        }

         // �ֿ� �� �ִ� ������ ���� (E Ű ������ ��)
         if (!isHoldingTrash /* && player.GetCurrentToolIndex() == 0 */) // Player Ŭ���� �����Ƿ� ���� ���� �ּ�ó��
         {
             // PlayerController�� OnTriggerStay�� ������ ���� �ʿ� (�Ǵ� ���� �д�)
             // ���⼭�� Update���� EŰ �Է¸� Ȯ��
             if (Input.GetKeyDown(KeyCode.E))
             {
                 // ��ó ������ ���� �� �ݱ� ���� �ʿ�
                 TryPickUpNearbyObject();
             }
         }
    }

    // Ʈ���� ���� �ȿ� �ӹ����� ���� ȣ��Ǵ� �Լ�
    void OnTriggerStay(Collider other)
    {
         // PlayerController �� �ߺ� -> ���⼭�� ���� ����/��Ż �÷��׸� �����ϰ� ���� Ȯ���� Update���� �ϴ� ���� ȥ���� ���� �� ����

        // �۾��� �ֺ� ������ ������ ��
        if (other.CompareTag("WorkbenchArea"))
        {
            if (!isNearWorkbench) {
                 isNearWorkbench = true; // �۾��� ��ó �÷��� Ȱ��ȭ
                 // �۾����� �θ� ������Ʈ�� ������ �θ�, ������ ���� ������Ʈ�� workbench�� ����
                 workbench = other.transform.parent != null ? other.transform.parent.gameObject : other.gameObject;
                 Debug.Log("�۾��� ��ó ���� - workbench: " + workbench.name);
                 CheckItemsOnWorkbench(); // �۾��� �� ������ Ȯ��
            } else {
                 // �ʿ��ϴٸ� ��� Ȯ��
                 // CheckItemsOnWorkbench();
            }

        }

        // �и������� ��ó�� ������ ��
        if (other.CompareTag("RecyclingBin"))
        {
             if (!isNearRecyclingBin)
             {
                 isNearRecyclingBin = true; // �и������� ��ó �÷��� Ȱ��ȭ
                 Debug.Log("�и������� ��ó ����");
             }
        }
    }

    // Ʈ���� ������ ��� �� ȣ��Ǵ� �Լ�
    void OnTriggerExit(Collider other)
    {
        // ��� �ִ� ������Ʈ�� Ʈ���Ÿ� ����� ���� �ʱ�ȭ -> PlayerController�� ������ ����, ���⼭ ó������ ����

        // �۾��� �ֺ� ������ ����� ���� ���� �ʱ�ȭ
        if (other.CompareTag("WorkbenchArea"))
        {
            isNearWorkbench = false;
            workbench = null;
            boxOnWorkbench = null;
            unfoldedBoxOnWorkbench = null;
            qKeyHoldTime = 0f;
            isUnfolding = false; // �۾��� ����� ��ġ�� ���� ����
            Debug.Log("�۾��� ���");
        }

        // �и������� ������ ����� �÷��� �ʱ�ȭ
        if (other.CompareTag("RecyclingBin"))
        {
            isNearRecyclingBin = false;
            Debug.Log("�и������� ���");
        }
    }

    // �ֺ� ������ �ݱ� �õ� (Update���� ȣ��)
    private void TryPickUpNearbyObject()
    {
        // PlayerController�� OnTriggerStay ������ �����ϰ� ���� �ʿ�
        // ��: ���� ����� ������ ã��
        float pickupRadius = 2.0f; // �ݱ� �ݰ�
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, pickupRadius);
        GameObject closestObject = null;
        float minDistance = float.MaxValue;

        foreach (var col in nearbyColliders)
        {
            if (col.CompareTag("Can") || col.CompareTag("Box") || col.CompareTag("UnfoldedBox"))
            {
                 // �÷��̾� �ڽ��� �ƴ���, �̹� ��� �ִ� ������Ʈ�� �ƴ��� �� �߰� ���� Ȯ�� ����
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestObject = col.gameObject;
                }
            }
        }

        if (closestObject != null)
        {
             Debug.Log("E Ű ���� - ������Ʈ �ݱ� �õ�: " + closestObject.name);
             heldObject = closestObject; // �ֿ� ������� ����
             PickUpObject(); // �ݴ� �Լ� ȣ��
        }
    }


     // �۾��� �� ������ Ȯ�� �Լ�
     private void CheckItemsOnWorkbench()
     {
        if (workbench == null) return;

        // ���� ���� �ʱ�ȭ
        boxOnWorkbench = null;
        unfoldedBoxOnWorkbench = null;

        // �۾��� �� Ž�� (WorkBench ��ũ��Ʈ�� �����ϰ�)
        float checkRadius = 2.0f; // Ž�� �ݰ� (WorkBench ��ũ��Ʈ�� ��ġ��Ű�� ���� ����)
        Vector3 checkCenter = workbench.transform.position; // Ž�� �߽�
        Collider[] colliders = Physics.OverlapSphere(checkCenter, checkRadius);

        foreach (var col in colliders)
        {
            // �ڱ� �ڽ��̳� �÷��̾ ��� �ִ� ���� ����
            if (col.gameObject == this.gameObject || col.gameObject == heldObject) continue;
            // �۾��� ��ü�� ����
            if (col.gameObject == workbench) continue;


            if (col.CompareTag("Box"))
            {
                boxOnWorkbench = col.gameObject;
                // Debug.Log("�۾��� �� �Ϲ� �ڽ� ����: " + boxOnWorkbench.name);
                 break; // �ϳ��� ã���� �� (���ÿ� �� ������ �ö󰡴� ��� ����)
            }
            else if (col.CompareTag("UnfoldedBox"))
            {
                unfoldedBoxOnWorkbench = col.gameObject;
                // Debug.Log("�۾��� �� ������ �ڽ� ����: " + unfoldedBoxOnWorkbench.name);
                 break; // �ϳ��� ã���� ��
            }
        }
         // ����� �α״� ���� ���� �� �ѹ��� ����ϴ� ���� ����
        // Debug.Log($"�۾��� Ȯ�� ���: Box={boxOnWorkbench?.name ?? "null"}, Unfolded={unfoldedBoxOnWorkbench?.name ?? "null"}");
     }


    // �Ϲ� �����⸦ �տ� ��� �Լ�
    private void PickUpObject()
    {
        if (heldObject != null && !isHoldingTrash && rightHand != null) // ��� �ִ� ������Ʈ�� �ְ�, �̹� �����⸦ ��� ���� ������, ������ ������ ���� ��
        {
            // ������Ʈ�� �������� �ڽ����� ����
            heldObject.transform.SetParent(rightHand);
            heldObject.transform.localPosition = Vector3.zero; // ��ġ�� �տ� ����
            heldObject.transform.localRotation = Quaternion.identity; // ȸ���� �⺻������ ����

            // Rigidbody�� ������ ������ �������� ���߰� �տ� ����
            Rigidbody objRb = heldObject.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = true; // ���� ���� ��Ȱ��ȭ
                // objRb.velocity = Vector3.zero; // isKinematic=true�� �ʿ� ���� �� ����
                // objRb.angularVelocity = Vector3.zero;
            }

            // �浹 ó���� �����Ͽ� �÷��̾�� ������Ʈ�� ��ġ�� �ʵ��� ����
            Collider objCollider = heldObject.GetComponent<Collider>();
            Collider playerCollider = GetComponent<Collider>(); // PlayerController�� �ƴ� �� ��ũ��Ʈ�� ���� ������Ʈ�� �ݶ��̴�
            if (objCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(objCollider, playerCollider, true); // �÷��̾�� �浹 ����
                // objCollider.enabled = false; // �ݶ��̴��� ��Ȱ��ȭ�ϸ� �ٸ� �Ͱ��� �浹 �� ��. IgnoreCollision������ ����� �� ����.
            } else {
                // Debug.LogWarning("������Ʈ �Ǵ� �÷��̾� �ݶ��̴��� �����ϴ�.");
            }

            isHoldingTrash = true; // ������ ��� ���� �÷��� Ȱ��ȭ
            // isBoxUnfolded = false; // CS0414: ���ŵ� (������Ʈ �±׷� �Ǵ�)
            Debug.Log("������Ʈ �ֿ�: " + heldObject.name);

             // �۾��뿡 �ִ� ���� �ֿ��ٸ� �۾��� ���� ����
             if (heldObject == boxOnWorkbench) boxOnWorkbench = null;
             if (heldObject == unfoldedBoxOnWorkbench) unfoldedBoxOnWorkbench = null;
        } else if (rightHand == null) {
             Debug.LogError("RightHand Transform�� �������� �ʾ� ������Ʈ�� �� �� �����ϴ�.");
        }
    }

    // �ڽ��� �۾��� ���� �ø��� �Լ�
    private void PlaceBoxOnWorkbench()
    {
        if (heldObject != null && workbench != null && heldObject.CompareTag("Box")) // ��� �ִ� 'Box' ������Ʈ�� �۾��밡 ������ ��
        {
            // �۾��� ���� ��ġ ��� (WorkBench ��ũ��Ʈ�� ������ �����ϰ�)
            // Vector3 workbenchTop = workbench.transform.position + Vector3.up * (workbench.transform.localScale.y * 0.5f + heldObject.transform.localScale.y * 0.5f + 0.1f); // �� �� ��Ȯ�� ���
             Vector3 workbenchTop = workbench.transform.position + Vector3.up * 1.0f; // �ܼ�ȭ (���� ���� �ʿ�)

            heldObject.transform.SetParent(null); // �տ��� �и�
            heldObject.transform.position = workbenchTop; // �۾��� ���� �̵�
            heldObject.transform.rotation = Quaternion.identity; // ȸ�� �ʱ�ȭ

            // Rigidbody ���� ����
            Rigidbody objRb = heldObject.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = false; // ���� ���� Ȱ��ȭ
                 objRb.velocity = Vector3.zero; // ���� �� �ӵ� �ʱ�ȭ
                 objRb.angularVelocity = Vector3.zero;
            }
            Collider objCollider = heldObject.GetComponent<Collider>();
            Collider playerCollider = GetComponent<Collider>();
            if (objCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(objCollider, playerCollider, false); // �浹 �ٽ� Ȱ��ȭ
                // objCollider.enabled = true; // ��Ȱ��ȭ �ߴٸ� �ٽ� Ȱ��ȭ
            }

            boxOnWorkbench = heldObject; // �۾��� �� �ڽ��� ����
            isHoldingTrash = false; // �տ��� ����
            heldObject = null; // ��� �ִ� ������Ʈ �ʱ�ȭ
            Debug.Log("�ڽ��� �۾��뿡 �ø� - Q Ű�� 2�� ���� ���� ��ĥ �� ����");
        }
    }

    // �ڽ��� �۾��뿡�� ��ġ�� �ڷ�ƾ (�ִϸ��̼� ����)
    private IEnumerator UnfoldBoxOnWorkbench()
    {
        if (boxOnWorkbench == null || unfoldedBoxPrefab == null || workbench == null || isUnfolding)
        {
            // �ʼ� ���� �� �ϳ��� ���ų� �̹� ���� ���̸� ����
            yield break; // �ڷ�ƾ ��� ����
        }

        isUnfolding = true; // ��ġ�� �� �÷��� Ȱ��ȭ
        GameObject originalBox = boxOnWorkbench; // ���� �ڽ� ���� ����
        boxOnWorkbench = null; // �۾��� �� �Ϲ� �ڽ� ���� ���� (�߿�)

        // ���� �ڽ��� ���� ���� �� ��Ȱ��ȭ �Ǵ� ���� �غ�
        Vector3 startPosition = originalBox.transform.position;
        Quaternion startRotation = originalBox.transform.rotation;
        Vector3 startScale = originalBox.transform.localScale;
        // originalBox.SetActive(false); // ��� �����

        // ������ �ڽ� �����տ��� ��ǥ ũ�� �������� (������ ��ü ������ ���)
        Vector3 targetScale = unfoldedBoxPrefab.transform.localScale;


        // ���ο� ������ �ڽ� ���� (Instantiate ��� ����, �ʿ�� PoolManager ���)
        GameObject newUnfoldedBox = Instantiate(unfoldedBoxPrefab, startPosition, startRotation);
        newUnfoldedBox.tag = "UnfoldedBox"; // �±� Ȯ��
        newUnfoldedBox.transform.localScale = startScale; // �ʱ� ũ��� ���� �ڽ��� ���� ����

        // ũ�� ��ȭ �ִϸ��̼� (Lerp ���)
        float elapsed = 0f;
        while (elapsed < UNFOLD_DURATION)
        {
            elapsed += Time.deltaTime; // ��� �ð� ����
            float t = Mathf.Clamp01(elapsed / UNFOLD_DURATION); // ����� (0~1), Clamp01�� ���� ����
            newUnfoldedBox.transform.localScale = Vector3.Lerp(startScale, targetScale, t); // ũ�� ����
            yield return null; // ���� �����ӱ��� ���
        }
        newUnfoldedBox.transform.localScale = targetScale; // ���� ũ�� Ȯ���� ����

        // ���� �ڽ� ���� (�ִϸ��̼� ��)
        Destroy(originalBox); // Instantiate ��� �� Destroy, PoolManager ��� �� ReturnToPool

        // ���� ������Ʈ
        unfoldedBoxOnWorkbench = newUnfoldedBox; // ������ �ڽ��� �۾��� �� ������Ʈ�� ����
        // boxOnWorkbench = null; // �̹� ������ null ó����
        // isHoldingTrash = false; // �տ� �� �� �ƴϹǷ� ���� ����
        // heldObject = null; // �տ� �� �� �ƴϹǷ� ���� ����
        // isBoxUnfolded = true; // CS0414: ���ŵ�
        qKeyHoldTime = 0f; // Q Ű �ð� �ʱ�ȭ
        Debug.Log("�ڽ� ��ħ �Ϸ� - E Ű�� �ֿ� �� ����");

        isUnfolding = false; // ��ħ �Ϸ�
    }


    // ������ �ڽ��� �տ� ��� �Լ�
    private void PickUpUnfoldedBox()
    {
        if (unfoldedBoxOnWorkbench != null && !isHoldingTrash && rightHand != null) // �۾��뿡 ������ �ڽ��� �ְ�, ���� ��� ������, ������ ������ ���� ��
        {
            heldObject = unfoldedBoxOnWorkbench; // �� ������Ʈ�� ����
            unfoldedBoxOnWorkbench = null; // �۾��뿡�� ���� (�߿�)

            PickUpObject(); // ���� �ݱ� ���� ��� (PickUpObject ���ο��� isHoldingTrash=true ó����)

            // isBoxUnfolded = true; // CS0414: ���ŵ� (�±׷� �Ǵ�)
            Debug.Log("������ �ڽ� �ֿ�: " + heldObject.name);
        } else if (rightHand == null) {
            Debug.LogError("RightHand Transform�� �������� �ʾ� ������Ʈ�� �� �� �����ϴ�.");
        }
    }

    // �и������忡�� �����⸦ �����ϴ� �Լ�
    private void ReturnTrashToPool()
    {
        if (isHoldingTrash && heldObject != null) // �����⸦ ��� ���� ��
        {
             bool removed = false;
             string tag = heldObject.tag; // �̸� �±� ����

            // Obstacle ������Ʈ Ȯ�� (������, PoolManager ��� �� �ʿ�)
            Obstacle obstacle = heldObject.GetComponent<Obstacle>();

            if (tag == "UnfoldedBox" || tag == "Can") // ������ �ڽ� �Ǵ� ĵ�̸�
            {
                Debug.Log(tag + " �и����� �õ�");
                 if (obstacle != null) {
                     // obstacle.CleanObstacle(); // PoolManager ����� ��� �� �Լ� ȣ��
                     PoolManager.Instance?.ReturnToPool(heldObject.name.Replace("(Clone)","").Trim(), obstacle); // PoolManager ��� ����
                     // Destroy(heldObject); // PoolManager �Ⱦ��� ���� ����
                 } else {
                     Destroy(heldObject); // Obstacle ������ ���� ����
                 }
                removed = true;
                // isBoxUnfolded = false; // CS0414: ���ŵ�
            }
            else if (tag == "Box") // �Ϲ� �ڽ���
            {
                Debug.Log("�������� ���� �ڽ��� �и����� �Ұ�");
                // �������� ����
            } else {
                 Debug.LogWarning($"�� �� ���� �±�({tag})�� ������Ʈ �и����� �õ�");
                 // �ʿ�� ���� ���� �߰�
            }

            if (removed)
            {
                 heldObject = null; // ��� �ִ� ������Ʈ �ʱ�ȭ
                 isHoldingTrash = false; // ������ ��� ���� �÷��� ��Ȱ��ȭ
                 Debug.Log(tag + " �и����� �Ϸ�");
            }
        }
    }

    // �ܺο��� �����⸦ ��� �ִ��� Ȯ���� �� �ִ� �Լ� (�ʿ��ϴٸ� public ����)
    public bool IsHoldingTrash()
    {
        return isHoldingTrash;
    }
}