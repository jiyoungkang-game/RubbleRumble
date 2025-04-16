using System.Collections;
using UnityEngine;

// 참고: 이 TrashHandler 스크립트는 PlayerController와 기능이 많이 중복됩니다.
// PlayerController 하나로 통합하거나, 역할을 명확히 분리하는 리팩토링을 고려해볼 수 있습니다.
// 여기서는 에러만 해결하는 방향으로 수정합니다.
public class TrashHandler : MonoBehaviour
{
    // private Player player; // Player 클래스가 없으므로 주석 처리 또는 삭제
    private Transform rightHand; // 쓰레기나 도구를 붙일 플레이어의 오른손 위치 (Transform)
    private GameObject heldObject; // 플레이어가 현재 들고 있는 오브젝트 (쓰레기나 박스 등)
    private bool isHoldingTrash = false; // 플레이어가 쓰레기를 들고 있는지 여부를 추적
    // private bool isBoxUnfolded = false; // CS0414: 사용되지 않음 - 제거됨

    public GameObject unfoldedBoxPrefab; // 펼쳐진 박스 프리팹 (Unity Inspector에서 설정 필요)
    private bool isNearWorkbench = false; // 플레이어가 작업대 근처에 있는지 여부
    private bool isNearRecyclingBin = false; // 플레이어가 분리수거장 근처에 있는지 여부
    private float qKeyHoldTime = 0f; // Q 키를 누르고 있는 시간을 측정 (초 단위)
    private const float UNFOLD_DURATION = 2f; // 박스를 펼치는 데 필요한 시간 (2초로 고정)
    private bool isUnfolding = false; // 박스가 펼쳐지는 중인지 여부를 추적
    private GameObject workbench; // 현재 감지된 작업대 오브젝트 참조
    private GameObject boxOnWorkbench; // 작업대 위에 올라가 있는 일반 박스
    private GameObject unfoldedBoxOnWorkbench; // 작업대 위에 있는 펼쳐진 박스

    // 오브젝트가 처음 생성될 때 호출되는 초기화 함수
    void Awake()
    {
        // 현재 오브젝트에서 Player 컴포넌트를 가져와 참조 설정
        // player = GetComponent<Player>(); // Player 클래스 없으므로 주석 처리
        // 플레이어의 Animator에서 오른손 뼈(Bone)의 Transform을 가져옴
        Animator animator = GetComponentInChildren<Animator>(); // Animator 먼저 찾기
         if (animator != null)
         {
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
             // 오른손 위치를 손바닥 방향으로 약간 조정 (0.15 유닛 이동)
             if (rightHand != null)
             {
                rightHand.position = rightHand.position + rightHand.forward * 0.15f;
             } else {
                Debug.LogError("RightHand Transform을 찾을 수 없습니다. Animator와 HumanBodyBones 설정을 확인하세요.");
             }
         } else {
             Debug.LogError("Animator 컴포넌트를 찾을 수 없습니다.");
         }


        // 펼쳐진 박스 프리팹이 Inspector에서 설정되지 않았으면 경고 메시지 출력
        if (unfoldedBoxPrefab == null)
        {
            Debug.LogWarning("펼쳐진 박스 프리팹(UnfoldedBoxPrefab)이 설정되지 않았습니다. Inspector에서 연결해주세요.");
        }
    }

