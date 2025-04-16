using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBase<UIManager>
{
    // ���� ȭ��� ����, FHD, for �ػ� ����
    public float screenWidth = 1920;
    public float screenHeight = 1080;

    [SerializeField] private List<UIBase> uiList = new List<UIBase>();  // UI ��� �����ϴ� ����Ʈ

    protected override void Awake()
    {
        base.Awake();
    }

    // UI ��Ҹ� ���ҽ� �������� �������� ȭ�鿡 ǥ���ϴ� �޼���
    public T Show<T>() where T : UIBase
    {
        string uiName = typeof(T).ToString();   // UI ��� �̸��� T Ÿ������ �ޱ�
        UIBase go = Resources.Load<UIBase>("UI/" + uiName); // Resource �������� �������� ������ �ҷ�����
        /* �ݵ�� UI�� Script �̸��� Prefab �̸��� �����ؾ��� */
        if (go == null) // ��ο� �������� ������ �α׷� �˸��� null ��ȯ
        {
            Debug.Log($"UI Load Failed. {uiName} doesn't exist in Resources/UI/");
            return null;
        }
        var ui = Load<T>(go, uiName);
        uiList.Add(ui);
        return (T)ui;
    }

    // ĵ������ UI ��Ҹ� �����ϰ� UI�� ĵ������ ��ġ�ϵ��� �����ϴ� �޼���
    private T Load<T>(UIBase prefab, string uiName) where T : UIBase
    {
        GameObject newCanvasObject = new GameObject(uiName + "Canvas"); // ĵ������ ���� �� ���ӿ�����Ʈ ����

        var canvas = newCanvasObject.AddComponent<Canvas>();    // ĵ���� ������Ʈ �߰�
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;  // ĵ���� ���� ��� ����

        // ĵ���� ũ�� �����ϴ� ������Ʈ �߰� �� ũ�� ����
        var canvasScaler = newCanvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(screenWidth, screenHeight);
        
        newCanvasObject.AddComponent<GraphicRaycaster>();   // ��ȣ�ۿ��� ���� ������Ʈ �߰�

        UIBase ui = Instantiate(prefab, newCanvasObject.transform); // UI ��Ҹ� ĵ������ �ڽ����� ����
        ui.name = ui.name.Replace("(Clone)", "");   // �̸����� (Clone) ����
        ui.canvas = canvas; // ���� ������ UI�� ĵ������ ������ ���� ĵ������ ����
        ui.canvas.sortingOrder = uiList.Count;  // �ֱٿ� ������ UI�� �ֻ�ܿ� ���̵��� ����

        return (T)ui;
    }

    public void Hide<T>() where T : UIBase
    {
        string uiName = typeof(T).ToString();
        Hide(uiName);
    }

    public void Hide(string uiName)
    {
        UIBase go = uiList.Find(obj => obj.name == uiName); // UI �̸��� Ȱ��ȭ�� UI ����Ʈ�� �ִ��� Ž��
        uiList.Remove(go);
        Destroy(go.canvas.gameObject);
    }
}