using UnityEngine;

public class GameObjectPool : MonoBehaviour
{
    public GameObject PrefabPool;
    public void Get()
    {
        isUse = true;
    }
    public void Release()
    {
        isUse = false;
    }
    public bool isUse = false;

    public void Init(GameObject perfab)
    {
        PrefabPool = perfab;
        isUse = false;
    }
}
