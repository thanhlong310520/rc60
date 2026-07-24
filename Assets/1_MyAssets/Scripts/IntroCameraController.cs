using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Camera intro giống style "Obby Run": bắt đầu từ điểm kết thúc màn chơi (endPoint),
/// bay lên cao, sau đó tiến thẳng về phía Player trong khi tự xoay quanh trục của chính nó.
///
/// Lưu ý: bản này camera KHÔNG hướng vào Player và KHÔNG orbit quanh Player.
/// - Giai đoạn bay lên: camera nhìn về phía endPoint (điểm nó vừa xuất phát).
/// - Giai đoạn 2: camera di chuyển thẳng (lerp vị trí) tới điểm gần Player, đồng thời
///   tự xoay quanh trục thẳng đứng của chính nó (giống spin/con quay tại chỗ) đủ
///   orbitCount vòng (mặc định 2 vòng), độc lập với việc nó đang bay tới đâu.
///
/// Cách dùng:
/// 1. Gắn script này vào một GameObject rỗng trong scene (ví dụ "IntroCameraController").
/// 2. Kéo Camera chính vào ô "cam".
/// 3. Kéo điểm cuối màn chơi (nơi camera bắt đầu) vào "endPoint".
/// 4. Kéo Transform của Player vào "player" (dùng để tính điểm đích ở giai đoạn 2).
/// 5. Tùy chỉnh các thông số trong Inspector.
/// 6. Khi intro kết thúc, sự kiện OnIntroComplete sẽ được gọi.
/// </summary>
public class IntroCameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Camera sẽ được điều khiển trong intro")]
    public Transform cam;

    [Tooltip("Điểm kết thúc màn chơi - nơi camera bắt đầu (vị trí ban đầu)")]
    public Transform endPoint;
    public Vector3 offsetEndPoint;
    [Tooltip("Transform của Player - dùng làm tâm xoay ở giai đoạn 2")]
    public Transform player;
    public Vector3 offsetPlayer;

    [Header("Giai đoạn 1: Bay lên cao")]
    [Tooltip("Camera sẽ bay lên cao thêm bao nhiêu mét so với endPoint")]
    public float riseHeight = 20f;
    [Tooltip("Thời gian (giây) cho giai đoạn bay lên")]
    public float riseDuration = 1f;

    [Header("Giai đoạn 2: Xoay vòng quanh Player")]
    [Tooltip("Số vòng xoay quanh Player trước khi kết thúc (Obby Run thường dùng 2 vòng)")]
    public int orbitCount = 2;
    [Tooltip("Thời gian (giây) cho giai đoạn xoay vòng + tiến lại gần")]
    public float orbitDuration = 3f;
    [Tooltip("Bán kính quanh Player khi kết thúc xoay (camera sẽ dừng cách Player khoảng này)")]
    public float finalRadius = 4f;
    [Tooltip("Độ cao camera so với Player khi kết thúc")]
    public float finalHeightOffset = 2.5f;

    [Header("Easing")]
    [Tooltip("Đường cong tăng tốc/giảm tốc cho cả 2 giai đoạn. Mặc định EaseInOut nếu để trống.")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sự kiện")]
    [Tooltip("Gọi khi intro kết thúc - dùng để enable player control, chuyển camera sang follow-cam, v.v.")]
    public UnityEngine.Events.UnityEvent OnIntroComplete;

    [Tooltip("Tự động chạy intro khi Start(). Tắt nếu bạn muốn gọi PlayIntro() thủ công.")]
    public bool playOnStart = true;

    private void Start()
    {

        if (playOnStart)
            StartCoroutine(PlayIntro());
    }

    /// <summary>
    /// Gọi hàm này để bắt đầu intro theo yêu cầu (ví dụ khi load xong màn chơi).
    /// </summary>
    public void PlayIntroNow(Transform player)
    {
        this.player = player;
        StopAllCoroutines();
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        if (cam == null || endPoint == null || player == null)
        {
            Debug.LogWarning("IntroCameraController: thiếu reference cam/endPoint/player.");
            yield break;
        }

        cam.gameObject.SetActive(true);
        // ---------- GIAI ĐOẠN 1: Bay thẳng lên từ endPoint ----------
        Vector3 startPos = endPoint.position + offsetEndPoint;
        Vector3 risePos = startPos + Vector3.up * riseHeight;

        cam.position = startPos;
        LookAtEndPoint();

        float t = 0f;
        while (t < riseDuration)
        {
            t += Time.deltaTime;
            float p = easeCurve.Evaluate(Mathf.Clamp01(t / riseDuration));

            cam.position = Vector3.Lerp(startPos, risePos, p);
            LookAtEndPoint(); // giai đoạn bay lên: nhìn xuống endPoint, KHÔNG nhìn player

            yield return null;
        }
        cam.position = risePos;

        // ---------- GIAI ĐOẠN 2: Tiến thẳng về phía Player + tự xoay quanh trục của chính mình ----------
        // Vị trí: di chuyển thẳng (lerp) từ vị trí hiện tại đến vị trí đích gần Player.
        // Xoay: camera KHÔNG orbit quanh Player và KHÔNG nhìn vào Player.
        // Thay vào đó nó tự quay quanh trục thẳng đứng của chính nó (giống con quay/spin tại chỗ),
        // quay đủ orbitCount vòng (360 độ * orbitCount) trong lúc bay tới, độ nghiêng (pitch/roll)
        // giữ nguyên như lúc kết thúc giai đoạn 1.
        Vector3 startPos2 = cam.position;
        Vector3 targetPos2 = ComputeFinalPositionNearPlayer();

        Vector3 startEuler = cam.eulerAngles; // giữ nguyên pitch (x) và roll (z), chỉ xoay yaw (y)
        float startYaw = startEuler.y;
        float totalYaw = orbitCount * 360f; // số vòng * 360 độ

        t = 0f;
        while (t < orbitDuration)
        {
            t += Time.deltaTime;
            float p = easeCurve.Evaluate(Mathf.Clamp01(t / orbitDuration));

            // Vị trí đích cập nhật theo Player mỗi frame để bám nếu Player di chuyển trong lúc intro
            targetPos2 = ComputeFinalPositionNearPlayer();

            cam.position = Vector3.Lerp(startPos2, targetPos2, p);

            float yaw = startYaw + totalYaw * p;
            cam.eulerAngles = new Vector3(startEuler.x, yaw, startEuler.z);

            yield return null;
        }

        // Đảm bảo kết thúc đúng vị trí mong muốn (yaw sau n vòng tròn sẽ tự trùng với góc ban đầu)
        cam.position = ComputeFinalPositionNearPlayer();
        cam.eulerAngles = startEuler;

        cam.gameObject.SetActive(false);
        OnIntroComplete?.Invoke();
    }

    /// <summary>
    /// Tính vị trí đích gần Player (phía sau lưng Player theo hướng player.forward, cách finalRadius,
    /// cao hơn finalHeightOffset). Camera sẽ tiến thẳng tới điểm này ở giai đoạn 2.
    /// </summary>
    private Vector3 ComputeFinalPositionNearPlayer()
    {
        Vector3 backDir = player.forward.sqrMagnitude > 0.001f ? -player.forward : Vector3.back;
        return player.position + offsetPlayer + backDir.normalized * finalRadius + Vector3.up * finalHeightOffset;
    }

    private void LookAtEndPoint()
    {
        Vector3 dir = endPoint.position - cam.position;
        //if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward; // tránh lỗi khi cam đứng ngay tại endPoint
        //cam.rotation = Quaternion.LookRotation(dir.normalized);
    }

}