using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lắc một UI element (RectTransform). Dùng anchoredPosition nên hoạt động đúng
/// với mọi anchor/pivot và tự scale theo CanvasScaler.
/// Gọi StopShake() để dừng và trả về trạng thái ban đầu.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class ShakeUI : MonoBehaviour
{
    [Header("Target (để trống = chính object này)")]
    [SerializeField] private RectTransform target;

    [Header("Bật/tắt từng thành phần")]
    [SerializeField] private bool shakePosition = true;
    [SerializeField] private bool shakeRotation = false;
    [SerializeField] private bool shakeScale = false;

    [Header("Cường độ")]
    [Tooltip("Tính bằng pixel theo Reference Resolution của CanvasScaler.")]
    [SerializeField] private Vector2 positionStrength = new Vector2(15f, 15f);
    [Tooltip("Độ (degrees), thường chỉ cần trục Z cho UI.")]
    [SerializeField] private float rotationStrength = 6f;
    [SerializeField] private Vector2 scaleStrength = new Vector2(0.08f, 0.08f);

    [Header("Thông số")]
    [Tooltip("Thời gian lắc. Để 0 = lắc vô hạn cho tới khi gọi StopShake().")]
    [Min(0f)] [SerializeField] private float duration = 0.35f;
    [Tooltip("Tốc độ rung, càng cao càng gấp.")]
    [Min(0.1f)] [SerializeField] private float frequency = 25f;
    [SerializeField] private bool fadeOut = true;
    [Tooltip("Bật nếu UI cần lắc khi game đang pause (Time.timeScale = 0).")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool playOnEnable = false;

    /// <summary>Bắn ra khi lắc kết thúc (cả khi hết giờ lẫn khi bị StopShake).</summary>
    public event Action OnShakeEnd;

    public bool IsShaking { get; private set; }

    private Vector2 _originAnchoredPos;
    private Quaternion _originRotation;
    private Vector3 _originScale;
    private Coroutine _routine;
    private float _seed;

    #region Unity

    private void Awake()
    {
        if (target == null) target = GetComponent<RectTransform>();
        CaptureOrigin();
        WarnIfControlledByLayout();
    }

    private void OnEnable()
    {
        if (playOnEnable) Shake();
    }

    private void OnDisable()
    {
        StopShake();
    }

    #endregion

    #region Public API

    /// <summary>Lắc theo thông số cài trong Inspector.</summary>
    public void Shake() => Shake(duration, 1f);

    /// <summary>Lắc với thời gian tuỳ chọn. duration &lt;= 0 nghĩa là lắc vô hạn.</summary>
    public void Shake(float duration, float multiplier = 1f)
    {
        if (!isActiveAndEnabled) return;

        // Đang lắc dở thì trả về gốc trước, tránh cộng dồn sai vị trí.
        if (IsShaking) ResetToOrigin();
        else CaptureOrigin();

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShakeRoutine(duration, Mathf.Max(0f, multiplier)));
    }

    /// <summary>Lắc mãi cho tới khi gọi StopShake().</summary>
    public void ShakeForever(float multiplier = 1f) => Shake(0f, multiplier);

    /// <summary>Dừng lắc. Mặc định trả target về đúng trạng thái ban đầu.</summary>
    public void StopShake(bool restore = true)
    {
        bool wasShaking = IsShaking;

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        IsShaking = false;
        if (restore) ResetToOrigin();
        if (wasShaking) OnShakeEnd?.Invoke();
    }

    /// <summary>Ghi lại trạng thái hiện tại làm trạng thái gốc mới.</summary>
    public void CaptureOrigin()
    {
        if (target == null) target = GetComponent<RectTransform>();
        _originAnchoredPos = target.anchoredPosition;
        _originRotation = target.localRotation;
        _originScale = target.localScale;
    }

    /// <summary>Trả target về trạng thái gốc ngay lập tức.</summary>
    public void ResetToOrigin()
    {
        if (target == null) return;
        if (shakePosition) target.anchoredPosition = _originAnchoredPos;
        if (shakeRotation) target.localRotation = _originRotation;
        if (shakeScale) target.localScale = _originScale;
    }

    #endregion

    #region Core

    private IEnumerator ShakeRoutine(float dur, float multiplier)
    {
        IsShaking = true;
        _seed = UnityEngine.Random.Range(0f, 1000f);

        bool infinite = dur <= 0f;
        float elapsed = 0f;

        while (infinite || elapsed < dur)
        {
            float damper = (!infinite && fadeOut) ? 1f - Mathf.Clamp01(elapsed / dur) : 1f;
            damper *= multiplier;

            float t = elapsed * frequency;

            if (shakePosition)
            {
                Vector2 offset = new Vector2(
                    Noise(t, 0f) * positionStrength.x,
                    Noise(t, 31f) * positionStrength.y);
                target.anchoredPosition = _originAnchoredPos + offset * damper;
            }

            if (shakeRotation)
                target.localRotation = _originRotation *
                    Quaternion.Euler(0f, 0f, Noise(t, 57f) * rotationStrength * damper);

            if (shakeScale)
            {
                Vector3 offset = new Vector3(
                    Noise(t, 83f) * scaleStrength.x,
                    Noise(t, 109f) * scaleStrength.y,
                    0f);
                target.localScale = _originScale + offset * damper;
            }

            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        _routine = null;
        StopShake();   // hết giờ -> tự trả về gốc + bắn OnShakeEnd
    }

    /// <summary>Giá trị mượt trong khoảng [-1, 1].</summary>
    private float Noise(float t, float offset) => Mathf.PerlinNoise(_seed + offset, t) * 2f - 1f;

    /// <summary>Cảnh báo nếu Layout Group sẽ ghi đè vị trí khi đang lắc.</summary>
    private void WarnIfControlledByLayout()
    {
        if (!shakePosition || target.parent == null) return;
        if (target.parent.GetComponent<LayoutGroup>() == null) return;

        var element = target.GetComponent<LayoutElement>();
        if (element == null || !element.ignoreLayout)
        {
            Debug.LogWarning(
                $"[ShakeUI] '{name}' nằm trong Layout Group nên vị trí sẽ bị ghi đè khi lắc. " +
                "Hãy gắn ShakeUI lên một child object, hoặc thêm LayoutElement với Ignore Layout = true.",
                this);
        }
    }

    #endregion
}
