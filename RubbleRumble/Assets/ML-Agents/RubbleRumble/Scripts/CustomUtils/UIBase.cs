using UnityEngine;

public class UIBase : MonoBehaviour
{
    public Canvas canvas;   // UIBase 상속하는 클래스가 기본적으로 캔버스 갖도록 하기

    public void Hide()
    {
        UIManager.Instance.Hide(gameObject.name);
    }
}