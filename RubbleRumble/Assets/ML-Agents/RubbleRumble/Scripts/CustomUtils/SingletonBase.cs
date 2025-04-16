using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance    // �̱��� �ν��Ͻ��� �ܺο��� �ٷ� ���� ���
    {
        get
        {
            if (_instance != null)  // �̹� ������ �ν��Ͻ� ������
                return _instance;   // ���� �ν��Ͻ� ��ȯ

            // scene�� T Ÿ���� ������Ʈ�� ������ �ν��Ͻ��� ����
            _instance = FindObjectOfType<T>();

            if (_instance == null)  // scene�� T Ÿ�� ������Ʈ�� ������
            {
                // ���� T Ÿ���� ���ӿ�����Ʈ�� �����ϰ� ������Ʈ �߰�
                GameObject go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)  // �ν��Ͻ��� ������
        {
            _instance = this as T;  // ���� ������ �ν��Ͻ��� �̱��� �ν��Ͻ��� ����
        }
        else if (_instance != this) // ���� �ν��Ͻ��� ���� ������ �ν��Ͻ��� �ٸ���
        {
            Destroy(gameObject);    // ���� ������ �ν��Ͻ��� �ı�(���� �ν��Ͻ��� ����)
        }
    }
}