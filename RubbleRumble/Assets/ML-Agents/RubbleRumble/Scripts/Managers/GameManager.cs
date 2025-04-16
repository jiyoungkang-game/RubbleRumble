using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);  // �� �Ѿ�� �ı����� �ʰ� ����
    }

    private void Start()
    {
        Application.targetFrameRate = 60;   // ���� ������ ����(60)
    }

    public void GameOver()
    {
        Time.timeScale = 0f;    // �ð� �Ͻ� ����
        StageManager.Instance.IsPlaying = false;
        UIManager.Instance.Show<GameOverPopup>();   // ���� ����â �˾�
        // TODO: �÷��� ���� ���� ��.. ���� �� �����ؾ� �� ���� �ۼ�
    }
}
