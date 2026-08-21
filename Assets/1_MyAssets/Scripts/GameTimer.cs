using System;
using UnityEngine;

/// <summary>
/// Đồng hồ bấm giờ đơn giản: đo thời gian từ lúc bắt đầu đến lúc kết thúc.
/// Không cần Update() vì dựa trực tiếp vào Time.time.
/// </summary>
public class GameTimer
{
    private float startTime;      // mốc thời gian bắt đầu
    private float accumulated;    // thời gian đã tích lũy (cộng dồn qua các lần pause)
    private bool isRunning;
    private readonly bool useUnscaledTime;

    /// <param name="useUnscaledTime">true nếu muốn đồng hồ không bị ảnh hưởng bởi Time.timeScale</param>
    public GameTimer(bool useUnscaledTime = false)
    {
        this.useUnscaledTime = useUnscaledTime;
    }

    private float Now => useUnscaledTime ? Time.unscaledTime : Time.time;

    public bool IsRunning => isRunning;

    /// <summary>Thời gian đã trôi qua (giây). Đọc được cả khi đang chạy lẫn sau khi dừng.</summary>
    public float Elapsed => isRunning ? accumulated + (Now - startTime) : accumulated;

    /// <summary>Bắt đầu đếm lại từ 0.</summary>
    public void Begin()
    {
        accumulated = 0f;
        startTime = Now;
        isRunning = true;
    }

    /// <summary>Tạm dừng, giữ nguyên thời gian đã đếm.</summary>
    public void Pause()
    {
        if (!isRunning) return;
        accumulated += Now - startTime;
        isRunning = false;
    }

    /// <summary>Chạy tiếp sau khi Pause.</summary>
    public void Resume()
    {
        if (isRunning) return;
        startTime = Now;
        isRunning = true;
    }

    /// <summary>Kết thúc và trả về tổng thời gian (giây).</summary>
    public float End()
    {
        Pause();
        return accumulated;
    }

    public void Reset()
    {
        accumulated = 0f;
        isRunning = false;
    }

    /// <summary>Chuỗi dạng "mm:ss.ff" — tiện gán thẳng vào UI.</summary>
    public override string ToString() => Format(Elapsed);

    public static string Format(float seconds)
    {
        var t = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        return $"{(int)t.TotalMinutes:00}:{t.Seconds:00}.{t.Milliseconds / 10:00}";
    }
}
