using System.Collections.Generic;
using UnityEngine;

namespace Raccoon
{
    public class GamePool : MonoBehaviour
    {
        [SerializeField] private int maxSize = 100;

        private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

        public static GamePool Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (Instance != null)
            {
                if (!Instance.pools.TryGetValue(prefab, out var queue))
                {
                    queue = new Queue<GameObject>();
                    Instance.pools[prefab] = queue;
                }
                GameObject obj;

                if (queue.Count > 0)
                {
                    obj = queue.Dequeue();
                    obj.SetActive(true);
                }
                else
                {
                    obj = Instantiate(prefab, position, rotation, parent);
                    // attach a helper so it knows how to return itself
                    var returner = obj.AddComponent<GameObjectPool>();
                    if (returner != null)
                        returner.Init(prefab);
                }

                if (parent != null) obj.transform.SetParent(parent);
                obj.transform.SetPositionAndRotation(position, rotation);

                return obj;
            }
            else
            {
                var obj = Instantiate(prefab, position, rotation, parent);
                return obj;
            }
        }
        
        public static GameObject Get(GameObject prefab, Transform parent = null)
        {
            if (Instance != null)
            {
                if (!Instance.pools.TryGetValue(prefab, out var queue))
                {
                    queue = new Queue<GameObject>();
                    Instance.pools[prefab] = queue;
                }
                GameObject obj = null;

                while(queue.Count > 0)
                {
                    obj = queue.Dequeue();
                    if(obj != null)
                    {
                        var op = obj.GetComponent<GameObjectPool>();
                        if(op != null)
                        {
                            if (!op.isUse)
                            {
                                op.Get();
                                break;
                            }
                        }
                        else
                        {
                            Destroy(obj);
                            obj = null;
                        }
                    }
                }
                if(obj == null)
                {
                    obj = Instantiate(prefab, parent);
                    // attach a helper so it knows how to return itself
                    var returner = obj.AddComponent<GameObjectPool>();
                    if (returner != null)
                    {
                        returner.Init(prefab);
                        returner.Get();
                    }
                }

                if (parent != null) obj.transform.SetParent(parent);
                return obj;
            }
            else
            {
                var obj = Instantiate(prefab, parent);
                return obj;
            }
        }

        public static void Release(GameObject prefab, GameObject obj)
        {
            if (Instance == null)
            {
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(Instance.transform); // keep pool organized
            if(prefab == null) return;
            if (!Instance.pools.TryGetValue(prefab, out var queue))
                queue = Instance.pools[prefab] = new Queue<GameObject>();

            if (queue.Count < Instance.maxSize)
                queue.Enqueue(obj);
            else
                Destroy(obj); // avoid overgrowing pool
        }
        
        public static void Release(GameObject obj)
        {
            if (Instance == null)
            {
                Destroy(obj);
                return;
            }
            
            var prefab = obj.GetComponent<GameObjectPool>();
            if (prefab == null)
            {
                Destroy(obj);
                return;
            }
            prefab.Release();
            obj.SetActive(false);
            obj.transform.SetParent(Instance.transform); // keep pool organized

            if (!Instance.pools.TryGetValue(prefab.PrefabPool, out var queue))
                queue = Instance.pools[prefab.PrefabPool] = new Queue<GameObject>();

            if (queue.Count < Instance.maxSize)
                queue.Enqueue(obj);
            else
                Destroy(obj); // avoid overgrowing pool
        }
    }

}