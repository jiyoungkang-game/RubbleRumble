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

    private Transform rightHandTransform; // 쓰레기나 도구를 붙일 플레이어의 오른손 위치 (Transform)

    public GameObject unfoldedBoxPrefab; // 펼쳐진 박스 프리팹 (Unity Inspector에서 설정 필요)
    private GameObject heldObject;
    // private GameObject workbench; // CS0414: 사용되지 않음 - 제거됨
    //private GameObject boxOnWorkbench; // 작업대 위에 올라가 있는 일반 박스
    //private GameObject unfoldedBoxOnWorkbench; // 작업대 위에 있는 펼쳐진 박스
    private GameObject trashOnWorkbench;

    private bool isHoldingTrash = false; // 플레이어가 쓰레기를 들고 있는지 여부를 추적
    // private bool isBoxUnfolded = false; // CS0414: 사용되지 않음 - 제거됨
    private bool isNearWorkbench = false; // 플레이어가 작업대 근처에 있는지 여부
    private bool isNearRecyclingBin = false; // 플레이어가 분리수거장 근처에 있는지 여부
    private bool isUnfolding = false; // 박스가 펼쳐지는 중인지 여부를 추적

    private const float UNFOLD_DURATION = 2f; // 박스를 펼치는 데 필요한 시간 (2초로 고정)
    private float qKeyHoldTime = 0f; // Q 키를 누르고 있는 시간을 측정 (초 단위)

    private Coroutine recycleCoroutine;

    private int interactUIState;

    void Awake()
    {
        // 플레이어의 Animator에서 오른손 뼈(Bone)의 Transform을 가져옴
        rightHandTransform = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        // 오른손 위치를 손바닥 방향으로 약간 조정 (0.15 유닛 이동)
        if (rightHandTransform != null) // Null 체크 추가
        {
            rightHandTransform.position = rightHandTransform.position + rightHandTransform.forward * 0.15f;
        } else {
             Debug.LogError("RightHand Transform을 찾을 수 없습니다. Animator와 HumanBodyBones 설정을 확인하세요.");
        }


        toolManager = GameObject.Find("Managers").GetComponent<ToolManager>();
        //workBench = GameObject.Find("Workbench").GetComponent<WorkBench>();
        workBench = FindFirstObjectByType<WorkBench>();
        playerHand = GameObject.Find("Player").GetComponent<PlayerHand>();
        playerInteract = GameObject.Find("Player").GetComponent<PlayerInteract>();

        if (workBench == null)
        {
            Debug.LogError("WorkBench 컴포넌트를 찾을 수 없습니다.");
        }
        if (playerHand == null)
        {
            Debug.LogError("PlayerHand 컴포넌트를 찾을 수 없습니다.");
        }
         if (playerInteract == null)
        {
            Debug.LogError("PlayerInteract 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        // 도구 선택 : 1=맨손, 2=빗자루, 3=대걸레
        if (Input.GetKeyDown(KeyCode.Alpha1)) toolManager.EquipTool(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) toolManager.EquipTool(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) toolManager.EquipTool(2);

        if (workBench != null) // Null 체크 추가
        {
             isUnfolding = workBench.IsRecycling;    // 재활용 중인지 상태 갱신
        }


        if (isNearWorkbench && !isUnfolding && workBench != null) // workBench Null 체크 추가
        {
            // 디버깅용: 현재 상태 출력 (쓰레기 들고 있는지, 작업대 위 박스 상태 등)
            // Debug.Log("작업대 근처 - isHoldingTrash: " + isHoldingTrash + ", heldObject: " + (heldObject != null ? heldObject.name : "null") + ", trashOnWorkbench: " + (trashOnWorkbench != null ? trashOnWorkbench.name : "null"));

            // 작업대 근처에서 펼쳐진 박스를 E 키로 줍기
            if (trashOnWorkbench != null && Input.GetKeyDown(KeyCode.E) && !isHoldingTrash)
            {
                Debug.Log("E 키 눌림 - 펼쳐진 박스 줍기");
                //playerHand.PickUpTrash(heldObject); // 펼쳐진 박스를 손에 드는 함수 호출
                playerHand.PickUpTrash(trashOnWorkbench); // 펼쳐진 박스를 손에 드는 함수 호출
                heldObject = trashOnWorkbench; // trashOnWorkbench의 gameObject가 아니라 trashOnWorkbench 자체를 할당해야 할 수 있습니다. PlayerHand 로직 확인 필요
                trashOnWorkbench = null; // 작업대에서 제거
                isHoldingTrash = true;
            }

            // Q 키를 처음 눌렀을 때: 박스를 작업대에 올리는 동작
            if (Input.GetKeyDown(KeyCode.Q) && isHoldingTrash && heldObject != null && heldObject.CompareTag("Box") && trashOnWorkbench == null)
            {
                Debug.Log("Q 키 눌림 - 박스를 작업대에 올림");
                playerHand.PlaceTrashOnWorkbench(workBench, heldObject); // 박스를 작업대 위에 올리는 함수 호출
                trashOnWorkbench = heldObject; // 작업대 위 오브젝트로 설정
                heldObject = null; // 손에서 제거
                isHoldingTrash = false;
            }

            // Q 키를 계속 누르고 있을 때: 작업대 위 박스를 펼치는 동작 준비
            if (Input.GetKey(KeyCode.Q) && trashOnWorkbench != null && trashOnWorkbench.CompareTag("Box")) // Box 태그일때만 펼치기 시도
            {
                qKeyHoldTime += Time.deltaTime; // Q 키 누른 시간 누적
                // Debug.Log("Q 키 누르는 중 - qKeyHoldTime: " + qKeyHoldTime);
                if (qKeyHoldTime >= UNFOLD_DURATION) // 2초 이상 누르면 박스 펼침 시작
                {
                    Debug.Log("Q 키 2초 이상 - 박스 펼침 시작");
                    if (recycleCoroutine == null) // 코루틴 중복 실행 방지
                    {
                        recycleCoroutine = StartCoroutine(workBench.RecycleAction(trashOnWorkbench)); // 박스 펼치는 코루틴 시작
                        trashOnWorkbench = null; // 작업대에서 원래 박스 참조 제거
                        qKeyHoldTime = 0f; // 홀드 시간 초기화 (코루틴 시작 시)
                    }
                }
            }

            // Q 키를 뗐을 때: 누른 시간 초기화 (코루틴이 시작되지 않았을 경우)
            if (Input.GetKeyUp(KeyCode.Q))
            {
                //Debug.Log("Q 키 뗌 - qKeyHoldTime 초기화");
                qKeyHoldTime = 0f;
                // 코루틴이 진행중이지 않을 때(2초 전에 뗐을 때)는 특별히 중지할 필요 없음
                // if (recycleCoroutine != null && !isUnfolding) // 코루틴이 실행되었지만 아직 IsRecycling이 true가 되기 전? -> 이 조건보다는 아래가 나음
                // {
                //     StopCoroutine(recycleCoroutine);
                //     recycleCoroutine = null;
                // }
            }
        } else if (!isNearWorkbench && recycleCoroutine != null) // 작업대 벗어나면 코루틴 중지
        {
             StopCoroutine(recycleCoroutine);
             recycleCoroutine = null;
             isUnfolding = false; // 확실히 하기 위해 상태 변경
             Debug.Log("작업대 벗어나서 펼치기 코루틴 중지");
        }


        // 분리수거장 근처에서 E 키로 쓰레기 제거
        if (isNearRecyclingBin && isHoldingTrash && heldObject != null && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E 키 눌림 - 분리수거장 쓰레기 제거");
            ReturnTrashToPool(); // 쓰레기를 분리수거장으로 보내는 함수 호출
        }

        ReturnCurrentInteract();    // 상호작용 UI 상태 갱신
    }

    // 트리거 영역 안에 머무르는 동안 호출되는 함수
    void OnTriggerStay(Collider other)
    {
        // 쓰레기를 들고 있지 않고, 맨손(인덱스 0)일 때만 오브젝트 줍기 가능
        if (!isHoldingTrash && toolManager.currentTool == 0 && (other.CompareTag("Can") || other.CompareTag("Box") || other.CompareTag("UnfoldedBox")))
        {
            // PlayerHand에서 이미 주울 수 있는 오브젝트인지 확인하는 로직이 있을 수 있으므로, 여기서는 감지만 하고 E키 입력은 Update에서 처리하는 것이 더 일반적일 수 있음
            // 만약 여기서 바로 줍는 로직을 유지한다면 heldObject 할당 필요
             if (Input.GetKey(KeyCode.E)) // E 키를 누르면 줍기 실행
            {
                 heldObject = other.gameObject; // E키 누르는 순간에만 할당
                 Debug.Log("E 키 눌림 - 오브젝트 줍기: " + heldObject.name);
                 playerHand.PickUpTrash(heldObject); // 오브젝트를 손에 드는 함수 호출
                 isHoldingTrash = true;
                 // 주운 후에는 트리거 내 다른 오브젝트와 상호작용 방지 위해 heldObject를 null로? -> PlayerHand에서 관리하는 것이 좋을 수 있음
                 // heldObject = null; // 주운 후 초기화 (선택적)
            } else {
                 // E키를 누르지 않은 상태에서는 잠재적 대상만 표시 (UI 용도 등)
                 // playerInteract.ShowInteractHint(other.gameObject); // 예시
            }
        }

        // 작업대 주변 영역에 들어왔을 때
        if (other.CompareTag("WorkbenchArea"))
        {
            if (!isNearWorkbench) // 처음 들어왔을 때만 로그 출력 및 상태 확인
            {
                isNearWorkbench = true; // 작업대 근처 플래그 활성화
                Debug.Log("작업대 근처 진입");
                 if (workBench != null) { // null 체크
                    trashOnWorkbench = workBench.CheckOnWorkbench(); // 작업대 위 오브젝트 확인
                 } else {
                    Debug.LogError("WorkBench 참조가 null입니다.");
                 }
            } else {
                 // 계속 머무르는 동안 작업대 위 상태 갱신 (필요 시)
                 // trashOnWorkbench = workBench?.CheckOnWorkbench(); // Optional chaining
            }
        }

        // 분리수거장 근처에 들어왔을 때
        if (other.CompareTag("RecyclingBin"))
        {
            if (!isNearRecyclingBin) // 처음 들어왔을 때만 로그 출력
            {
                 isNearRecyclingBin = true; // 분리수거장 근처 플래그 활성화
                 Debug.Log("분리수거장 근처 진입");
            }
        }
    }

    // 트리거 영역을 벗어날 때 호출되는 함수
    void OnTriggerExit(Collider other)
    {
        // 감지 대상 오브젝트가 트리거를 벗어나면 참조 초기화 (줍지 않은 경우)
        // if (other.gameObject == heldObject) // 이 조건은 이미 주운 경우에도 해당될 수 있어 부적절
        // {
        //     // heldObject = null; // 여기서 null 처리하면 들고 있는 상태도 해제될 수 있음
        //     // Debug.Log("트리거 벗어남 - heldObject 초기화?");
        // }

        // 작업대 주변 영역을 벗어나면 관련 변수 초기화
        if (other.CompareTag("WorkbenchArea"))
        {
            isNearWorkbench = false;
            // workbench = null; // CS0414: 제거됨
            trashOnWorkbench = null; // 작업대 벗어나면 참조 초기화
            qKeyHoldTime = 0f;
            Debug.Log("작업대 벗어남");
            if (recycleCoroutine != null) // 작업대 벗어날 때 코루틴 실행 중이면 중지
            {
                 Debug.Log("작업대 벗어나서 펼치기 코루틴 중지 (OnTriggerExit)");
                 StopCoroutine(recycleCoroutine);
                 recycleCoroutine = null;
                 isUnfolding = false; // 상태 확실히 변경
            }
        }

        // 분리수거장 영역을 벗어나면 플래그 초기화
        if (other.CompareTag("RecyclingBin"))
        {
            isNearRecyclingBin = false;
            Debug.Log("분리수거장 벗어남");
        }
    }

    // 분리수거장에서 쓰레기를 제거하는 함수
    private void ReturnTrashToPool()
    {
        if (isHoldingTrash && heldObject != null) // 쓰레기를 들고 있을 때
        {
            Obstacle obstacle = heldObject.GetComponent<Obstacle>();
            bool removed = false;

            if (heldObject.CompareTag("UnfoldedBox") || heldObject.CompareTag("Can")) // 펼쳐진 박스 또는 캔이면
            {
                if (obstacle != null)
                {
                    obstacle.CleanObstacle(); // Obstacle의 제거 로직 사용
                    Debug.Log(heldObject.tag + " 분리수거 완료");
                    removed = true;
                }
                else
                {
                    Debug.LogWarning(heldObject.name + "에 Obstacle 컴포넌트가 없습니다. Destroy로 대체합니다.");
                    Destroy(heldObject); // 폴백
                    removed = true;
                }
                // isBoxUnfolded = false; // CS0414: 제거됨
            }
            else if (heldObject.CompareTag("Box")) // 일반 박스면
            {
                Debug.Log("펼쳐지지 않은 박스는 분리수거 불가");
                // 제거하지 않고 종료
            }
            else
            {
                 Debug.LogWarning("알 수 없는 태그의 쓰레기: " + heldObject.tag);
                 // 필요하다면 여기서도 제거 로직 추가
            }


            if (removed)
            {
                 heldObject = null; // 들고 있는 오브젝트 초기화
                 isHoldingTrash = false; // 쓰레기 들고 있음 플래그 비활성화
            }
        }
    }

    // 안내할 상호작용 이벤트 판정하는 함수
    public int ReturnCurrentInteract()
    {
         int newState = 0; // 기본 상태: 비활성화

        if (isNearWorkbench)    // 작업장 근처에 있고
        {
            if (trashOnWorkbench != null)   // 작업대에 쓰레기가 있고
            {
                 if (trashOnWorkbench.CompareTag("Box")) // 접힌 박스
                 {
                     if (Input.GetKey(KeyCode.Q)) // Q 누르고 있으면
                     {
                         newState = 3;    // 홀딩바 활성화
                     } else {
                         newState = 2;    // 상호작용 Q 안내 (놓기/펼치기 시작)
                     }
                 }
                 else if (trashOnWorkbench.CompareTag("UnfoldedBox"))// 작업대 위에 펼친 상자가 있으면
                 {
                     newState = 1;    // 상호작용 E 안내 (줍기)
                 }
                 // 캔 등 다른 오브젝트가 작업대에 있을 경우 상태 0 유지
            }
            else if (isHoldingTrash && heldObject != null) // 작업대는 비었고, 들고 있는 물건이 있으며
            {
                 if (heldObject.CompareTag("Box"))    // 접힌 상자를 들고 있으면
                 {
                     newState = 2;    // 상호작용 Q 안내 (놓기)
                 }
                  // 다른 걸 들고 있을 땐 작업대에서 할 수 있는 상호작용 없음 (상태 0 유지)
            }
             // 작업대 근처이고 아무것도 안들고 있고 작업대도 비어있으면 상태 0 유지
        }
        else if (isNearRecyclingBin && isHoldingTrash && heldObject != null)   // 분리수거장 근처에 있고 쓰레기를 들고 있는 경우
        {
            if (heldObject.CompareTag("Box"))    // 접힌 박스를 들고 있으면 분리수거 불가
                newState = 0;    // 상호작용 UI 비활성화
            else if (heldObject.CompareTag("UnfoldedBox") || heldObject.CompareTag("Can")) // 펼쳐진 박스나 캔
                newState = 1;    // 상호작용 E 안내 (버리기)
        }
        else if (!isHoldingTrash && toolManager.currentTool == 0) // 맨손이고 아무것도 안 들고 있을 때
        {
           
             // Collider[] nearbyItems = Physics.OverlapSphere(transform.position, 1.5f, LayerMask.GetMask("Interactable")); // 예시
             // if (nearbyItems.Length > 0) {
             //     bool canPickUp = false;
             //     foreach(var itemCollider in nearbyItems) {
             //         if (itemCollider.CompareTag("Can") || itemCollider.CompareTag("Box") || itemCollider.CompareTag("UnfoldedBox")) {
             //              canPickUp = true;
             //              break;
             //         }
             //     }
             //     if(canPickUp) newState = 1; // 상호작용 E 안내 (줍기)
             // }
             // ---> 이 부분은 PlayerInteract 스크립트에서 처리하는 것이 더 적합할 수 있음.
             // ---> PlayerController는 상태 플래그만 제공하고, PlayerInteract가 최종 UI 상태 결정.
        }


        // 최종 상태 적용
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