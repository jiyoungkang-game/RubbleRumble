using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPopup : UIBase
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI gameoverTxt;
    [SerializeField] private TextMeshProUGUI winnerLabel;
    [SerializeField] private TextMeshProUGUI loserLabel;
    [SerializeField] private TextMeshProUGUI winnerScoreTxt;
    [SerializeField] private TextMeshProUGUI loserScoreTxt;
    [SerializeField] private TextMeshProUGUI nextBtnTxt;

    [Header("Button")]
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button exitBtn;

    private void Awake()
    {
        if (StageManager.Instance.IsWin)
            SetWinInfo();
        else
            SetLoseInfo();
    }

    private void OnEnable()
    {
        if (StageManager.Instance.IsWin)
            // TODO: ���� ���������� �Ѿ�� �ϴ� ���� �ۼ�(���� �ӽ÷� Retry�� �����ϰ� ����)
            nextBtn.onClick.AddListener(OnNextBtnClicked);
        else
            nextBtn.onClick.AddListener(OnNextBtnClicked);

        // TODO: exitBtn ������ LobbyScene���� ������ �ۼ�
    }

    // �¸� �� ���� ���� �˾� ����
    private void SetWinInfo()
    {
        gameoverTxt.text = "VICTORY";
        winnerLabel.text = "YOUR SCORE:";
        loserLabel.text = "AI SCORE:";
        winnerScoreTxt.text = StageManager.Instance.PlayerScore.ToString();
        loserScoreTxt.text = StageManager.Instance.AIScore.ToString();
        nextBtnTxt.text = "NEXT";    
    }

    // �й� �� ���� ���� �˾� ����
    private void SetLoseInfo()
    {
        gameoverTxt.text = "DEFEAT";
        winnerLabel.text = "AI SCORE:";
        loserLabel.text = "YOUR SCORE:";
        winnerScoreTxt.text = StageManager.Instance.AIScore.ToString();
        loserScoreTxt.text = StageManager.Instance.PlayerScore.ToString();
        nextBtnTxt.text = "RETRY";
    }

    private void OnNextBtnClicked()
    {
        MapManager.Instance.ReturnAllObstacles();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
