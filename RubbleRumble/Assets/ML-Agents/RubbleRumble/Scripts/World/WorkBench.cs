using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UIElements; // 사용되지 않으므로 주석 처리 또는 삭제

public class WorkBench : MonoBehaviour
{
    // 이 프리팹들은 RecycleAction에서 직접 참조되지 않고, 이름("UnfoldedBox")으로 PoolManager에서 가져옵니다.
    // public GameObject trashAfterRecycling; // 펼쳐진 박스 프리팹 참조 (필요시 유지)
    // public GameObject trashBeforeRecycling; // 원래 박스 프리팹 참조 (필요시 유지)

    // 작업대 위에 있는 오브젝트 참조 (PlayerController에서 직접 확인하므로 여기서는 불필요할 수 있음)
    // private GameObject boxOnWorkbench;
    // private GameObject unfoldedBoxOnWorkbench;

    private const float UNFOLD_DURATION = 2f;

    public bool IsRecycling { get; private set; }   // 재활용 코루틴이 실행중인지 여부

    /// <summary>
    /// 작업대 위에서 박스를 펼치는 코루틴
    /// </summary>
    /// <param name="trash">펼칠 박스 오브젝트 (반드시 "Box" 태그여야 함)</param>
    public IEnumerator RecycleAction(GameObject trash)
    {
        // 입력 유효성 검사
        if (trash == null || !trash.CompareTag("Box") || IsRecycling)
        {
            Debug.LogWarning("RecycleAction 호출 조건 미충족 또는 이미 실행 중");
            yield break; // 조건 안 맞으면 코루틴 종료
        }

        // 재활용 시작
        IsRecycling = true;
        Debug.Log("WorkBench: RecycleAction 시작 - " + trash.name);

        // 기존 박스의 정보 저장
        Vector3 startPosition = trash.transform.position;
        Quaternion startRotation = trash.transform.rotation;
        Vector3 startScale = trash.transform.localScale;

        // Obstacle 컴포넌트 및 플레이어 소유 여부 확인
        Obstacle obstacle = trash.GetComponent<Obstacle>();
        bool isPlayer = false;
        if (obstacle != null)
        {
            isPlayer = obstacle.IsPlayer;
             // 스테이지 매니저 카운트 업데이트 (Null 체크 추가)
             if (StageManager.Instance != null)
             {
                if (isPlayer) StageManager.Instance.PlayerObstacleCnt++;
                else StageManager.Instance.AIObstacleCnt++;
             } else {
                 Debug.LogWarning("StageManager 인스턴스를 찾을 수 없습니다.");
             }

            // 기존 박스 제거 (PoolManager 사용)
            obstacle.RemoveObstacle(); // 내부에서 ReturnToPool 호출 가정
             Debug.Log("WorkBench: 기존 박스 제거/반환 - " + trash.name);
        } else {
            Debug.LogWarning(trash.name + "에 Obstacle 컴포넌트가 없습니다. Destroy로 대체합니다.");
            Destroy(trash); // 폴백: 직접 제거
             Debug.Log("WorkBench: 기존 박스 Destroy - " + trash.name);
        }


        // 새로운 펼쳐진 박스 생성 (PoolManager 사용)
        // Vector3 targetScale = trashAfterRecycling.transform.localScale * 2f; // 프리팹 직접 참조 대신 이름 사용
         Vector3 targetScale = Vector3.one * 2.0f; // 예시: 기본 크기의 2배 (프리팹 크기에 맞게 조정 필요)
                                                // 또는 PoolManager에서 가져온 오브젝트의 기본 스케일 사용

        Obstacle newUnfoldedBox = null;
        if (PoolManager.Instance != null)
        {
            newUnfoldedBox = PoolManager.Instance.SpawnFromPool<Obstacle>("UnfoldedBox", startPosition, startRotation);
        }

        if (newUnfoldedBox != null)
        {
             Debug.Log("WorkBench: 새 펼쳐진 박스 생성 - " + newUnfoldedBox.name);
             newUnfoldedBox.IsPlayer = isPlayer; // 소유자 정보 설정
             // newUnfoldedBox.tag = "UnfoldedBox"; // PoolManager에서 설정된 태그 사용
             newUnfoldedBox.transform.localScale = startScale; // 초기 크기 설정

             // 리스트 및 카운트 업데이트 (Null 체크 추가)
             MapManager.Instance?.AddToList(newUnfoldedBox);
             if (StageManager.Instance != null)
             {
                 if (isPlayer) StageManager.Instance.PlayerObstacleCnt--;
                 else StageManager.Instance.AIObstacleCnt--;
             }

             // 크기 변화 애니메이션 (Lerp)
             float elapsed = 0f;
             while (elapsed < UNFOLD_DURATION)
             {
                 elapsed += Time.deltaTime;
                 float t = Mathf.Clamp01(elapsed / UNFOLD_DURATION);
                 newUnfoldedBox.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                 yield return null;
             }
             newUnfoldedBox.transform.localScale = targetScale; // 최종 크기 적용
             Debug.Log("WorkBench: 펼치기 애니메이션 완료 - " + newUnfoldedBox.name);
        } else {
            Debug.LogError("PoolManager에서 UnfoldedBox를 스폰하지 못했습니다.");
        }


        // 재활용 완료
        IsRecycling = false;
         Debug.Log("WorkBench: RecycleAction 완료");
    }

