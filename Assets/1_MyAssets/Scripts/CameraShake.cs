using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float dampingSpeed = 20f;

    private Vector3 originalLocalPos;
    private float shakeDuration;
    private float shakeMagnitude;

    bool isShake = false;
    private void Awake()
    {
        originalLocalPos = transform.localPosition;
        isShake = false;
    }

    private void Update()
    {
        if (!isShake) return;

        if (shakeDuration > 0)
        {
            Vector3 offset = Random.insideUnitSphere * shakeMagnitude;
            offset.z = 0f; // Nếu game 2D thì giữ nguyên trục Z

            transform.localPosition = originalLocalPos + offset;

            shakeDuration -= Time.deltaTime;
        }
        else
        {
            EndShake();
        }
    }

    /// <summary>
    /// Gọi hàm này để làm rung camera.
    /// </summary>
    /// <param name="duration">Thời gian rung (giây).</param>
    /// <param name="magnitude">Độ mạnh của rung.</param>
    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        isShake = true;
    }

    void EndShake()
    {
        isShake = false;
        shakeDuration = 0;
        transform.localPosition = transform.localPosition;
    }

    public void Test()
    {
        Shake(0.3f, 0.2f);
    }
}