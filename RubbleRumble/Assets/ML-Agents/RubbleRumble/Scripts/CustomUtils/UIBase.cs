using UnityEngine;

public class UIBase : MonoBehaviour
{
    public Canvas canvas;   // UIBase ����ϴ� Ŭ������ �⺻������ ĵ���� ������ �ϱ�

    public void Hide()
    {
        UIManager.Instance.Hide(gameObject.name);
    }
}