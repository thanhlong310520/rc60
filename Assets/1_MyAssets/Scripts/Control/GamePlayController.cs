using Raccoon;
using Raccoon.Controller;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý danh sách các map (ScriptableObject) và spawn map theo id khi vào game.
/// </summary>
public class GamePlayController : MonoBehaviour
{
    #region singleton
    public static GamePlayController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    public Transform holderMap;
    public GameObject inputUI;
    [Header("Id map sẽ load khi vào game")]
    [Tooltip("Nếu để trống, sẽ lấy từ PlayerPrefs key 'SelectedMapId'")]
    [SerializeField] private string currentMapId;

    private GameObject currentMapInstance;
    private MapController currentMapController;

    public MapController CurrentMapController => currentMapController;

    private void Start()
    {
        MapData map = GameData.Get.currentMap;
        Init(map);
    }
    public void Init(MapData mapdata)
    {
        SetCanAction(true);
        inputUI.SetActive(false);
        canShowPopup = true;
        if (SpawnMap(mapdata))
        {
            CameraController.instance.gameObject.SetActive(false);
            var startPoint = GetStartPoint();
            if(startPoint == null)
            {
                Debug.Log(($"[GamePlayController] Init False: Khong lay duoc startPoint"));
                InitFall();
            }
            else
            {
                Debug.Log(($"[GamePlayController] Init Done"));

                PlayerController.instance.SetStartPoint(startPoint);
            }
        }
        else
        {
            InitFall();
            Debug.Log(($"[GamePlayController] Init False: khong lay duoc map"));
        }
    }

    void InitFall()
    {
        SceneLoader.Instance.LoadScene("Home");
    }

    public void DoneIntro()
    {
        inputUI.SetActive(true);
        CameraController.instance.gameObject.SetActive(true);
        PlayerController.instance.Init();
    }

    /// <summary>
    /// Tìm MapData theo id và spawn prefab tương ứng vào scene.
    /// </summary>
    public bool SpawnMap(MapData data)
    {

        if (data == null || data.mapPrefab == null)
        {
            Debug.LogWarning($"[GamePlayController] Không tìm thấy map id '{data.mapId}'.");
            return false;
        }

        // Xóa map cũ nếu có (đề phòng load lại map khác)
        if (currentMapInstance != null)
        {
            Destroy(currentMapInstance);
            return false;
        }

        currentMapInstance = Instantiate(data.mapPrefab, Vector3.zero, Quaternion.identity, holderMap);
        currentMapController = currentMapInstance.GetComponent<MapController>();

        if (currentMapController == null)
        {
            Debug.LogWarning($"[GamePlayController] Prefab map '{data.mapId}' không có component MapController.");
            Destroy(currentMapInstance);
            return false;
        }

        currentMapId = data.mapId;

        currentMapController.Init();
        Debug.Log($"[GamePlayController] Đã spawn map: {data.mapId}");
        return true;
    }

    public Transform GetStartPoint()
    {
        string idCheckpoint = PlayerData.Get.GetLastCheckPointInMap(currentMapId);
        return currentMapController.GetCheckpointTransform(idCheckpoint);
    }

    public void Resume()
    {
        var startPoint = GetStartPoint();
        PlayerController.instance.SetStartPoint(startPoint);
        PlayerController.instance.ResetPlayer();
    }

    public void SetNextPoint()
    {

        string idCheckpoint = PlayerData.Get.GetLastCheckPointInMap(currentMapId);
        var nextPoint = currentMapController.GetNextCheckpointTransform(idCheckpoint);
        PlayerController.instance.SetStartPoint(nextPoint.transform);
        PlayerController.instance.ResetPlayer();
    }


    public void NextLevel()
    { 
        GameData.Get.SetWinMap(currentMapId);
        GameData.Get.NextMap();
        SceneLoader.Instance.LoadScene("GamePlay");

    }

    public bool SaveCheckPoint(string idCheckpoint)
    {
        Debug.Log("[GamePlayController] Save check point");

        return GameData.Get.SaveCheckPoint(currentMapId, idCheckpoint);
    }
    public void WinGame()
    {
        Debug.Log("[GamePlayController] Win");
    }
    #region UI
    public void ShowPopup(PopupCanvas.PopupType type, CharacterData data = null)
    {
        if (CheckCanShowPopup())
        {
            MainUiControl.instance.ShowPopup(type, HidePopup, data);
            SetCanAction(false);
        }

    }
    public void ShowPopup(PopupCanvas.PopupType type, UnityAction action)
    {
        MainUiControl.instance.ShowPopup(type, action, null);

    }
    void HidePopup()
    {
        SetCanAction(true);
    }

    bool CheckCanShowPopup()
    {
        bool result = true;
        if (!canShowPopup) result = false;
        if (!canAction) result = false;
        return result;
    }
    public void SetCanAction(bool canAction)
    {
        this.canAction = canAction;
    }


    bool canShowPopup = false;
    bool canAction = false;

    public bool CanAction => canAction;
    #endregion
}
