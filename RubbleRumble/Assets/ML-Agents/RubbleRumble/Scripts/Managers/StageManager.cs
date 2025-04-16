using System.Collections.Generic;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    [SerializeField] private float timeLimit;    // ���� �ð�
    public int PlayerObstacleCnt;    // �÷��̾� ���� �� ������ ��
    public int AIObstacleCnt;        // AI ���� �� ������ ��
    public bool IsPlaying; // �÷��� ������ Ȯ���ϴ� �÷���

    public int PlayerScore { get; private set; }    // �÷��̾� ���� ����
    public int AIScore { get; private set; }    // AI ���� ����
    public float TimeLimit { get { return timeLimit;  } } // ���� �ð�
    public float TimeLeft { get; private set; } // �ܿ� �ð�
    public bool IsWin { get; private set; } // ���� ���

    protected override void Awake()
    {
        base.Awake();

        // ���� �� ���� �ð� �ʱ�ȭ
        PlayerScore = 0;
        AIScore = 0;
        timeLimit = 120f;
        TimeLeft = TimeLimit;

        /* TestCode */
        //timeLimit = 10f;
        //TimeLeft = TimeLimit;
    }

    private void Start()
    {
        Time.timeScale = 1.0f;
        IsPlaying = true;
    }

    private void Update()
    {
        if (!IsPlaying) return; // �÷��� ���� ���� ����

        TimeLeft -= Time.deltaTime;    // ���� �ð� ����

        if (TimeLeft <= 0)  // ���� �ð� ���� ��
        {
            if (PlayerScore >= AIScore) // �÷��̾� ������ �� ������ �¸�
                IsWin = true;
            else    // AI ������ �� ������ �й�
                IsWin = false;
            GameManager.Instance.GameOver();
        }

        // �÷��̾� ���� �� �����Ⱑ ������ �¸�
        if (PlayerObstacleCnt <= 0)    
        {
            IsWin = true;
            GameManager.Instance.GameOver();
        }

        // AI ���� �� �����Ⱑ ������ �й�
        if (AIObstacleCnt <= 0)
        {
            IsWin = false;
            GameManager.Instance.GameOver();
        }

        #region TestCode
        {
            if (Input.GetKeyDown(KeyCode.O))  // O�� ������ AI ���� ����
            {
                for (int i = 0; i < MapManager.Instance.aiObstacleList.Count; i++)
                {
                    if (MapManager.Instance.aiObstacleList[i].gameObject.activeSelf)
                    {
                        MapManager.Instance.aiObstacleList[i].CleanObstacle();
                        break;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.P))  // P�� ������ Player ���� ����
            {
                for (int i = 0; i < MapManager.Instance.playerObstacleList.Count; i++)
                {
                    if (MapManager.Instance.playerObstacleList[i].gameObject.activeSelf)
                    {
                        MapManager.Instance.playerObstacleList[i].CleanObstacle();
                        break;
                    }
                }
            }
        }
        #endregion
    }

    // TODO: ������ �ʿ��� �ش� �����Ⱑ �÷��̾� ������ AI �Ǵ��ϰ� ���ڷ� �Ѱ��ֱ�
    // �����Ⱑ Ǯ�� ��ȯ�Ǵ� ������ ȣ��
    public void AddScore(int score, bool isPlayer)
    {
        if (isPlayer)   // �÷��̾� ������ ���� �߰�
            PlayerScore += score;
        
        else    // AI ������ ���� �߰�
            AIScore += score;
    }
}
