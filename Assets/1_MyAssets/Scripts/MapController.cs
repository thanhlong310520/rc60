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

    // Bắn ra ngoài để các hệ thống khác (SaveManager, GameManager,...) lắng nghe
    public event Action<string> OnSaveCheckpoint;
    public event Action OnWinGame;

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
        Checkpoint cp = checkpoints.Find(c => c.id == id);

        if (cp != null)
            return cp.transform;
        if (startPoint != null)
            return startPoint;
        return null;
    }

    public void Init(Action<string> actionSaveCheckPoint, Action actionWinGame)
    {
        foreach (var c in checkpoints)
        {
            c.Init(this);
        }
        OnSaveCheckpoint += actionSaveCheckPoint;
        OnWinGame += actionWinGame;
    }

    private void OnDisable()
    {
        OnSaveCheckpoint = null; OnWinGame = null;
    }

    /// <summary>
    /// Gọi khi player va chạm 1 checkpoint (được Checkpoint tự gọi qua OnTriggerEnter).
    /// </summary>
    public void OnPlayerHitCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;

        if (checkpoint.isEndPoint)
        {
            WinGame();
        }
        else
        {
            SaveCheckpointData(checkpoint.id);
        }
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

    private void SaveCheckpointData(string id)
    {
        // TODO: thay bằng hệ thống save thật của bạn (JSON, PlayerPrefs, SaveManager,...)
        
        Debug.Log($"[MapController] Đã lưu checkpoint: {id}");
        OnSaveCheckpoint?.Invoke(id);
    }

    private void WinGame()
    {
        Debug.Log("[MapController] Win game!");
        OnWinGame?.Invoke();
    }
}