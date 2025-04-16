using System.Collections.Generic;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    [SerializeField] private float timeLimit;    // 제한 시간
    public int PlayerObstacleCnt;    // 플레이어 진영 내 쓰레기 수
    public int AIObstacleCnt;        // AI 진영 내 쓰레기 수
    public bool IsPlaying; // 플레이 중인지 확인하는 플래그

    public int PlayerScore { get; private set; }    // 플레이어 진영 점수
    public int AIScore { get; private set; }    // AI 진영 점수
    public float TimeLimit { get { return timeLimit;  } } // 제한 시간
    public float TimeLeft { get; private set; } // 잔여 시간
    public bool IsWin { get; private set; } // 승패 결과

    protected override void Awake()
    {
        base.Awake();

        // 점수 및 제한 시간 초기화
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
        if (!IsPlaying) return; // 플레이 중일 때만 실행

        TimeLeft -= Time.deltaTime;    // 제한 시간 감소

        if (TimeLeft <= 0)  // 제한 시간 종료 시
        {
            if (PlayerScore >= AIScore) // 플레이어 점수가 더 높으면 승리
                IsWin = true;
            else    // AI 점수가 더 높으면 패배
                IsWin = false;
            GameManager.Instance.GameOver();
        }

        // 플레이어 진영 내 쓰레기가 없으면 승리
        if (PlayerObstacleCnt <= 0)    
        {
            IsWin = true;
            GameManager.Instance.GameOver();
        }

        // AI 진영 내 쓰레기가 없으면 패배
        if (AIObstacleCnt <= 0)
        {
            IsWin = false;
            GameManager.Instance.GameOver();
        }

        #region TestCode
        {
            if (Input.GetKeyDown(KeyCode.O))  // O를 누르면 AI 먼지 삭제
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
            if (Input.GetKeyDown(KeyCode.P))  // P를 누르면 Player 먼지 삭제
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

    // TODO: 쓰레기 쪽에서 해당 쓰레기가 플레이어 측인지 AI 판단하고 인자로 넘겨주기
    // 쓰레기가 풀로 반환되는 시점에 호출
    public void AddScore(int score, bool isPlayer)
    {
        if (isPlayer)   // 플레이어 진영에 점수 추가
            PlayerScore += score;
        
        else    // AI 진영에 점수 추가
            AIScore += score;
    }
}
