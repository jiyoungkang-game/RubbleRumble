using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // �̵� �ӵ� ���� ����
    public float moveSpeed = 10.0f;  // ĳ������ �⺻ �̵� �ӵ� (����: �ʴ� �̵� �Ÿ�, Inspector���� ���� ����)
    public float runSpeedMultiplier = 1.8f;  // �޸��� �� �ӵ� ���� ���� (�⺻��: 1.8��, Inspector���� ���� ����)

    // �Է°� ���� ����
    private float hAxis;  // ������ �Է°� (�¿� �̵�, -1: ����, 0: ����, 1: ������)
    private float vAxis;  // ������ �Է°� (���� �̵�, -1: �ڷ�, 0: ����, 1: ������)
    private bool isShiftDown;  // Shift Ű�� ���� �ִ��� ���¸� Ȯ���ϴ� �÷��� (�޸��� ���� �Ǵ�)

    // ������Ʈ ���� ����
    private Animator animator;  // ĳ���� �ִϸ��̼��� �����ϱ� ���� Animator ������Ʈ
    private Rigidbody rb;  // ĳ������ ������ �������� ó���ϱ� ���� Rigidbody ������Ʈ
    private Vector3 moveVec;  // ĳ������ �̵� ���� ���� (�Է°��� ������� ���)

    // ���� ���� ����
    public GameObject[] toolPrefabs;  // 1, 2, 3�� Ű�� ������ ���� ������ �迭 (Inspector���� ����)
    private GameObject[] tools;  // ���� ������ ���� �ν��Ͻ��� �����ϴ� �迭
    private int currentToolIndex = -1;  // ���� ������ ������ �ε��� (-1�� �ƹ� ������ �������� ������ �ǹ�)
    private Transform rightHand;  // ĳ���� ������ Transform (������ ���� ��ġ)

    // ���� ȿ�� ���� ����
    public ParticleSystem dustEffect;  // �޸��� �� �߻��ϴ� ���� ȿ���� ���� Particle System
    private bool isDustPlaying = false;  // ���� ȿ���� ���� ��� ������ ���¸� �����ϴ� �÷���

    // �ʱ�ȭ �޼���: ��ü�� ������ �� ȣ���
    void Awake()
    {
        // �ڽ� ������Ʈ���� Animator ������Ʈ�� ������ ���� ����
        animator = GetComponentInChildren<Animator>();
        // Rigidbody ������Ʈ�� ������ ������ �̵��� �غ�
        rb = GetComponent<Rigidbody>();
        // Animator���� ������ ��(Bone)�� Transform�� ������ ���� ��ġ�� ���
        rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

        // ���� �迭 �ʱ�ȭ �� �ν��Ͻ� ����
        tools = new GameObject[toolPrefabs.Length];
        for (int i = 0; i < toolPrefabs.Length; i++)
        {
            if (toolPrefabs[i] != null)  // �������� null�� �ƴ� ��쿡�� ����
            {
                // ������ ������ ��ġ�� �����ϰ�, �������� �θ�� ����
                tools[i] = Instantiate(toolPrefabs[i], rightHand.position, rightHand.rotation, rightHand);
                // ������ �ʱ� ȸ���� ������ �ڿ������� ���̵��� ����
                tools[i].transform.localRotation = Quaternion.Euler(30, 20, -60);
                // �ʱ� ���¿��� ������ ��Ȱ��ȭ�Ͽ� ȭ�鿡 ������ �ʰ� ��
                tools[i].SetActive(false);
            }
        }
        // Mop3 ������ ��ġ ����
        tools[2].transform.localPosition += Vector3.up * 0.1f;
        tools[2].transform.localPosition += Vector3.forward * 0.1f;
        tools[2].transform.localRotation = Quaternion.Euler(90, 0, 45);

        // ���� ȿ���� �����Ǿ� �ִ��� Ȯ��
        if (dustEffect == null)
        {
            // Inspector���� ���� ȿ���� �������� ���� ��� ��� ���
            Debug.LogWarning("���� ȿ��(Dust Effect)�� �������� �ʾҽ��ϴ�. Inspector���� �������ּ���.");
        }
    }

    // �� �����Ӹ��� ȣ��Ǵ� ������Ʈ �޼���
    void Update()
    {
        // �Է°��� �ǽð����� ����
        hAxis = Input.GetAxisRaw("Horizontal");  // ������ �Է°� �������� (-1, 0, 1)
        vAxis = Input.GetAxisRaw("Vertical");    // ������ �Է°� �������� (-1, 0, 1)
        // ���� �Ǵ� ���� Shift Ű�� ���ȴ��� Ȯ�� (�޸��� ���� ����)
        isShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // �̵� ������ �Է°����� ����ϰ� �ε巴�� ��ȯ
        Vector3 rawInput = new Vector3(hAxis, 0, vAxis).normalized;  // �Է°��� ����ȭ�Ͽ� ũ�� 1�� ����
        moveVec = Vector3.Lerp(moveVec, rawInput, Time.deltaTime * 15f);  // �ε巴�� �̵� ���� ����

        // ���� ���� Ű �Է� ó��
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipTool(0);  // 1�� Ű: ù ��° ���� ����
        else if (Input.GetKeyDown(KeyCode.Alpha2)) EquipTool(1);  // 2�� Ű: �� ��° ���� ����
        else if (Input.GetKeyDown(KeyCode.Alpha3)) EquipTool(2);  // 3�� Ű: �� ��° ���� ����
    }

    // ���� �ð� �������� ȣ��Ǵ� ���� ������Ʈ �޼���
    void FixedUpdate()
    {
        // ĳ���Ͱ� �̵� ������ Ȯ��
        bool isMoving = moveVec != Vector3.zero;
        // �̵� �ӵ��� �ִϸ����Ϳ� ���� (�޸��� ���� �ݿ�)
        float speed = moveVec.magnitude;
        if (isMoving && isShiftDown) speed *= runSpeedMultiplier;  // �޸��� �� �ӵ� ����
        animator.SetFloat("speed", speed);  // �ִϸ������� speed �Ķ���� ����

        // ĳ���� �̵� �� ȸ�� ó��
        if (isMoving)
        {
            // �̵� ������ �������� ĳ���� ȸ�� ���
            Quaternion targetRotation = Quaternion.LookRotation(moveVec);
            Vector3 euler = targetRotation.eulerAngles;
            targetRotation = Quaternion.Euler(0, euler.y, 0);  // Y�� ȸ���� ����

            // ȸ�� �ӵ� ����: �޸��� �� �� ������ ȸ��
            float rotationSpeed = isShiftDown ? 15f : 10f;
            // �ε巴�� ȸ�� ����
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);

            // �̵� ����� �ӵ��� ����� ���������� �̵�
            Vector3 moveDirection = transform.forward * moveVec.magnitude;
            float currentSpeed = isShiftDown ? moveSpeed * runSpeedMultiplier : moveSpeed;  // �޸��� �ӵ� �ݿ�
            rb.velocity = new Vector3(moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);  // Y�� �ӵ��� ����
        }
        else
        {
            // �Է��� ������ ���� �ӵ��� 0���� ����, �߷��� ����
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // ���� ȿ���� �̵� ���¿� ���� ó��
        HandleDustEffect(isMoving && isShiftDown);
    }

    // ������ �����ϴ� �޼���
    void EquipTool(int index)
    {
        // �ε����� ��ȿ���� Ȯ��
        if (index < 0 || index >= tools.Length || tools[index] == null) return;

        // ���� ������ ������ ������ ��Ȱ��ȭ
        if (currentToolIndex != -1) tools[currentToolIndex].SetActive(false);

        // ���ο� ������ Ȱ��ȭ�ϰ� ���� �ε��� ������Ʈ
        tools[index].SetActive(true);
        currentToolIndex = index;
    }

    // ���� ������ ������ �ε����� ��ȯ�ϴ� �޼��� (TrashHandler���� ȣ��)
    public int GetCurrentToolIndex()
    {
        return currentToolIndex; // ���� ���� �ε��� ��ȯ
    }

    // ������ Transform�� ��ȯ�ϴ� �޼��� (TrashHandler���� ȣ��)
    public Transform GetRightHand()
    {
        return rightHand; // ������ Transform ��ȯ
    }

    // ���� ȿ���� �����ϴ� �޼���
    void HandleDustEffect(bool isRunning)
    {
        if (dustEffect != null)  // ���� ȿ���� �����Ǿ� ���� ���� ����
        {
            if (isRunning && !isDustPlaying)  // �޸��� ���̰� ȿ���� ��� ���� �ƴϸ�
            {
                dustEffect.Play();  // ���� ȿ�� ��� ����
                isDustPlaying = true;  // ��� ���·� ������Ʈ
            }
            else if (!isRunning && isDustPlaying)  // �޸��� �ʰ� ȿ���� ��� ���̸�
            {
                dustEffect.Stop();  // ���� ȿ�� ����
                isDustPlaying = false;  // ���� ���·� ������Ʈ
            }
        }
    }
}