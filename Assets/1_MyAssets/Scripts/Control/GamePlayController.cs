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
    public MainUiControl mainUiControl;
    [Header("Id map sẽ load khi vào game")]
    [Tooltip("Nếu để trống, sẽ lấy từ PlayerPrefs key 'SelectedMapId'")]
    [SerializeField] private string currentMapId;

    [SerializeField] PlayerController player;
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
        mainUiControl.gameObject.SetActive(false);
        canShowPopup = true;
        if (SpawnMap(mapdata))
        {
            CameraController.instance.gameObject.SetActive(false);
            var startPoint = GetStartPoint();
            player.SetStartPoint(startPoint);

            string idCheckpoint = PlayerData.Get.GetLastCheckPointInMap(currentMapId);
            int index = currentMapController.GetIndexCheckpoint(idCheckpoint);
            ShowUICheckpoint(index, currentMapController.GetCheckpoints().Count);



            if (startPoint == null)
            {
                Debug.Log(($"[GamePlayController] Init False: Khong lay duoc startPoint"));
                InitFall();
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
        mainUiControl.gameObject.SetActive(true);
        mainUiControl.Init(currentMapId);
        CameraController.instance.gameObject.SetActive(true);
        GameData.Get.PlayBgMusic(SoundType.InGame);
        player.Init();
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
        currentMapController.Init(player.transform, true);
        Debug.Log($"[GamePlayController] Đã spawn map: {data.mapId}");
        return true;
    }

    public Transform GetStartPoint()
    {
        string idCheckpoint = PlayerData.Get.GetLastCheckPointInMap(currentMapId);

        Debug.Log($"[GamePlayController] GetStartPoint: {currentMapId}");
        Debug.Log($"[GamePlayController]  LastCheckpoint: {idCheckpoint}");

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
        GameData.Get.StopBgMusic(SoundType.InGame);
        SceneLoader.Instance.LoadScene("GamePlay");

    }

    public bool SaveCheckPoint(string idCheckpoint)
    {
        Debug.Log("[GamePlayController] Save check point");

        return GameData.Get.SaveCheckPoint(currentMapId, idCheckpoint);
    }
    public void WinGame(Vector3 dirCheckpoint)
    {
        Debug.Log("[GamePlayController] Win");
        mainUiControl.ShowWin();
        GameData.Get.SetInFinishLineMap(currentMapId);
        PlayerController.instance.SetWin(dirCheckpoint);
    }


    public GameObject SpawnItem(GameObject prefab)
    {
        GameObject result = null;
        //if (prefab != null) result = PoolByID.Instance.GetPrefab(prefab);
        if (prefab != null) result = GamePool.Get(prefab);

        return result;
    }

    public void RespawnItem(GameObject item)
    {
        //PoolByID.Instance.PushToPool(item);
        GamePool.Release(item);
    }
    public void ClaimRewardWin(int scale = 1)
    {
        var datareward = GameData.Get.currentMap.rewardWin;
        GameData.Get.SetClaimRewardWinMap(currentMapId);

        foreach (var d in datareward)
        {
            GameData.Get.AddIncome(d.typeCurrency, d.amount * scale);
        }
    }
    public void ShowRewardWin()
    {
        var datareward = GameData.Get.currentMap.rewardWin;
        MainUiControl.instance.ShowReward(datareward);
        
    }

    #region UI

    public void ShowPopup(PopupCanvas.PopupType type)
    {
        if (CheckCanShowPopup())
        {
            MainUiControl.instance.ShowPopup(type, HidePopup, null);
            SetCanAction(false);
        }

    }
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

    public void ShowUICheckpoint(int index, int total)
    {
        // Implementation for showing UI checkpoint

        float percent = (float)(index + 1) / total;
        mainUiControl.ShowCheckPoint(percent);
        mainUiControl.ShowIndexCheckPoint(index + 1, total);
    }

    bool canShowPopup = false;
    bool canAction = false;

    public bool CanAction => canAction;
    #endregion
}
