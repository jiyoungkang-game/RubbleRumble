using System.Collections.Generic;
using UnityEngine;

public class MapManager : SingletonBase<MapManager>
{
    [Header("Map")]
    [SerializeField] private Transform playerMap;
    [SerializeField] private Transform aiMap;

    [Header("Count")]
    [SerializeField] private int dirtCnt;
    [SerializeField] private int canCnt;
    [SerializeField] private int boxCnt;

    public List<Obstacle> playerObstacleList;
    public List<Obstacle> aiObstacleList;

    [Header("Pool")]
    [SerializeField] private PoolManager.PoolConfig[] _poolConfigs;

    protected override void Awake()
    {
        base.Awake();
        PoolManager.Instance.AddPools<Obstacle>(_poolConfigs);

        SettingMap(playerMap, aiMap, "Dirt", dirtCnt);
        SettingMap(playerMap, aiMap, "Can", canCnt);
        SettingMap(playerMap, aiMap, "Box", boxCnt);
    }

    private void SettingMap(Transform playerMap, Transform aiMap, string name, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Vector3 randPos = new Vector3(Random.Range(-4.5f, 4.5f), 0.2f, Random.Range(-4.5f, 4.5f));
            Vector3 randPos = new Vector3(Random.Range(-10.0f, 10.0f), 0.2f, Random.Range(-10.0f, 10.0f));
            randPos = transform.TransformDirection(randPos);

            Obstacle playerObstacle = PoolManager.Instance.SpawnFromPool<Obstacle>(name, randPos + playerMap.transform.position, Quaternion.identity);
            playerObstacle.IsPlayer = true;
            AddToList(playerObstacle);

            Obstacle aiObstacle = PoolManager.Instance.SpawnFromPool<Obstacle>(name, randPos + aiMap.transform.position, Quaternion.identity);
            aiObstacle.IsPlayer = false;
            AddToList(aiObstacle);
        }
    }

    public void RemoveFromList(Obstacle obstacle)
    {
        if (obstacle.IsPlayer)
        {
            playerObstacleList.Remove(obstacle);
            StageManager.Instance.PlayerObstacleCnt--;
        }
        else
        {
            aiObstacleList.Remove(obstacle);
            StageManager.Instance.AIObstacleCnt--;
        }
    }

    public void AddToList(Obstacle obstacle)
    {
        if (obstacle.IsPlayer)
        {
            playerObstacleList.Add(obstacle);
            StageManager.Instance.PlayerObstacleCnt++;
        }
        else
        {
            aiObstacleList.Add(obstacle);
            StageManager.Instance.AIObstacleCnt++;
        }
    }

    public void ReturnAllObstacles()
    {
        for (int i = playerObstacleList.Count; i > 0; i--)
        {
            Obstacle playerObstacle = playerObstacleList[i - 1];
            if (playerObstacle.isActiveAndEnabled)
                PoolManager.Instance.ReturnToPool(playerObstacle.name, playerObstacle);
            playerObstacleList.Remove(playerObstacle);
        }
        for (int i = aiObstacleList.Count; i > 0; i--)
        {
            Obstacle aiObstacle = aiObstacleList[i - 1];
            if (aiObstacle.isActiveAndEnabled)
                PoolManager.Instance.ReturnToPool(aiObstacle.name, aiObstacle);
            aiObstacleList.Remove(aiObstacle);
        }
    }
}