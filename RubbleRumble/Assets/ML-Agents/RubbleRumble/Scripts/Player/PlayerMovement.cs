using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
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

    // ���� ȿ�� ���� ����
    public ParticleSystem dustEffect;  // �޸��� �� �߻��ϴ� ���� ȿ���� ���� Particle System
    private bool isDustPlaying = false;  // ���� ȿ���� ���� ��� ������ ���¸� �����ϴ� �÷���

    void Awake()
    {
        // �ڽ� ������Ʈ���� Animator ������Ʈ�� ������ ���� ����
        animator = GetComponentInChildren<Animator>();
        // Rigidbody ������Ʈ�� ������ ������ �̵��� �غ�
        rb = GetComponent<Rigidbody>();

        // ���� ȿ���� �����Ǿ� �ִ��� Ȯ��
        if (dustEffect == null)
        {
            // Inspector���� ���� ȿ���� �������� ���� ��� ��� ���
            Debug.LogWarning("���� ȿ��(Dust Effect)�� �������� �ʾҽ��ϴ�. Inspector���� �������ּ���.");
        }
    }

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