    // 매 프레임마다 호출되는 업데이트 함수
    void Update()
    {
        // 작업대 근처에 있고, 박스가 펼쳐지는 중이 아닐 때 키 입력 처리
        if (isNearWorkbench && !isUnfolding)
        {
            // 디버깅용: 현재 상태 출력 (쓰레기 들고 있는지, 작업대 위 박스 상태 등)
            // Debug.Log("작업대 근처 - isHoldingTrash: " + isHoldingTrash + ", heldObject: " + (heldObject != null ? heldObject.name : "null") + ", boxOnWorkbench: " + (boxOnWorkbench != null ? boxOnWorkbench.name : "null") + ", unfoldedBoxOnWorkbench: " + (unfoldedBoxOnWorkbench != null ? unfoldedBoxOnWorkbench.name : "null"));

            // Q 키를 처음 눌렀을 때: 박스를 작업대에 올리는 동작
            if (Input.GetKeyDown(KeyCode.Q) && isHoldingTrash && heldObject != null && heldObject.CompareTag("Box") && boxOnWorkbench == null && unfoldedBoxOnWorkbench == null) // 작업대가 비었는지 확인
            {
                Debug.Log("Q 키 눌림 - 박스를 작업대에 올림");
                PlaceBoxOnWorkbench(); // 박스를 작업대 위에 올리는 함수 호출
            }

            // Q 키를 계속 누르고 있을 때: 작업대 위 박스를 펼치는 동작 준비
            if (Input.GetKey(KeyCode.Q) && boxOnWorkbench != null)
            {
                qKeyHoldTime += Time.deltaTime; // Q 키 누른 시간 누적
                // Debug.Log("Q 키 누르는 중 - qKeyHoldTime: " + qKeyHoldTime);
                if (qKeyHoldTime >= UNFOLD_DURATION) // 2초 이상 누르면 박스 펼침 시작
                {
                     if (!isUnfolding) // 중복 실행 방지
                     {
                        Debug.Log("Q 키 2초 이상 - 박스 펼침 시작");
                        // heldObject = boxOnWorkbench; // 작업대 위 박스를 대상으로 설정 (UnfoldBoxOnWorkbench 코루틴 내에서 처리됨)
                        StartCoroutine(UnfoldBoxOnWorkbench()); // 박스 펼치는 코루틴 시작
                        qKeyHoldTime = 0f; // 코루틴 시작 시 초기화
                     }
                }
            }

            // Q 키를 뗐을 때: 누른 시간 초기화
            if (Input.GetKeyUp(KeyCode.Q))
            {
                // Debug.Log("Q 키 뗌 - qKeyHoldTime 초기화");
                qKeyHoldTime = 0f;
            }

            // 작업대 근처에서 펼쳐진 박스를 E 키로 줍기
            if (unfoldedBoxOnWorkbench != null && Input.GetKeyDown(KeyCode.E) && !isHoldingTrash)
            {
                Debug.Log("E 키 눌림 - 펼쳐진 박스 줍기");
                PickUpUnfoldedBox(); // 펼쳐진 박스를 손에 드는 함수 호출
            }
        }

        // 분리수거장 근처에서 E 키로 쓰레기 제거
        if (isNearRecyclingBin && isHoldingTrash && heldObject != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E 키 눌림 - 분리수거장 쓰레기 제거");
            ReturnTrashToPool(); // 쓰레기를 분리수거장으로 보내는 함수 호출
        }

         // 주울 수 있는 아이템 감지 (E 키 누르기 전)
         if (!isHoldingTrash /* && player.GetCurrentToolIndex() == 0 */) // Player 클래스 없으므로 도구 조건 주석처리
         {
             // PlayerController의 OnTriggerStay와 유사한 로직 필요 (또는 역할 분담)
             // 여기서는 Update에서 E키 입력만 확인
             if (Input.GetKeyDown(KeyCode.E))
             {
                 // 근처 아이템 감지 및 줍기 로직 필요
                 TryPickUpNearbyObject();
             }
         }
    }

    // 트리거 영역 안에 머무르는 동안 호출되는 함수
    void OnTriggerStay(Collider other)
    {
         // PlayerController 와 중복 -> 여기서는 영역 진입/이탈 플래그만 설정하고 상태 확인은 Update에서 하는 것이 혼란을 줄일 수 있음

        // 작업대 주변 영역에 들어왔을 때
        if (other.CompareTag("WorkbenchArea"))
        {
            if (!isNearWorkbench) {
                 isNearWorkbench = true; // 작업대 근처 플래그 활성화
                 // 작업대의 부모 오브젝트가 있으면 부모를, 없으면 현재 오브젝트를 workbench로 설정
                 workbench = other.transform.parent != null ? other.transform.parent.gameObject : other.gameObject;
                 Debug.Log("작업대 근처 진입 - workbench: " + workbench.name);
                 CheckItemsOnWorkbench(); // 작업대 위 아이템 확인
            } else {
                 // 필요하다면 계속 확인
                 // CheckItemsOnWorkbench();
            }

        }

        // 분리수거장 근처에 들어왔을 때
        if (other.CompareTag("RecyclingBin"))
        {
             if (!isNearRecyclingBin)
             {
                 isNearRecyclingBin = true; // 분리수거장 근처 플래그 활성화
                 Debug.Log("분리수거장 근처 진입");
             }
        }
    }

