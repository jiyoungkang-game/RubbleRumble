using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 이동 속도 관련 변수
    public float moveSpeed = 10.0f;  // 캐릭터의 기본 이동 속도 (단위: 초당 이동 거리, Inspector에서 설정 가능)
    public float runSpeedMultiplier = 1.8f;  // 달리기 시 속도 증가 배율 (기본값: 1.8배, Inspector에서 조정 가능)

    // 입력값 저장 변수
    private float hAxis;  // 수평축 입력값 (좌우 이동, -1: 왼쪽, 0: 없음, 1: 오른쪽)
    private float vAxis;  // 수직축 입력값 (상하 이동, -1: 뒤로, 0: 없음, 1: 앞으로)
    private bool isShiftDown;  // Shift 키가 눌려 있는지 상태를 확인하는 플래그 (달리기 여부 판단)

    // 컴포넌트 참조 변수
    private Animator animator;  // 캐릭터 애니메이션을 제어하기 위한 Animator 컴포넌트
    private Rigidbody rb;  // 캐릭터의 물리적 움직임을 처리하기 위한 Rigidbody 컴포넌트
    private Vector3 moveVec;  // 캐릭터의 이동 방향 벡터 (입력값을 기반으로 계산)

    // 먼지 효과 관련 변수
    public ParticleSystem dustEffect;  // 달리기 시 발생하는 먼지 효과를 위한 Particle System
    private bool isDustPlaying = false;  // 먼지 효과가 현재 재생 중인지 상태를 저장하는 플래그

    void Awake()
    {
        // 자식 오브젝트에서 Animator 컴포넌트를 가져와 참조 설정
        animator = GetComponentInChildren<Animator>();
        // Rigidbody 컴포넌트를 가져와 물리적 이동을 준비
        rb = GetComponent<Rigidbody>();

        // 먼지 효과가 설정되어 있는지 확인
        if (dustEffect == null)
        {
            // Inspector에서 먼지 효과가 설정되지 않은 경우 경고 출력
            Debug.LogWarning("먼지 효과(Dust Effect)가 설정되지 않았습니다. Inspector에서 설정해주세요.");
        }
    }

    void Update()
    {
        // 입력값을 실시간으로 갱신
        hAxis = Input.GetAxisRaw("Horizontal");  // 수평축 입력값 가져오기 (-1, 0, 1)
        vAxis = Input.GetAxisRaw("Vertical");    // 수직축 입력값 가져오기 (-1, 0, 1)
        // 좌측 또는 우측 Shift 키가 눌렸는지 확인 (달리기 상태 감지)
        isShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // 이동 방향을 입력값으로 계산하고 부드럽게 전환
        Vector3 rawInput = new Vector3(hAxis, 0, vAxis).normalized;  // 입력값을 정규화하여 크기 1로 맞춤
        moveVec = Vector3.Lerp(moveVec, rawInput, Time.deltaTime * 15f);  // 부드럽게 이동 방향 보간
    }

    // 물리 시간 간격으로 호출되는 고정 업데이트 메서드
    void FixedUpdate()
    {
        // 캐릭터가 이동 중인지 확인
        bool isMoving = moveVec != Vector3.zero;
        // 이동 속도를 애니메이터에 전달 (달리기 상태 반영)
        float speed = moveVec.magnitude;
        if (isMoving && isShiftDown) speed *= runSpeedMultiplier;  // 달리기 시 속도 증가
        animator.SetFloat("speed", speed);  // 애니메이터의 speed 파라미터 설정

        // 캐릭터 이동 및 회전 처리
        if (isMoving)
        {
            // 이동 방향을 기준으로 캐릭터 회전 계산
            Quaternion targetRotation = Quaternion.LookRotation(moveVec);
            Vector3 euler = targetRotation.eulerAngles;
            targetRotation = Quaternion.Euler(0, euler.y, 0);  // Y축 회전만 적용

            // 회전 속도 설정: 달리기 시 더 빠르게 회전
            float rotationSpeed = isShiftDown ? 15f : 10f;
            // 부드럽게 회전 적용
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);

            // 이동 방향과 속도를 계산해 물리적으로 이동
            Vector3 moveDirection = transform.forward * moveVec.magnitude;
            float currentSpeed = isShiftDown ? moveSpeed * runSpeedMultiplier : moveSpeed;  // 달리기 속도 반영
            rb.velocity = new Vector3(moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);  // Y축 속도는 유지
        }
        else
        {
            // 입력이 없으면 수평 속도를 0으로 설정, 중력은 유지
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // 먼지 효과를 이동 상태에 따라 처리
        HandleDustEffect(isMoving && isShiftDown);
    }

    void HandleDustEffect(bool isRunning)
    {
        if (dustEffect != null)  // 먼지 효과가 설정되어 있을 때만 실행
        {
            if (isRunning && !isDustPlaying)  // 달리기 중이고 효과가 재생 중이 아니면
            {
                dustEffect.Play();  // 먼지 효과 재생 시작
                isDustPlaying = true;  // 재생 상태로 업데이트
            }
            else if (!isRunning && isDustPlaying)  // 달리지 않고 효과가 재생 중이면
            {
                dustEffect.Stop();  // 먼지 효과 중지
                isDustPlaying = false;  // 중지 상태로 업데이트
            }
        }
    }
}
