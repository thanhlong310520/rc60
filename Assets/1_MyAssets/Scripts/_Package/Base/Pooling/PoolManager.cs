using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum PoolName
{
    None,
    Poision,
    Stun,
    DamageSprite,
    Freeze,
    Health,
    CreateCharacter,
    DigitSprite,
    Victory
}

[System.Serializable]
public struct PoolNameAndNamePrefab
{
    public PoolName poolName;
    public GameObject poolRef;

    public PoolNameAndNamePrefab(PoolName _poolName, GameObject _poolRef)
    {
        poolName = _poolName;
        poolRef = _poolRef;
    }
}

public class PoolManager : MonoSingleton<PoolManager>
{
    public PoolNameAndNamePrefab[] effectObjects = null;
    public PoolNameAndNamePrefab[] otherObjects = null;

    private PoolNameAndNamePrefab[] poolNameAndNamePrefab = null;
    private IDictionary<PoolName, Queue<GameObject>> pools;

    protected override void Awake()
    {
        base.Awake();
        pools = new Dictionary<PoolName, Queue<GameObject>>();

        var effectPoolNumber = effectObjects.Length;
        var otherPoolNumber = otherObjects.Length;
        var poolNumber = effectPoolNumber + otherPoolNumber;

        poolNameAndNamePrefab = new PoolNameAndNamePrefab[poolNumber];
        //poolSizes = new PoolSize[poolNumber];
        for (int i = 0; i < poolNumber; i++)
        {
            if (i < effectPoolNumber)
            {
                poolNameAndNamePrefab[i] = effectObjects[i];
                continue;
            }
            if (i - effectPoolNumber < otherPoolNumber)
            {
                poolNameAndNamePrefab[i] = otherObjects[i - effectPoolNumber];
                continue;
            }
        }
    }

    public GameObject PopPool(PoolName poolName)
    {
        GameObject _object = null;
        if (pools.ContainsKey(poolName) == false) pools.Add(poolName, new Queue<GameObject>());
        if (pools[poolName].Count == 0)
        {
            var _cache = Instantiate(GetPrefabByName(poolName)) as GameObject;
            pools[poolName].Enqueue(_cache);
        }
        _object = pools[poolName].Dequeue();
        _object.name = poolName.ToString();
        _object.Show();

        //if (!IsEffect(poolName))
        //    GameEvent.OnCreateObjectMethod(_object);

        return _object;
    }    

    public GameObject PopPool(PoolName poolName, Vector3 pos = new Vector3(), Quaternion rotate = new Quaternion())
    {
        GameObject obj = PopPool(poolName);
        obj.transform.parent = null;
        obj.transform.position = pos;
        obj.transform.rotation = rotate;
        return obj;
    }

    public GameObject PopPool(PoolName poolName, Transform parent, Vector3 pos = new Vector3(), Quaternion rotate = new Quaternion())
    {
        GameObject obj = PopPool(poolName);

        obj.transform.parent = parent;
        obj.transform.position = pos;
        obj.transform.rotation = rotate;

        return obj;
    }

    public T PopPoolWithComponent<T>(PoolName poolName, Vector3 pos = new Vector3(), Quaternion rotate = new Quaternion())
    {
        GameObject obj = PopPool(poolName);

        obj.transform.parent = null;
        obj.transform.position = pos;
        obj.transform.rotation = rotate;

        return obj.GetComponent<T>();
    }

    public T PopPoolWithComponent<T>(PoolName poolName, Transform parent, Vector3 pos = new Vector3(), Quaternion rotate = new Quaternion())
    {
        GameObject obj = PopPool(poolName);

        obj.transform.parent = parent;
        obj.transform.position = pos;
        obj.transform.rotation = rotate;

        return obj.GetComponent<T>();
    }

    public void PushPool(GameObject obj, PoolName poolName)
    {
        if (obj == null) return;

        obj.transform.parent = this.transform;
        obj.transform.DOKill();
        obj.Hide();

        pools[poolName].Enqueue(obj);
    }

    public void PushPool(GameObject go, PoolName poolName, float after)
    {
        DOVirtual.DelayedCall(after, () => PushPool(go, poolName));
    }    

    private GameObject GetPrefabByName(PoolName name)
    {
        for(int i = 0; i < poolNameAndNamePrefab.Length; i++)
        {
            if (poolNameAndNamePrefab[i].poolName == name)
                return poolNameAndNamePrefab[i].poolRef;
        }
        return null;
    }

    public bool IsEffect(PoolName poolName)
    {
        for(int i = 0; i < effectObjects.Length; i++)
        {
            if (effectObjects[i].poolName == poolName)
                return true;
        }    
        return false;
    }
}