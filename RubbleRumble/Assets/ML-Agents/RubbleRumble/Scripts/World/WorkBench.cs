using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.UIElements; // ������ �����Ƿ� �ּ� ó�� �Ǵ� ����

public class WorkBench : MonoBehaviour
{
    // �� �����յ��� RecycleAction���� ���� �������� �ʰ�, �̸�("UnfoldedBox")���� PoolManager���� �����ɴϴ�.
    // public GameObject trashAfterRecycling; // ������ �ڽ� ������ ���� (�ʿ�� ����)
    // public GameObject trashBeforeRecycling; // ���� �ڽ� ������ ���� (�ʿ�� ����)

    // �۾��� ���� �ִ� ������Ʈ ���� (PlayerController���� ���� Ȯ���ϹǷ� ���⼭�� ���ʿ��� �� ����)
    // private GameObject boxOnWorkbench;
    // private GameObject unfoldedBoxOnWorkbench;

    private const float UNFOLD_DURATION = 2f;

    public bool IsRecycling { get; private set; }   // ��Ȱ�� �ڷ�ƾ�� ���������� ����

    /// <summary>
    /// �۾��� ������ �ڽ��� ��ġ�� �ڷ�ƾ
    /// </summary>
    /// <param name="trash">��ĥ �ڽ� ������Ʈ (�ݵ�� "Box" �±׿��� ��)</param>
    public IEnumerator RecycleAction(GameObject trash)
    {
        // �Է� ��ȿ�� �˻�
        if (trash == null || !trash.CompareTag("Box") || IsRecycling)
        {
            Debug.LogWarning("RecycleAction ȣ�� ���� ������ �Ǵ� �̹� ���� ��");
            yield break; // ���� �� ������ �ڷ�ƾ ����
        }

        // ��Ȱ�� ����
        IsRecycling = true;
        Debug.Log("WorkBench: RecycleAction ���� - " + trash.name);

        // ���� �ڽ��� ���� ����
        Vector3 startPosition = trash.transform.position;
        Quaternion startRotation = trash.transform.rotation;
        Vector3 startScale = trash.transform.localScale;

        // Obstacle ������Ʈ �� �÷��̾� ���� ���� Ȯ��
        Obstacle obstacle = trash.GetComponent<Obstacle>();
        bool isPlayer = false;
        if (obstacle != null)
        {
            isPlayer = obstacle.IsPlayer;
             // �������� �Ŵ��� ī��Ʈ ������Ʈ (Null üũ �߰�)
             if (StageManager.Instance != null)
             {
                if (isPlayer) StageManager.Instance.PlayerObstacleCnt++;
                else StageManager.Instance.AIObstacleCnt++;
             } else {
                 Debug.LogWarning("StageManager �ν��Ͻ��� ã�� �� �����ϴ�.");
             }

            // ���� �ڽ� ���� (PoolManager ���)
            obstacle.RemoveObstacle(); // ���ο��� ReturnToPool ȣ�� ����
             Debug.Log("WorkBench: ���� �ڽ� ����/��ȯ - " + trash.name);
        } else {
            Debug.LogWarning(trash.name + "�� Obstacle ������Ʈ�� �����ϴ�. Destroy�� ��ü�մϴ�.");
            Destroy(trash); // ����: ���� ����
             Debug.Log("WorkBench: ���� �ڽ� Destroy - " + trash.name);
        }


        // ���ο� ������ �ڽ� ���� (PoolManager ���)
        // Vector3 targetScale = trashAfterRecycling.transform.localScale * 2f; // ������ ���� ���� ��� �̸� ���
         Vector3 targetScale = Vector3.one * 2.0f; // ����: �⺻ ũ���� 2�� (������ ũ�⿡ �°� ���� �ʿ�)
                                                // �Ǵ� PoolManager���� ������ ������Ʈ�� �⺻ ������ ���

        Obstacle newUnfoldedBox = null;
        if (PoolManager.Instance != null)
        {
            newUnfoldedBox = PoolManager.Instance.SpawnFromPool<Obstacle>("UnfoldedBox", startPosition, startRotation);
        }

        if (newUnfoldedBox != null)
        {
             Debug.Log("WorkBench: �� ������ �ڽ� ���� - " + newUnfoldedBox.name);
             newUnfoldedBox.IsPlayer = isPlayer; // ������ ���� ����
             // newUnfoldedBox.tag = "UnfoldedBox"; // PoolManager���� ������ �±� ���
             newUnfoldedBox.transform.localScale = startScale; // �ʱ� ũ�� ����

             // ����Ʈ �� ī��Ʈ ������Ʈ (Null üũ �߰�)
             MapManager.Instance?.AddToList(newUnfoldedBox);
             if (StageManager.Instance != null)
             {
                 if (isPlayer) StageManager.Instance.PlayerObstacleCnt--;
                 else StageManager.Instance.AIObstacleCnt--;
             }

             // ũ�� ��ȭ �ִϸ��̼� (Lerp)
             float elapsed = 0f;
             while (elapsed < UNFOLD_DURATION)
             {
                 elapsed += Time.deltaTime;
                 float t = Mathf.Clamp01(elapsed / UNFOLD_DURATION);
                 newUnfoldedBox.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                 yield return null;
             }
             newUnfoldedBox.transform.localScale = targetScale; // ���� ũ�� ����
             Debug.Log("WorkBench: ��ġ�� �ִϸ��̼� �Ϸ� - " + newUnfoldedBox.name);
        } else {
            Debug.LogError("PoolManager���� UnfoldedBox�� �������� ���߽��ϴ�.");
        }


        // ��Ȱ�� �Ϸ�
        IsRecycling = false;
         Debug.Log("WorkBench: RecycleAction �Ϸ�");
    }

