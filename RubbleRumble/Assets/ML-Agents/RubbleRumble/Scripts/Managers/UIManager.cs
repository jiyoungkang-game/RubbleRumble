using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>
{
    // 고정 화면비 설정, FHD, for 해상도 대응
    public float screenWidth = 1920;
    public float screenHeight = 1080;

    [SerializeField] private List<UIBase> uiList = new List<UIBase>();  // UI 요소 관리하는 리스트

    protected override void Awake()
    {
        base.Awake();
    }

    // UI 요소를 리소스 폴더에서 가져오고 화면에 표시하는 메서드
    public T Show<T>() where T : UIBase
    {
        string uiName = typeof(T).ToString();   // UI 요소 이름을 T 타입으로 받기
        UIBase go = Resources.Load<UIBase>("UI/" + uiName); // Resource 폴더에서 동적으로 프리팹 불러오기
        /* 반드시 UI의 Script 이름과 Prefab 이름이 동일해야함 */
        if (go == null) // 경로에 존재하지 않으면 로그로 알리고 null 반환
        {
            Debug.Log($"UI Load Failed. {uiName} doesn't exist in Resources/UI/");
            return null;
        }
        var ui = Load<T>(go, uiName);
        uiList.Add(ui);
        return (T)ui;
    }

    // 캔버스와 UI 요소를 생성하고 UI를 캔버스에 위치하도록 설정하는 메서드
    private T Load<T>(UIBase prefab, string uiName) where T : UIBase
    {
        GameObject newCanvasObject = new GameObject(uiName + "Canvas"); // 캔버스를 넣을 빈 게임오브젝트 생성

        var canvas = newCanvasObject.AddComponent<Canvas>();    // 캔버스 컴포넌트 추가
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;  // 캔버스 랜더 모드 설정

        // 캔버스 크기 설정하는 컴포넌트 추가 후 크기 설정
        var canvasScaler = newCanvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(screenWidth, screenHeight);
        
        newCanvasObject.AddComponent<GraphicRaycaster>();   // 상호작용을 위한 컴포넌트 추가

        UIBase ui = Instantiate(prefab, newCanvasObject.transform); // UI 요소를 캔버스의 자식으로 생성
        ui.name = ui.name.Replace("(Clone)", "");   // 이름에서 (Clone) 삭제
        ui.canvas = canvas; // 새로 생성된 UI의 캔버스를 기존에 만든 캔버스로 설정
        ui.canvas.sortingOrder = uiList.Count;  // 최근에 생성된 UI가 최상단에 보이도록 설정

        return (T)ui;
    }

    public void Hide<T>() where T : UIBase
    {
        string uiName = typeof(T).ToString();
        Hide(uiName);
    }

    public void Hide(string uiName)
    {
        UIBase go = uiList.Find(obj => obj.name == uiName); // UI 이름이 활성화된 UI 리스트에 있는지 탐색
        uiList.Remove(go);
        Destroy(go.canvas.gameObject);
    }
}