    /// <summary>
    /// 작업대 근처(반경 2.0f)에 있는 첫 번째 Box 또는 UnfoldedBox 오브젝트를 반환합니다.
    /// </summary>
    /// <returns>찾은 오브젝트 또는 null</returns>
    public GameObject CheckOnWorkbench()
    {
        // 탐지 반경 및 중심 설정
        float checkRadius = 2.0f;
        Vector3 checkCenter = transform.position; // 작업대 위치 기준

        Collider[] colliders = Physics.OverlapSphere(checkCenter, checkRadius);
        GameObject foundBox = null;
        GameObject foundUnfoldedBox = null;

        foreach (var col in colliders)
        {
            // 자기 자신은 제외
            if (col.gameObject == this.gameObject) continue;

            // Box 태그 우선 확인
            if (col.CompareTag("Box"))
            {
                // Rigidbody가 kinematic이 아닌지 확인 (플레이어가 들고 있지 않은지 간접 확인)
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb == null || !rb.isKinematic)
                {
                     foundBox = col.gameObject;
                     // Debug.Log("작업대 위 일반 박스 감지: " + foundBox.name);
                     break; // 일반 박스 찾으면 바로 반환 (우선순위)
                }
            }
            // 일반 박스가 없다면 펼쳐진 박스 확인
            else if (col.CompareTag("UnfoldedBox") && foundBox == null)
            {
                 Rigidbody rb = col.GetComponent<Rigidbody>();
                 if (rb == null || !rb.isKinematic)
                 {
                    foundUnfoldedBox = col.gameObject;
                    // Debug.Log("작업대 위 펼쳐진 박스 감지: " + foundUnfoldedBox.name);
                    // 계속 탐색하여 일반 박스가 있는지 확인
                 }
            }
        }

         // 일반 박스를 찾았으면 그것을 반환, 없으면 펼쳐진 박스를 반환, 둘 다 없으면 null 반환
         if (foundBox != null) {
             // Debug.Log("CheckOnWorkbench 결과: Box " + foundBox.name);
             return foundBox;
         } else if (foundUnfoldedBox != null) {
             // Debug.Log("CheckOnWorkbench 결과: UnfoldedBox " + foundUnfoldedBox.name);
             return foundUnfoldedBox;
         } else {
             // Debug.Log("CheckOnWorkbench 결과: 없음");
             return null;
         }
    }

    // 디버깅용 Gizmo (필요시 주석 해제)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // 색상 변경
        Gizmos.DrawWireSphere(transform.position, 2.0f); // CheckOnWorkbench 반경 시각화
    }
}