    /// <summary>
    /// �۾��� ��ó(�ݰ� 2.0f)�� �ִ� ù ��° Box �Ǵ� UnfoldedBox ������Ʈ�� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>ã�� ������Ʈ �Ǵ� null</returns>
    public GameObject CheckOnWorkbench()
    {
        // Ž�� �ݰ� �� �߽� ����
        float checkRadius = 2.0f;
        Vector3 checkCenter = transform.position; // �۾��� ��ġ ����

        Collider[] colliders = Physics.OverlapSphere(checkCenter, checkRadius);
        GameObject foundBox = null;
        GameObject foundUnfoldedBox = null;

        foreach (var col in colliders)
        {
            // �ڱ� �ڽ��� ����
            if (col.gameObject == this.gameObject) continue;

            // Box �±� �켱 Ȯ��
            if (col.CompareTag("Box"))
            {
                // Rigidbody�� kinematic�� �ƴ��� Ȯ�� (�÷��̾ ��� ���� ������ ���� Ȯ��)
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb == null || !rb.isKinematic)
                {
                     foundBox = col.gameObject;
                     // Debug.Log("�۾��� �� �Ϲ� �ڽ� ����: " + foundBox.name);
                     break; // �Ϲ� �ڽ� ã���� �ٷ� ��ȯ (�켱����)
                }
            }
            // �Ϲ� �ڽ��� ���ٸ� ������ �ڽ� Ȯ��
            else if (col.CompareTag("UnfoldedBox") && foundBox == null)
            {
                 Rigidbody rb = col.GetComponent<Rigidbody>();
                 if (rb == null || !rb.isKinematic)
                 {
                    foundUnfoldedBox = col.gameObject;
                    // Debug.Log("�۾��� �� ������ �ڽ� ����: " + foundUnfoldedBox.name);
                    // ��� Ž���Ͽ� �Ϲ� �ڽ��� �ִ��� Ȯ��
                 }
            }
        }

         // �Ϲ� �ڽ��� ã������ �װ��� ��ȯ, ������ ������ �ڽ��� ��ȯ, �� �� ������ null ��ȯ
         if (foundBox != null) {
             // Debug.Log("CheckOnWorkbench ���: Box " + foundBox.name);
             return foundBox;
         } else if (foundUnfoldedBox != null) {
             // Debug.Log("CheckOnWorkbench ���: UnfoldedBox " + foundUnfoldedBox.name);
             return foundUnfoldedBox;
         } else {
             // Debug.Log("CheckOnWorkbench ���: ����");
             return null;
         }
    }

    // ������ Gizmo (�ʿ�� �ּ� ����)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // ���� ����
        Gizmos.DrawWireSphere(transform.position, 2.0f); // CheckOnWorkbench �ݰ� �ð�ȭ
    }
}