using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : SingletonBase<PoolManager>
{
    // Inspector���� ���� Ǯ �ѹ��� ����
    [Serializable]
    public class PoolConfig
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    private List<PoolConfig> _poolConfigs = new List<PoolConfig>(); // Ǯ ������ �����ϴ� ����Ʈ
    private Dictionary<string, object> _pools = new Dictionary<string, object>();   // ���� Ǯ�� �ϳ��� �����ϴ� ��ųʸ�

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);  // �� �Ѿ�� �ı����� �ʰ� ����
    }

    // ���ο� Ǯ ����
    private void CreatePool<T>(string tag, GameObject prefab, int size) where T : Component
    {
        // Ǯ ��ųʸ��� �̹� �ش� �±׿� ��ġ�ϴ� Ǯ ������ ����(�ߺ� Ǯ ���� ����)
        if (_pools.ContainsKey(tag))
        {
            Debug.Log($"Pool with tag {tag} already exists.");
            return;
        }

        // ���� ���� �����Ͽ� ����
        GameObject poolObject = new GameObject($"Pool_{tag}"); // Ǯ ������ �� ���ӿ�����Ʈ �����ϰ� �±׷� �̸� ����
        poolObject.transform.SetParent(transform); // PoolManager�� �ڽ����� ����


        // Inspector���� �޾ƿ� ���� ���� ������� ���ο� ������Ʈ Ǯ ����
        IObjectPool<T> objectPool = new ObjectPool<T>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab);
                obj.name = tag; // �����Ǵ� Ǯ�� ������Ʈ�� �̸��� �±׸�� �����ϰ� ����
                obj.transform.SetParent(poolObject.transform);
                return obj.GetComponent<T>();
            },
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => Destroy(obj.gameObject),
            defaultCapacity: size,
            maxSize: 100
        );

        ExpandPool(objectPool, size);    // size��ŭ �̸� ����

        _pools.Add(tag, objectPool);    // Ǯ ��ųʸ��� ���ο� ������Ʈ Ǯ �߰�
    }

    // ���ڷ� ���� �����ŭ Ǯ�� Ȯ��
    private void ExpandPool<T>(IObjectPool<T> pool, int size) where T : Component
    {
        Stack<T> temp = new Stack<T>();
        for (int i = 0; i < size; i++)
        {
            temp.Push(pool.Get());
        }
        for (int i = 0; i < size; i++)
        {
            pool.Release(temp.Pop());
        }
    }

    // Ǯ ���� ����Ʈ�� ���ο� ���� ���� �߰�
    public void AddPools<T>(PoolConfig[] newPools) where T : Component
    {
        if (newPools == null) return;

        foreach (var pool in newPools)
        {
            if (_pools.ContainsKey(pool.tag)) continue; // �̹� ������ �߰� X
            _poolConfigs.Add(pool); // �ܺ� Ŭ�������� �޾ƿ� Ǯ ���� ����Ʈ�� �߰�
        }
    }

    // Ǯ���� T Ÿ�� ������Ʈ�� ������ ��ȯ(Transform ���� o)
    public T SpawnFromPool<T>(string tag, Vector3 position, Quaternion rotation) where T : Component
    {
        // Ǯ���� ������Ʈ ������ Transform ���� �� ��ȯ
        T obj = SpawnFromPool<T>(tag);
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        Debug.Log($"Type Error: Pool with tag {tag} is {typeof(T)}.");
        return null;
    }

    // Ǯ���� T Ÿ�� ������Ʈ�� ������ ��ȯ
    public T SpawnFromPool<T>(string tag) where T : Component
    {
        // �±׿� ��ġ�ϴ� Ǯ�� ������ Ǯ ����
        if (!_pools.TryGetValue(tag, out var pool))
        {
            foreach (var poolConfig in _poolConfigs)
            {
                if (poolConfig.tag == tag)
                {
                    CreatePool<T>(poolConfig.tag, poolConfig.prefab, poolConfig.size);
                }
            }
        }

        // Ǯ ���� ���� �� �����޽��� ��� �� null ��ȯ
        if (!_pools.TryGetValue(tag, out pool))
        {
            Debug.Log($"Pool with tag {tag} cannot be created.");
            return null;
        }

        // �±׿� ��ġ�ϴ� Ǯ�� ������ 
        if (pool is IObjectPool<T> typedPool)
        {
            // ��� ������Ʈ�� ��� ���̸� Ǯ Ȯ��
            if (typedPool.CountInactive == 0)
            {
                var poolConfig = _poolConfigs.Find(config => config.tag == tag);
                if (poolConfig != null)
                {
                    ExpandPool(typedPool, poolConfig.size);
                }
            }

            // Ǯ���� ������Ʈ ������ ��ȯ
            T obj = typedPool.Get();
            return obj;
        }

        Debug.Log($"Type Error: Pool with tag {tag} is {typeof(T)}.");
        return null;
    }

    public void ReturnToPool<T>(string tag, T obj) where T : Component
    {
        if (obj == null) return;

        // �±׿� ��ġ�ϴ� Ǯ�� �ִ��� ��ȿ�� �˻�
        if (!_pools.TryGetValue(tag, out var pool))
        {
            Debug.Log($"Pool with tag {tag} does not exist.");
            return;
        }

        // ������Ʈ Ǯ�� ��ȯ
        if (pool is IObjectPool<T> typedPool)
        {
            typedPool.Release(obj);
            return;
        }

        Debug.Log($"Type Error: Pool with tag {tag} is {typeof(T)}.");
    }

    // Ư�� �±��� ������Ʈ Ǯ�� ����
    public void DeletePool(string tag)
    {
        // �±׿� ��ġ�ϴ� Ǯ�� �ִ��� ��ȿ�� �˻�
        if (!_pools.ContainsKey(tag))
        {
            Debug.Log($"Pool with tag {tag} does not exist.");
            return;
        }

        if (_pools[tag] is IObjectPool<Component> pool)
        {
            pool.Clear();   // ǽ ��ųʸ����� ����
        }

        Transform poolObject = transform.Find($"Pool_{tag}");
        if (poolObject != null)
        {
            Destroy(poolObject.gameObject);  // ������Ʈ Ǯ ����
        }
    }

    // Ǯ ��ųʸ��� ��ϵ� ��� ������Ʈ Ǯ ����
    public void DeleteAllPools()
    {
        // Ǯ ��ųʸ��� �ִ� ������Ʈ Ǯ�� ������ ���ӿ�����Ʈ ����
        foreach (var key in _pools.Keys)
        {
            Transform poolObject = transform.Find($"Pool_{key}");
            if (poolObject != null)
            {
                Destroy(poolObject.gameObject);
            }
        }
        _pools.Clear(); // Ǯ ��ųʸ����� ��� �׸� ����
    }
}