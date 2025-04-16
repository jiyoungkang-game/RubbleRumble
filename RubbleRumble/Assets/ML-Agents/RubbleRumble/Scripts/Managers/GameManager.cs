using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);  // 씬 넘어가도 파괴되지 않고 유지
    }

    private void Start()
    {
        Application.targetFrameRate = 60;   // 고정 프레임 설정(60)
    }

    public void GameOver()
    {
        Time.timeScale = 0f;    // 시간 일시 정지
        StageManager.Instance.IsPlaying = false;
        UIManager.Instance.Show<GameOverPopup>();   // 게임 종료창 팝업
        // TODO: 플레이 정보 저장 등.. 종료 후 시행해야 할 로직 작성
    }
}
