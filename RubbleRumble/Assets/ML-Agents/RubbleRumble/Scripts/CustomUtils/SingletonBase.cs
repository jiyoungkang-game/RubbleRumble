using UnityEngine;

public class SingletonBase<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance    // 싱글톤 인스턴스를 외부에서 바로 접근 허용
    {
        get
        {
            if (_instance != null)  // 이미 생성된 인스턴스 있으면
                return _instance;   // 기존 인스턴스 반환

            // scene에 T 타입의 오브젝트가 있으면 인스턴스로 설정
            _instance = FindObjectOfType<T>();

            if (_instance == null)  // scene에 T 타입 오브젝트가 없으면
            {
                // 새로 T 타입의 게임오브젝트를 생성하고 컴포넌트 추가
                GameObject go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)  // 인스턴스가 없으면
        {
            _instance = this as T;  // 현재 생성된 인스턴스를 싱글톤 인스턴스로 설정
        }
        else if (_instance != this) // 기존 인스턴스와 새로 생성된 인스턴스가 다르면
        {
            Destroy(gameObject);    // 새로 생성된 인스턴스를 파괴(기존 인스턴스를 유지)
        }
    }
}