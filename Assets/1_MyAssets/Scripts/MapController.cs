using Raccoon;
using Raccoon.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



/// <summary>
/// Quản lý toàn bộ checkpoint trong map: điểm đầu, điểm cuối,
/// lấy transform theo id, xử lý sự kiện khi chạm checkpoint.
/// </summary>
public class MapController : MonoBehaviour
{

    [Header("Danh sách checkpoint trong scene")]
    [Tooltip("Có thể để trống, hệ thống sẽ tự tìm các Checkpoint con trong Awake")]
    [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

    [SerializeField] private Transform startPoint;
    public IntroCameraController introCameraController;


    private void Awake()
    {
        if (checkpoints == null || checkpoints.Count == 0)
        {
            checkpoints = GetComponentsInChildren<Checkpoint>(true).ToList();
        }

    }

    /// <summary>
    /// Lấy Transform của checkpoint theo id.
    /// Nếu không tìm thấy id -> trả về điểm đầu (startPoint).
    /// </summary>
    public Transform GetCheckpointTransform(string id)
    {
        Checkpoint cp = checkpoints.FirstOrDefault(c => c.id == id);

        if (cp != null)
            return cp.GetPointPlayerStay();
        if (startPoint != null)
            return startPoint;
        return null;
    }

    public Transform GetNextCheckpointTransform(string idLastPoint)
    {
        int index = checkpoints.FindIndex(c => c.id == idLastPoint);
        if (index == -1) return checkpoints[0].transform;

        if (index == checkpoints.Count - 1)
            return checkpoints[checkpoints.Count - 1].GetPointPlayerStay();
        if (index < checkpoints.Count - 1)
            return checkpoints[index + 1].GetPointPlayerStay();
        return null;
    }

    public void Init(Transform player, bool showIntro = true)
    {
        foreach (var c in checkpoints)
        {
            bool isSave = GameData.Get.GetIsCheckpointSaved(GameData.Get.currentMap.mapId, c.id);

            //print("[MapController] Init checkpoint: " + c.id + " isSave: " + isSave);   
            c.Init(this, isSave);
        }
        if (showIntro)
        {
            introCameraController.PlayIntroNow(player);
        }
        else
        {
            introCameraController.cam.gameObject.SetActive(false);
            EndIntro();
        }
    }


    /// <summary>
    /// Gọi khi player va chạm 1 checkpoint (được Checkpoint tự gọi qua OnTriggerEnter).
    /// </summary>
    public void OnPlayerHitCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;
        ShowUICheckpoint(checkpoint);
        SaveCheckpointData(checkpoint);
    }

    /// <summary>
    /// Overload tiện dùng nếu chỉ có id (ví dụ gọi từ nơi khác không có reference Checkpoint).
    /// </summary>
    public void OnPlayerHitCheckpoint(string id)
    {
        Checkpoint cp = checkpoints.Find(c => c.id == id);

        if (cp == null)
        {
            Debug.LogWarning($"[MapController] Checkpoint id '{id}' không tồn tại.");
            return;
        }

        OnPlayerHitCheckpoint(cp);
    }
    public void EndIntro()
    {
        GamePlayController.instance.DoneIntro();
    }

    private void SaveCheckpointData(Checkpoint checkpoint)
    {
        // TODO: thay bằng hệ thống save thật của bạn (JSON, PlayerPrefs, SaveManager,...)
        if (GamePlayController.instance.SaveCheckPoint(checkpoint.id))
        {
            Debug.Log($"[MapController] Đã lưu checkpoint: {checkpoint.id}");
            bool turnOff = true;
            ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.SaveCheckpoint);
            if (checkpoint.isEndPoint)
            {
                WinGame(checkpoint);
                turnOff = false;
            }

            checkpoint.PlayVfx(turnOff);
        }
        else
        {
            Debug.Log($"[MapController] have checkpoint");
        }

    }

    private void WinGame(Checkpoint checkpoint)
    {
        Debug.Log("[MapController] Win game!");
        GamePlayController.instance.WinGame(checkpoint.GetPointPlayerStay().transform.forward);
    }

    public void ShowUICheckpoint(Checkpoint cp)
    {
        int index = checkpoints.IndexOf(cp);
        GamePlayController.instance.ShowUICheckpoint(index, checkpoints.Count);
    }

    public int GetIndexCheckpoint(string id)
    {
        int index = checkpoints.FindIndex(c => c.id == id);
        return index;
    }

    public float GetPercentCheckpoint(string id)
    {
        int index = GetIndexCheckpoint(id);
        if (index < 0) return 0;
        float percent = (float)(index + 1) / checkpoints.Count;
        return percent;
    }

    public List<Checkpoint> GetCheckpoints()
    {
        return checkpoints;
    }
}