using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerScoreTxt;
    [SerializeField] private TextMeshProUGUI aiScoreTxt;
    [SerializeField] private Image timerBarImg;

    private void Update()
    {
        // ���� UI ����
        playerScoreTxt.text = StageManager.Instance.PlayerScore.ToString();
        aiScoreTxt.text = StageManager.Instance.AIScore.ToString();

        timerBarImg.fillAmount = StageManager.Instance.TimeLeft / StageManager.Instance.TimeLimit; // ���� �ð� UI ����
    }
}
