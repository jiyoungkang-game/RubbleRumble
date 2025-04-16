using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int Score;
    public bool IsPlayer;
    public Transform ParentPool;

    private void OnEnable()
    {
        ParentPool = transform.parent;
    }

    public void CleanObstacle()
    {
        if (IsPlayer)
        {
            StageManager.Instance.AddScore(Score, true);
        }
        else
        {
            StageManager.Instance.AddScore(Score, false);
        }

        RemoveObstacle();
    }

    public void RemoveObstacle()
    {
        transform.parent = ParentPool;
        MapManager.Instance.RemoveFromList(this);
        if (isActiveAndEnabled)
            PoolManager.Instance.ReturnToPool(gameObject.name, this);
    }
}