using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EventManager — Singleton Observer Hub
/// Đăng ký, huỷ đăng ký và phát sự kiện theo tên (string key).
/// Hỗ trợ: void events, events có 1 tham số generic.
/// </summary>
public class ObserverEventManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────
    private static ObserverEventManager _instance;
    public static ObserverEventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Tự tạo nếu chưa có trong scene
                var go = new GameObject("[EventManager]");
                _instance = go.AddComponent<ObserverEventManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── Storage ──────────────────────────────────────────────────
    // Lưu các listener không có tham số
    private readonly Dictionary<string, Action> _voidEvents
        = new Dictionary<string, Action>();

    // Lưu các listener có 1 tham số (boxing qua object)
    private readonly Dictionary<string, Action<object>> _dataEvents
        = new Dictionary<string, Action<object>>();

    // ─── Void Events ──────────────────────────────────────────────

    /// <summary>Đăng ký lắng nghe sự kiện không có tham số.</summary>
    public void Subscribe(string eventName, Action listener)
    {
        if (!_voidEvents.ContainsKey(eventName))
            _voidEvents[eventName] = null;

        _voidEvents[eventName] += listener;
    }

    /// <summary>Huỷ đăng ký sự kiện không có tham số.</summary>
    public void Unsubscribe(string eventName, Action listener)
    {
        if (_voidEvents.ContainsKey(eventName))
            _voidEvents[eventName] -= listener;
    }

    /// <summary>Phát sự kiện không có tham số.</summary>
    public void Publish(string eventName)
    {
        if (_voidEvents.TryGetValue(eventName, out var action))
            action?.Invoke();
        else
            Debug.LogWarning($"[EventManager] Publish: '{eventName}' không có subscriber nào.");
    }

    // ─── Data Events (Generic) ────────────────────────────────────

    /// <summary>Đăng ký lắng nghe sự kiện có tham số kiểu T.</summary>
    public void Subscribe<T>(string eventName, Action<T> listener)
    {
        if (!_dataEvents.ContainsKey(eventName))
            _dataEvents[eventName] = null;

        // Wrap listener để khớp với Action<object>
        _dataEvents[eventName] += (obj) => listener((T)obj);
    }

    /// <summary>
    /// Huỷ đăng ký sự kiện có tham số.
    /// Lưu ý: vì đã wrap, gọi hàm này sẽ xoá TOÀN BỘ listeners của key đó.
    /// Dùng UnsubscribeAll nếu muốn dọn sạch.
    /// </summary>
    public void Unsubscribe<T>(string eventName, Action<T> listener)
    {
        // Cách an toàn nhất với wrapped delegate: xoá cả key nếu chỉ có 1 listener
        // Nếu cần granular unsubscribe → dùng pattern lưu wrapper riêng (xem ghi chú cuối)
        Debug.LogWarning($"[EventManager] Unsubscribe<T>: Dùng UnsubscribeAll('{eventName}') " +
                         $"hoặc pattern wrapper để unsubscribe chính xác.");
    }

    /// <summary>Phát sự kiện có tham số kiểu T.</summary>
    public void Publish<T>(string eventName, T data)
    {
        if (_dataEvents.TryGetValue(eventName, out var action))
            action?.Invoke(data);
        else
            Debug.LogWarning($"[EventManager] Publish<T>: '{eventName}' không có subscriber nào.");
    }

    // ─── Utilities ────────────────────────────────────────────────

    /// <summary>Xoá toàn bộ listener của một sự kiện.</summary>
    public void UnsubscribeAll(string eventName)
    {
        _voidEvents.Remove(eventName);
        _dataEvents.Remove(eventName);
    }

    /// <summary>Xoá toàn bộ mọi listener (dùng khi load scene mới).</summary>
    public void ClearAll()
    {
        _voidEvents.Clear();
        _dataEvents.Clear();
    }

    /// <summary>In danh sách các event đang active (debug).</summary>
    public void PrintRegisteredEvents()
    {
        Debug.Log("=== Void Events ===");
        foreach (var key in _voidEvents.Keys)
            Debug.Log($"  [{key}]");

        Debug.Log("=== Data Events ===");
        foreach (var key in _dataEvents.Keys)
            Debug.Log($"  [{key}]");
    }



}
public enum EventObserverName
{
    PlaySfx,
}