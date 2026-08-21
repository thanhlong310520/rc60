using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolByID : MonoSingleton<PoolByID>
{
    private IDictionary<int, List<GameObject>> pools;
    private IDictionary<string, GameObject> poolPrefabs;

    protected override void Awake()
    {
        base.Awake();
        pools = new Dictionary<int, List<GameObject>>();
        poolPrefabs = new Dictionary<string, GameObject>();
    }

    /// <summary>
    /// Lấy một đối tượng từ pool dựa trên đường dẫn Resources.
    /// </summary>
    public GameObject GetPrefab(string path)
    {
        if (poolPrefabs.ContainsKey(path))
        {
            return GetPrefab(poolPrefabs[path]);
        }
        else
        {
            var _prefab = Resources.Load<GameObject>(path);
            if (_prefab == null)
            {
                Debug.LogError($"PoolByID: Prefab not found at path: {path}");
                return null;
            }
            poolPrefabs.Add(path, _prefab);
            return GetPrefab(_prefab);
        }
    }

    /// <summary>
    /// Lấy một đối tượng từ pool dựa trên prefab.
    /// Đối tượng sẽ được kích hoạt (active) và trả về.
    /// </summary>
    public GameObject GetPrefab(GameObject obj)
    {
        var id = obj.GetInstanceID();
        if (pools.ContainsKey(id))
        {
            // Tìm đối tượng đang DEACTIVE trong pool để tái sử dụng
            for (int i = 0; i < pools[id].Count; i++)
            {
                // activeSelf: Kiểm tra xem GameObject này có đang tự nó bị tắt hay không (không phụ thuộc vào cha)
                if (!pools[id][i].activeSelf)
                {
                    //pools[id][i].SetActive(true); // Kích hoạt đối tượng
                    pools[id][i].transform.SetParent(null); // Kích hoạt đối tượng

                    return pools[id][i];
                }
            }
        }
        else
        {
            // Nếu chưa có pool cho loại đối tượng này, tạo mới
            pools.Add(id, new List<GameObject>());
        }

        // Không tìm thấy đối tượng DEACTIVE, tạo mới một bản sao
        var _obj = Instantiate(obj) as GameObject;
        _obj.name = _obj.name.Replace("(Clone)", ""); // Xóa "(Clone)" trong tên
        _obj.SetActive(true); // Kích hoạt đối tượng mới tạo
        pools[id].Add(_obj);
        return _obj;
    }

    /// <summary>
    /// Lấy một đối tượng từ pool với vị trí và xoay cụ thể.
    /// </summary>
    public GameObject GetPrefab(GameObject obj, Vector3 pos, Quaternion quaternion)
    {
        var id = obj.GetInstanceID();
        if (pools.ContainsKey(id))
        {
            for (int i = 0; i < pools[id].Count; i++)
            {
                if (!pools[id][i].activeSelf)
                {
                    pools[id][i].transform.position = pos;
                    pools[id][i].transform.rotation = quaternion;
                    pools[id][i].SetActive(true);
                    return pools[id][i];
                }
            }
        }
        else
        {
            pools.Add(id, new List<GameObject>());
        }

        var _obj = Instantiate(obj, pos, quaternion) as GameObject;
        _obj.name = _obj.name.Replace("(Clone)", "");
        _obj.SetActive(true);
        pools[id].Add(_obj);
        return _obj;
    }

    /// <summary>
    /// Lấy một đối tượng từ pool với vị trí, xoay và parent cụ thể.
    /// </summary>
    public GameObject GetPrefab(GameObject obj, Vector3 pos, Quaternion quaternion, Transform parent)
    {
        var id = obj.GetInstanceID();
        if (pools.ContainsKey(id))
        {
            for (int i = 0; i < pools[id].Count; i++)
            {
                if (!pools[id][i].activeSelf)
                {
                    pools[id][i].transform.position = pos;
                    pools[id][i].transform.rotation = quaternion;
                    pools[id][i].transform.SetParent(parent); // Gắn parent cho đối tượng tái sử dụng
                    pools[id][i].SetActive(true);
                    return pools[id][i];
                }
            }
        }
        else
        {
            pools.Add(id, new List<GameObject>());
        }

        // Sửa lỗi: Đảm bảo Instantiate truyền parent vào
        var _obj = Instantiate(obj, pos, quaternion, parent) as GameObject;
        _obj.name = _obj.name.Replace("(Clone)", "");
        _obj.SetActive(true);
        pools[id].Add(_obj);
        return _obj;
    }

    /// <summary>
    /// Lấy một đối tượng từ pool với parent cụ thể.
    /// </summary>
    public GameObject GetPrefab(GameObject obj, Transform parent)
    {
        var id = obj.GetInstanceID();
        if (pools.ContainsKey(id))
        {
            for (int i = 0; i < pools[id].Count; i++)
            {
                if (!pools[id][i].activeSelf)
                {
                    pools[id][i].transform.SetParent(parent); // Gắn parent cho đối tượng tái sử dụng
                    pools[id][i].SetActive(true);
                    return pools[id][i];
                }
            }
        }
        else
        {
            pools.Add(id, new List<GameObject>());
        }

        var _obj = Instantiate(obj, parent) as GameObject;
        _obj.name = _obj.name.Replace("(Clone)", "");
        _obj.SetActive(true);
        pools[id].Add(_obj);
        return _obj;
    }

    /// <summary>
    /// Các phương thức tiện ích để lấy Component cùng lúc.
    /// </summary>
    public T GetPrefab<T>(GameObject obj, Vector3 pos, Quaternion quaternion) where T : Component
    {
        var _obj = GetPrefab(obj, pos, quaternion);
        return (_obj != null) ? _obj.GetComponent<T>() : null;
    }

    public T GetPrefab<T>(GameObject obj, Transform parent) where T : Component
    {
        var _obj = GetPrefab(obj, parent);
        return (_obj != null) ? _obj.GetComponent<T>() : null;
    }

    public T GetPrefab<T>(GameObject obj) where T : Component
    {
        var _obj = GetPrefab(obj);
        return (_obj != null) ? _obj.GetComponent<T>() : null;
    }

    /// <summary>
    /// Đẩy một đối tượng vào pool để tái sử dụng ngay lập tức.
    /// Đối tượng sẽ được ẩn và không còn parent.
    /// </summary>
    public void PushToPool(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.SetParent(transform); // Gỡ parent để tránh giữ reference không cần thiết
            obj.SetActive(false); // Ẩn đối tượng
        }
    }

    /// <summary>
    /// Đẩy một đối tượng vào pool, có thể giữ nguyên parent hoặc gỡ.
    /// </summary>
    /// <param name="obj">Đối tượng cần đẩy vào pool.</param>
    /// <param name="keepParent">Nếu là true, đối tượng sẽ giữ nguyên parent. Mặc định là false (gỡ parent).</param>
    public void PushToPool(GameObject obj, bool keepParent = false)
    {
        if (obj != null)
        {
            if (keepParent == false)
            {
                obj.transform.SetParent(null);
            }
            obj.SetActive(false);
        }
    }

    /// <summary>
    /// Đẩy một đối tượng vào pool sau một khoảng thời gian.
    /// </summary>
    /// <param name="obj">Đối tượng cần đẩy vào pool.</param>
    /// <param name="time">Thời gian (giây) sau đó đối tượng sẽ được đẩy vào pool.</param>
    public void PushToPool(GameObject obj, float time)
    {
        if (obj != null)
        {
            // Sử dụng DOTween's DelayedCall để đẩy vào pool sau một thời gian
            DOVirtual.DelayedCall(time, () => {
                if (obj != null) // Kiểm tra lại obj có thể đã bị hủy
                {
                    obj.transform.SetParent(null);
                    obj.SetActive(false);
                }
            });
        }
    }
}