    // 트리거 영역을 벗어날 때 호출되는 함수
    void OnTriggerExit(Collider other)
    {
        // 들고 있는 오브젝트가 트리거를 벗어나면 참조 초기화 -> PlayerController와 동일한 문제, 여기서 처리하지 않음

        // 작업대 주변 영역을 벗어나면 관련 변수 초기화
        if (other.CompareTag("WorkbenchArea"))
        {
            isNearWorkbench = false;
            workbench = null;
            boxOnWorkbench = null;
            unfoldedBoxOnWorkbench = null;
            qKeyHoldTime = 0f;
            isUnfolding = false; // 작업대 벗어나면 펼치기 상태 해제
            Debug.Log("작업대 벗어남");
        }

        // 분리수거장 영역을 벗어나면 플래그 초기화
        if (other.CompareTag("RecyclingBin"))
        {
            isNearRecyclingBin = false;
            Debug.Log("분리수거장 벗어남");
        }
    }

    // 주변 아이템 줍기 시도 (Update에서 호출)
    private void TryPickUpNearbyObject()
    {
        // PlayerController의 OnTriggerStay 로직과 유사하게 구현 필요
        // 예: 가장 가까운 아이템 찾기
        float pickupRadius = 2.0f; // 줍기 반경
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, pickupRadius);
        GameObject closestObject = null;
        float minDistance = float.MaxValue;

        foreach (var col in nearbyColliders)
        {
            if (col.CompareTag("Can") || col.CompareTag("Box") || col.CompareTag("UnfoldedBox"))
            {
                 // 플레이어 자신이 아닌지, 이미 들고 있는 오브젝트가 아닌지 등 추가 조건 확인 가능
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
             Debug.Log("E 키 눌림 - 오브젝트 줍기 시도: " + closestObject.name);
             heldObject = closestObject; // 주울 대상으로 설정
             PickUpObject(); // 줍는 함수 호출
        }
    }


     // 작업대 위 아이템 확인 함수
     private void CheckItemsOnWorkbench()
     {
        if (workbench == null) return;

        // 이전 상태 초기화
        boxOnWorkbench = null;
        unfoldedBoxOnWorkbench = null;

        // 작업대 위 탐지 (WorkBench 스크립트와 유사하게)
        float checkRadius = 2.0f; // 탐지 반경 (WorkBench 스크립트와 일치시키는 것이 좋음)
        Vector3 checkCenter = workbench.transform.position; // 탐지 중심
        Collider[] colliders = Physics.OverlapSphere(checkCenter, checkRadius);

        foreach (var col in colliders)
        {
            // 자기 자신이나 플레이어가 들고 있는 것은 제외
            if (col.gameObject == this.gameObject || col.gameObject == heldObject) continue;
            // 작업대 자체도 제외
            if (col.gameObject == workbench) continue;


            if (col.CompareTag("Box"))
            {
                boxOnWorkbench = col.gameObject;
                // Debug.Log("작업대 위 일반 박스 감지: " + boxOnWorkbench.name);
                 break; // 하나만 찾으면 됨 (동시에 두 종류가 올라가는 경우 방지)
            }
            else if (col.CompareTag("UnfoldedBox"))
            {
                unfoldedBoxOnWorkbench = col.gameObject;
                // Debug.Log("작업대 위 펼쳐진 박스 감지: " + unfoldedBoxOnWorkbench.name);
                 break; // 하나만 찾으면 됨
            }
        }
         // 디버그 로그는 상태 변경 시 한번만 출력하는 것이 좋음
        // Debug.Log($"작업대 확인 결과: Box={boxOnWorkbench?.name ?? "null"}, Unfolded={unfoldedBoxOnWorkbench?.name ?? "null"}");
     }


