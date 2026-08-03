using System.Collections;
using UnityEngine;

/// <summary>
/// Xử lý chuỗi hành động khi Player Win:
/// 1. Nhân vật tự động chạy thêm một đoạn về phía trước trên mặt đất.
/// 2. Camera bám theo nhân vật trong lúc chạy.
/// 3. Nhân vật dừng lại, camera xoay/di chuyển để chiếu thẳng vào nhân vật.
/// 4. Nhân vật chơi animation nhảy múa (dance).
///
/// Gắn script này vào 1 GameObject quản lý (ví dụ: GameManager hoặc chính Player),
/// rồi gọi TriggerWin() khi điều kiện thắng xảy ra.
/// </summary>
public class WinSequenceController : MonoBehaviour
{
    [Header("Tham chiếu bắt buộc")]
    [Tooltip("Transform của nhân vật sẽ chạy và nhảy múa")]
    [SerializeField] private Transform character;

    [Tooltip("Transform của Camera sẽ di chuyển/bám theo")]
    [SerializeField] private Transform cameraTransform;

    [Header("Cấu hình chạy thêm")]
    [Tooltip("Khoảng cách nhân vật sẽ chạy thêm về phía trước (mét)")]
    [SerializeField] private float runDistance = 5f;

    [Tooltip("Tốc độ chạy (m/s)")]
    [SerializeField] private float runSpeed = 4f;

    [Header("Cấu hình Camera bám theo")]
    [Tooltip("Offset của camera so với nhân vật trong lúc chạy (theo local: x=ngang, y=cao, z=lùi sau)")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 2f, -4f);

    [Tooltip("Độ mượt khi camera bám theo nhân vật, số càng nhỏ càng mượt/chậm")]
    [SerializeField] private float followSmoothTime = 0.2f;

    [Header("Cấu hình Camera cận cảnh lúc nhảy múa")]
    [Tooltip("Offset camera khi đã dừng lại, chiếu thẳng vào nhân vật")]
    [SerializeField] private Vector3 finalCamOffset = new Vector3(0f, 1.6f, -2.5f);

    [Tooltip("Thời gian camera di chuyển vào vị trí cận cảnh (giây)")]
    [SerializeField] private float finalCamTransitionTime = 1f;

    [Header("Sự kiện - gắn animation/logic của bạn vào đây trong Inspector")]
    [Tooltip("Gọi khi nhân vật bắt đầu chạy (VD: bật animation chạy)")]
    public UnityEngine.Events.UnityEvent OnRunStarted;

    [Tooltip("Gọi khi nhân vật vừa dừng lại, hết chạy (VD: tắt animation chạy)")]
    public UnityEngine.Events.UnityEvent OnRunFinished;

    [Tooltip("Gọi khi camera đã vào vị trí cận cảnh, bắt đầu nhảy múa (VD: bật animation dance)")]
    public UnityEngine.Events.UnityEvent OnDanceStarted;

    private Vector3 _camVelocity = Vector3.zero;
    private bool _isPlaying = false;

    /// <summary>
    /// Gọi hàm này khi người chơi Win để bắt đầu chuỗi hành động.
    /// </summary>
    public void TriggerWin()
    {
        if (_isPlaying) return;
        StartCoroutine(PlayWinSequence());
    }

    private IEnumerator PlayWinSequence()
    {
        _isPlaying = true;

        // ----- Bước 1: Nhân vật chạy về phía trước -----
        Vector3 startPos = character.position;
        Vector3 targetPos = startPos + character.forward * runDistance;

        OnRunStarted?.Invoke();

        while (Vector3.Distance(character.position, targetPos) > 0.05f)
        {
            // Di chuyển nhân vật về phía trước trên mặt đất
            character.position = Vector3.MoveTowards(character.position, targetPos, runSpeed * Time.deltaTime);

            // Camera bám theo nhân vật (theo hướng nhìn hiện tại của nhân vật)
            Vector3 desiredCamPos = character.position
                                    + character.right * followOffset.x
                                    + Vector3.up * followOffset.y
                                    + character.forward * followOffset.z;

            cameraTransform.position = Vector3.SmoothDamp(
                cameraTransform.position, desiredCamPos, ref _camVelocity, followSmoothTime);

            cameraTransform.LookAt(character.position + Vector3.up * 1.5f);

            yield return null;
        }

        // Đảm bảo dừng đúng vị trí
        character.position = targetPos;

        OnRunFinished?.Invoke();

        // ----- Bước 2: Camera di chuyển vào vị trí chiếu thẳng nhân vật -----
        Vector3 camStartPos = cameraTransform.position;
        Quaternion camStartRot = cameraTransform.rotation;

        Vector3 finalCamPos = character.position
                              + character.right * finalCamOffset.x
                              + Vector3.up * finalCamOffset.y
                              + character.forward * finalCamOffset.z;

        Vector3 lookTarget = character.position + Vector3.up * 1.5f;
        Quaternion finalCamRot = Quaternion.LookRotation(lookTarget - finalCamPos);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / finalCamTransitionTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.position = Vector3.Lerp(camStartPos, finalCamPos, smoothT);
            cameraTransform.rotation = Quaternion.Slerp(camStartRot, finalCamRot, smoothT);

            yield return null;
        }

        cameraTransform.position = finalCamPos;
        cameraTransform.rotation = finalCamRot;

        // ----- Bước 3: Gọi sự kiện để bên ngoài xử lý animation nhảy múa -----
        OnDanceStarted?.Invoke();

        _isPlaying = false;
    }
}
