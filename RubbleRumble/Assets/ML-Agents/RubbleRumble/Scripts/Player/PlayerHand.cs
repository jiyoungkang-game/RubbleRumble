using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    public Transform rightHand; // �����⳪ ������ ���� �÷��̾��� ������ ��ġ (Transform)

    // ������Ʈ�� ó�� ������ �� ȣ��Ǵ� �ʱ�ȭ �Լ�
    void Awake()
    {
        // �÷��̾��� Animator���� ������ ��(Bone)�� Transform�� ������
        rightHand = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        // ������ ��ġ�� �չٴ� �������� �ణ ���� (0.15 ���� �̵�)
        rightHand.position = rightHand.position + rightHand.forward * 0.15f;
    }

    public void PickUpTrash(GameObject trash)
    {
        // ������Ʈ�� �������� �ڽ����� ����
        trash.transform.SetParent(rightHand);
        trash.transform.localPosition = Vector3.zero; // ��ġ�� �տ� ����
        trash.transform.localRotation = Quaternion.identity; // ȸ���� �⺻������ ����

        // Rigidbody�� ������ ������ �������� ���߰� �տ� ����
        Rigidbody trashRb = trash.GetComponent<Rigidbody>();
        if (trashRb != null)
        {
            trashRb.isKinematic = true; // ���� ���� ��Ȱ��ȭ
            trashRb.velocity = Vector3.zero; // �ӵ� �ʱ�ȭ
            trashRb.angularVelocity = Vector3.zero; // ȸ�� �ӵ� �ʱ�ȭ
        }

        // �浹 ó���� �����Ͽ� �÷��̾�� ������Ʈ�� ��ġ�� �ʵ��� ����
        Collider trashCollider = trash.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();

        if (trashCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(trashCollider, playerCollider, true); // �÷��̾�� �浹 ����
            trashCollider.enabled = false; // ������Ʈ�� �浹ü ��Ȱ��ȭ
        }
    }

    public void PlaceTrashOnWorkbench(WorkBench workbench, GameObject trash)
    {
        if (trash != null) // ��� �ִ� ������Ʈ�� �۾��밡 ������ ��
        {
            // �۾��� ���� ��ġ ��� (�۾��� ���� + 0.2f ������)
            // Vector3 workbenchTop = transform.position + Vector3.up * (transform.localScale.y + 0.2f);
            //Vector3 workbenchTop = new Vector3(30, 2.5f, -21.6f);
            Vector3 workbenchTop = workbench.transform.position;
            trash.transform.SetParent(null); // �տ��� �и�
            trash.transform.position = workbenchTop; // �۾��� ���� �̵�
            trash.transform.rotation = Quaternion.identity; // ȸ�� �ʱ�ȭ

            // Rigidbody ���� ����
            Rigidbody objRb = trash.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.isKinematic = false; // ���� ���� Ȱ��ȭ
                objRb.velocity = Vector3.zero; // �ӵ� �ʱ�ȭ
            }
            Collider objCollider = trash.GetComponent<Collider>();
            Collider playerCollider = GetComponent<Collider>();
            if (objCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(objCollider, playerCollider, false); // �浹 �ٽ� Ȱ��ȭ
                objCollider.enabled = true; // �浹ü Ȱ��ȭ
            }
        }
    }
}
