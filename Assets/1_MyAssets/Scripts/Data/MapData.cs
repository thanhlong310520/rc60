
using UnityEngine;

/// <summary>
/// ScriptableObject chứa thông tin 1 map: id và prefab tương ứng.
/// Tạo asset qua menu: Assets > Create > Game > MapData
/// </summary>
[CreateAssetMenu(fileName = "MapData", menuName = "Game/MapData")]
public class MapData : ScriptableObject
{
    public string mapId;
    public GameObject mapPrefab;
}
