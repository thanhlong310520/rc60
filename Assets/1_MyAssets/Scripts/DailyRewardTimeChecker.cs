using System;
using UnityEngine;

/// <summary>
/// Class quản lý việc lưu, đọc và so sánh thời gian cho hệ thống Daily Reward.
/// Dùng PlayerPrefs để lưu trữ (đơn giản, không cần backend).
/// </summary>
public class DailyRewardTimeChecker
{
    
    public string ConvertTimeToString(DateTime time)
    {
        return time.Ticks.ToString();
    }

    /// <summary>
    /// Đọc mốc thời gian đã lưu. Trả về null nếu chưa từng lưu (lần đầu chơi).
    /// </summary>
    public DateTime? LoadSavedTime(string lastTimeString)
    {
        if (string.IsNullOrEmpty(lastTimeString)) return null;

        if (long.TryParse(lastTimeString, out long ticks))
        {
            return new DateTime(ticks, DateTimeKind.Utc);
        }
        return null;
    }


    // ----------------------------------------------------------------
    // CÁC HÀM SO SÁNH CHÊNH LỆCH THỜI GIAN
    // ----------------------------------------------------------------

    /// <summary>
    /// Trả về TimeSpan chênh lệch giữa hiện tại và thời điểm đã lưu.
    /// Nếu chưa lưu lần nào, trả về TimeSpan.MaxValue (coi như đã đủ điều kiện).
    /// </summary>
    public TimeSpan GetElapsedTime(string lastTimeString)
    {
        DateTime? saved = LoadSavedTime(lastTimeString);
        if (saved == null) return TimeSpan.MaxValue;

        return DateTime.UtcNow - saved.Value;
    }

    /// <summary>Số ngày đã trôi qua (làm tròn xuống, tính theo 24h).</summary>
    public int GetElapsedDays(string lastTimeString)
    {
        return (int)GetElapsedTime(lastTimeString).TotalDays;
    }

    /// <summary>Số giờ đã trôi qua (tổng số giờ, có phần thập phân nếu cần).</summary>
    public double GetElapsedHours(string lastTimeString)
    {
        return GetElapsedTime(lastTimeString).TotalHours;
    }

    /// <summary>Số phút đã trôi qua.</summary>
    public double GetElapsedMinutes(string lastTimeString)
    {
        return GetElapsedTime(lastTimeString).TotalMinutes;
    }

    /// <summary>
    /// Số tháng chênh lệch, tính theo lịch (calendar month), không phải theo 30 ngày.
    /// Ví dụ: 31/1 -> 1/2 tính là 1 tháng dù chỉ cách 1 ngày.
    /// </summary>
    public int GetElapsedCalendarMonths(string lastTimeString)
    {
        DateTime? saved = LoadSavedTime(lastTimeString);
        if (saved == null) return int.MaxValue;

        DateTime now = DateTime.UtcNow;
        DateTime s = saved.Value;

        int months = (now.Year - s.Year) * 12 + (now.Month - s.Month);
        if (now.Day < s.Day) months--; // chưa đủ tháng tròn
        return months;
    }

    /// <summary>Số năm chênh lệch, tính theo lịch (calendar year).</summary>
    public int GetElapsedCalendarYears(string lastTimeString)
    {
        DateTime? saved = LoadSavedTime(lastTimeString);
        if (saved == null) return int.MaxValue;

        DateTime now = DateTime.UtcNow;
        DateTime s = saved.Value;

        int years = now.Year - s.Year;
        if ((now.Month, now.Day).CompareTo((s.Month, s.Day)) < 0) years--;
        return years;
    }

    /// <summary>
    /// Kiểm tra xem đã sang "ngày mới" theo lịch chưa (khác ngày/tháng/năm),
    /// dùng cho reward kiểu "reset mỗi ngày mới" thay vì "đủ 24 tiếng".
    /// </summary>
    public bool IsNewCalendarDay(string lastTimeString)
    {
        DateTime? saved = LoadSavedTime(lastTimeString);
        if (saved == null) return true;

        DateTime now = DateTime.UtcNow;
        return now.Date != saved.Value.Date;
    }

    /// <summary>
    /// Kiểm tra người chơi có đủ điều kiện nhận reward chưa,
    /// dựa trên khoảng thời gian yêu cầu (ví dụ 24 tiếng).
    /// </summary>
    public bool CanClaimReward(string lastTimeString, TimeSpan requiredInterval)
    {
        return GetElapsedTime(lastTimeString) >= requiredInterval;
    }

    /// <summary>
    /// Kiểm tra xem có bị "mất chuỗi" (streak) không —
    /// tức là đã quá lâu (vượt quá maxAllowedGap) kể từ lần nhận trước.
    /// Dùng để quyết định reset streak về 0.
    /// </summary>
    public bool IsStreakBroken(string lastTimeString, TimeSpan maxAllowedGap)
    {
        return GetElapsedTime(lastTimeString) > maxAllowedGap;
    }


}

/*
 * Sử dụng ví dụ:
if (checker.CanClaimReward(TimeSpan.FromHours(24)))
{
    // Cho nhận reward
    checker.SaveCurrentTime();
}

if (checker.IsStreakBroken(TimeSpan.FromHours(48)))
{
    streakCount = 0; // reset chuỗi
}
*/
