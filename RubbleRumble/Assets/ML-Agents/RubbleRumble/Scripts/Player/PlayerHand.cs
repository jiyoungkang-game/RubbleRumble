using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public Transform rightHand; // 쓰레기나 도구를 붙일 플레이어의 오른손 위치 (Transform)

    // 오브젝트가 처음 생성될 때 호출되는 초기화 함수
    void Awake()
    {
        // 플레이어의 Animator에서 오른손 뼈(Bone)의 Transform을 가져옴
        rightHand = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        // 오른손 위치를 손바닥 방향으로 약간 조정 (0.15 유닛 이동)
        rightHand.position = rightHand.position + rightHand.forward * 0.15f;
    }

    public void PickUpTrash(GameObject trash)
    {
        // 오브젝트를 오른손의 자식으로 설정
        trash.transform.SetParent(rightHand);
        trash.transform.localPosition = Vector3.zero; // 위치를 손에 맞춤
        trash.transform.localRotation = Quaternion.identity; // 회전을 기본값으로 설정

        // Rigidbody가 있으면 물리적 움직임을 멈추고 손에 고정
        Rigidbody trashRb = trash.GetComponent<Rigidbody>();
        if (trashRb != null)
        {
            trashRb.isKinematic = true; // 물리 엔진 비활성화
            trashRb.velocity = Vector3.zero; // 속도 초기화
            trashRb.angularVelocity = Vector3.zero; // 회전 속도 초기화
        }

        // 충돌 처리를 조정하여 플레이어와 오브젝트가 겹치지 않도록 설정
        Collider trashCollider = trash.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();

        if (trashCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(trashCollider, playerCollider, true); // 플레이어와 충돌 무시
            trashCollider.enabled = false; // 오브젝트의 충돌체 비활성화
        }
    }

    public void PlaceTrashOnWorkbench(WorkBench workbench, GameObject trash)
    {
        if (trash != null) // 들고 있는 오브젝트와 작업대가 존재할 때
        {
            // 작업대 위쪽 위치 계산 (작업대 높이 + 0.2f 오프셋)
            // Vector3 workbenchTop = transform.position + Vector3.up * (transform.localScale.y + 0.2f);
            //Vector3 workbenchTop = new Vector3(30, 2.5f, -21.6f);
            Vector3 workbenchTop = workbench.transform.position;
            trash.transform.SetParent(null); // 손에서 분리
            trash.transform.position = workbenchTop; // 작업대 위로 이동
            trash.transform.rotation = Quaternion.identity; // 회전 초기화

            // Rigidbody 설정 복구
            Rigidbody objRb = trash.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = false; // 물리 엔진 활성화
                objRb.velocity = Vector3.zero; // 속도 초기화
            }
            Collider objCollider = trash.GetComponent<Collider>();
            Collider playerCollider = GetComponent<Collider>();
            if (objCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(objCollider, playerCollider, false); // 충돌 다시 활성화
                objCollider.enabled = true; // 충돌체 활성화
            }
        }
    }
}