    // 일반 쓰레기를 손에 드는 함수
    private void PickUpObject()
    {
        if (heldObject != null && !isHoldingTrash && rightHand != null) // 들고 있는 오브젝트가 있고, 이미 쓰레기를 들고 있지 않으며, 오른손 참조가 있을 때
        {
            // 오브젝트를 오른손의 자식으로 설정
            heldObject.transform.SetParent(rightHand);
            heldObject.transform.localPosition = Vector3.zero; // 위치를 손에 맞춤
            heldObject.transform.localRotation = Quaternion.identity; // 회전을 기본값으로 설정

            // Rigidbody가 있으면 물리적 움직임을 멈추고 손에 고정
            Rigidbody objRb = heldObject.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = true; // 물리 엔진 비활성화
                // objRb.velocity = Vector3.zero; // isKinematic=true면 필요 없을 수 있음
                // objRb.angularVelocity = Vector3.zero;
            }

            // 충돌 처리를 조정하여 플레이어와 오브젝트가 겹치지 않도록 설정
            Collider objCollider = heldObject.GetComponent<Collider>();
            Collider playerCollider = GetComponent<Collider>(); // PlayerController가 아닌 이 스크립트가 붙은 오브젝트의 콜라이더
            if (objCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(objCollider, playerCollider, true); // 플레이어와 충돌 무시
                // objCollider.enabled = false; // 콜라이더를 비활성화하면 다른 것과도 충돌 안 함. IgnoreCollision만으로 충분할 수 있음.
            } else {
                // Debug.LogWarning("오브젝트 또는 플레이어 콜라이더가 없습니다.");
            }

            isHoldingTrash = true; // 쓰레기 들고 있음 플래그 활성화
            // isBoxUnfolded = false; // CS0414: 제거됨 (오브젝트 태그로 판단)
            Debug.Log("오브젝트 주움: " + heldObject.name);

             // 작업대에 있던 것을 주웠다면 작업대 상태 갱신
             if (heldObject == boxOnWorkbench) boxOnWorkbench = null;
             if (heldObject == unfoldedBoxOnWorkbench) unfoldedBoxOnWorkbench = null;
        } else if (rightHand == null) {
             Debug.LogError("RightHand Transform이 설정되지 않아 오브젝트를 들 수 없습니다.");
        }
    }

    // 박스를 작업대 위에 올리는 함수
    private void PlaceBoxOnWorkbench()
    {
        if (heldObject != null && workbench != null && heldObject.CompareTag("Box")) // 들고 있는 'Box' 오브젝트와 작업대가 존재할 때
        {
            // 작업대 위쪽 위치 계산 (WorkBench 스크립트의 로직과 유사하게)
            // Vector3 workbenchTop = workbench.transform.position + Vector3.up * (workbench.transform.localScale.y * 0.5f + heldObject.transform.localScale.y * 0.5f + 0.1f); // 좀 더 정확한 계산
             Vector3 workbenchTop = workbench.transform.position + Vector3.up * 1.0f; // 단순화 (높이 조절 필요)

            heldObject.transform.SetParent(null); // 손에서 분리
            heldObject.transform.position = workbenchTop; // 작업대 위로 이동
            heldObject.transform.rotation = Quaternion.identity; // 회전 초기화

            // Rigidbody 설정 복구
            Rigidbody objRb = heldObject.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = false; // 물리 엔진 활성화
                 objRb.velocity = Vector3.zero; // 놓을 때 속도 초기화
                 objRb.angularVelocity = Vector3.zero;
            }
            Collider objCollider = heldObject.GetComponent<Collider>();
            Collider playerCollider = GetComponent<Collider>();
            if (objCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(objCollider, playerCollider, false); // 충돌 다시 활성화
                // objCollider.enabled = true; // 비활성화 했다면 다시 활성화
            }

            boxOnWorkbench = heldObject; // 작업대 위 박스로 설정
            isHoldingTrash = false; // 손에서 놓음
            heldObject = null; // 들고 있는 오브젝트 초기화
            Debug.Log("박스를 작업대에 올림 - Q 키를 2초 동안 눌러 펼칠 수 있음");
        }
    }

    // 박스를 작업대에서 펼치는 코루틴 (애니메이션 포함)
    private IEnumerator UnfoldBoxOnWorkbench()
    {
        if (boxOnWorkbench == null || unfoldedBoxPrefab == null || workbench == null || isUnfolding)
        {
            // 필수 조건 중 하나라도 없거나 이미 실행 중이면 종료
            yield break; // 코루틴 즉시 종료
        }

        isUnfolding = true; // 펼치는 중 플래그 활성화
        GameObject originalBox = boxOnWorkbench; // 원래 박스 참조 저장
        boxOnWorkbench = null; // 작업대 위 일반 박스 참조 제거 (중요)

        // 기존 박스의 상태 저장 및 비활성화 또는 제거 준비
        Vector3 startPosition = originalBox.transform.position;
        Quaternion startRotation = originalBox.transform.rotation;
        Vector3 startScale = originalBox.transform.localScale;
        // originalBox.SetActive(false); // 즉시 숨기기

        // 펼쳐진 박스 프리팹에서 목표 크기 가져오기 (프리팹 자체 스케일 사용)
        Vector3 targetScale = unfoldedBoxPrefab.transform.localScale;


        // 새로운 펼쳐진 박스 생성 (Instantiate 사용 예시, 필요시 PoolManager 사용)
        GameObject newUnfoldedBox = Instantiate(unfoldedBoxPrefab, startPosition, startRotation);
        newUnfoldedBox.tag = "UnfoldedBox"; // 태그 확인
        newUnfoldedBox.transform.localScale = startScale; // 초기 크기는 원래 박스와 같게 시작

        // 크기 변화 애니메이션 (Lerp 사용)
        float elapsed = 0f;
        while (elapsed < UNFOLD_DURATION)
        {
            elapsed += Time.deltaTime; // 경과 시간 누적
            float t = Mathf.Clamp01(elapsed / UNFOLD_DURATION); // 진행률 (0~1), Clamp01로 범위 보장
            newUnfoldedBox.transform.localScale = Vector3.Lerp(startScale, targetScale, t); // 크기 보간
            yield return null; // 다음 프레임까지 대기
        }
        newUnfoldedBox.transform.localScale = targetScale; // 최종 크기 확실히 적용

        // 기존 박스 제거 (애니메이션 후)
        Destroy(originalBox); // Instantiate 사용 시 Destroy, PoolManager 사용 시 ReturnToPool

        // 상태 업데이트
        unfoldedBoxOnWorkbench = newUnfoldedBox; // 펼쳐진 박스를 작업대 위 오브젝트로 설정
        // boxOnWorkbench = null; // 이미 위에서 null 처리됨
        // isHoldingTrash = false; // 손에 든 게 아니므로 변경 없음
        // heldObject = null; // 손에 든 게 아니므로 변경 없음
        // isBoxUnfolded = true; // CS0414: 제거됨
        qKeyHoldTime = 0f; // Q 키 시간 초기화
        Debug.Log("박스 펼침 완료 - E 키로 주울 수 있음");

        isUnfolding = false; // 펼침 완료
    }


    // 펼쳐진 박스를 손에 드는 함수
    private void PickUpUnfoldedBox()
    {
        if (unfoldedBoxOnWorkbench != null && !isHoldingTrash && rightHand != null) // 작업대에 펼쳐진 박스가 있고, 손이 비어 있으며, 오른손 참조가 있을 때
        {
            heldObject = unfoldedBoxOnWorkbench; // 들 오브젝트로 설정
            unfoldedBoxOnWorkbench = null; // 작업대에서 제거 (중요)

            PickUpObject(); // 공통 줍기 로직 사용 (PickUpObject 내부에서 isHoldingTrash=true 처리됨)

            // isBoxUnfolded = true; // CS0414: 제거됨 (태그로 판단)
            Debug.Log("펼쳐진 박스 주움: " + heldObject.name);
        } else if (rightHand == null) {
            Debug.LogError("RightHand Transform이 설정되지 않아 오브젝트를 들 수 없습니다.");
        }
    }

    // 분리수거장에서 쓰레기를 제거하는 함수
    private void ReturnTrashToPool()
    {
        if (isHoldingTrash && heldObject != null) // 쓰레기를 들고 있을 때
        {
             bool removed = false;
             string tag = heldObject.tag; // 미리 태그 저장

            // Obstacle 컴포넌트 확인 (선택적, PoolManager 사용 시 필요)
            Obstacle obstacle = heldObject.GetComponent<Obstacle>();

            if (tag == "UnfoldedBox" || tag == "Can") // 펼쳐진 박스 또는 캔이면
            {
                Debug.Log(tag + " 분리수거 시도");
                 if (obstacle != null) {
                     // obstacle.CleanObstacle(); // PoolManager 사용할 경우 이 함수 호출
                     PoolManager.Instance?.ReturnToPool(heldObject.name.Replace("(Clone)","").Trim(), obstacle); // PoolManager 사용 예시
                     // Destroy(heldObject); // PoolManager 안쓰면 직접 제거
                 } else {
                     Destroy(heldObject); // Obstacle 없으면 직접 제거
                 }
                removed = true;
                // isBoxUnfolded = false; // CS0414: 제거됨
            }
            else if (tag == "Box") // 일반 박스면
            {
                Debug.Log("펼쳐지지 않은 박스는 분리수거 불가");
                // 제거하지 않음
            } else {
                 Debug.LogWarning($"알 수 없는 태그({tag})의 오브젝트 분리수거 시도");
                 // 필요시 제거 로직 추가
            }

            if (removed)
            {
                 heldObject = null; // 들고 있는 오브젝트 초기화
                 isHoldingTrash = false; // 쓰레기 들고 있음 플래그 비활성화
                 Debug.Log(tag + " 분리수거 완료");
            }
        }
    }

    // 외부에서 쓰레기를 들고 있는지 확인할 수 있는 함수 (필요하다면 public 유지)
    public bool IsHoldingTrash()
    {
        return isHoldingTrash;
    }
}