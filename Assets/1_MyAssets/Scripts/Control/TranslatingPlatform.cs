using UnityEngine;

namespace Raccoon.Environment
{
    /// <summary>
    /// Platform di chuyển tịnh tiến qua danh sách waypoint (xe goòng, thang máy, bè trôi...).
    /// Dùng Rigidbody kinematic + MovePosition để player đứng trên có thể đọc đúng
    /// vận tốc qua Rigidbody.GetPointVelocity().
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class TranslatingPlatform : MonoBehaviour
    {
        public enum LoopMode
        {
            PingPong, // đi hết rồi quay đầu chạy ngược lại
            Loop,     // chạy hết list rồi quay về điểm đầu, lặp vô hạn
            Once      // chạy 1 lượt hết list rồi dừng hẳn
        }

        [Header("Waypoints (world space)")]
        [Tooltip("Danh sách các điểm platform sẽ đi qua, theo thứ tự. Cần tối thiểu 2 điểm.")]
        [SerializeField] private Transform[] waypoints;

        [Header("Speed & Loop")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private LoopMode loopMode = LoopMode.PingPong;
        [Tooltip("Thời gian đứng yên tại mỗi điểm trước khi đi tiếp (giây)")]
        [SerializeField] private float waitTimeAtPoint = 0f;

        private Rigidbody rb;
        private int currentIndex;
        private int direction = 1;
        private float waitTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Start()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                Debug.LogWarning($"{name}: TranslatingPlatform cần ít nhất 2 waypoint.", this);
                enabled = false;
                return;
            }

            rb.position = waypoints[0].position;
            currentIndex = 0;
        }

        private void FixedUpdate()
        {
            if (waitTimer > 0f)
            {
                waitTimer -= Time.fixedDeltaTime;
                return;
            }

            Vector3 targetPos = waypoints[currentIndex].position;
            Vector3 newPos = Vector3.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector3.Distance(newPos, targetPos) < 0.01f)
            {
                waitTimer = waitTimeAtPoint;
                AdvanceIndex();
            }
        }

        private void AdvanceIndex()
        {
            switch (loopMode)
            {
                case LoopMode.PingPong:
                    if (currentIndex + direction < 0 || currentIndex + direction >= waypoints.Length)
                        direction *= -1;
                    currentIndex += direction;
                    break;

                case LoopMode.Loop:
                    currentIndex = (currentIndex + 1) % waypoints.Length;
                    break;

                case LoopMode.Once:
                    if (currentIndex < waypoints.Length - 1)
                        currentIndex++;
                    else
                        enabled = false; // đã tới điểm cuối, dừng hẳn
                    break;
            }
        }

        // Vẽ đường đi trong Scene view cho dễ chỉnh waypoint
        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

                int next = i + 1;
                if (loopMode == LoopMode.Loop && next >= waypoints.Length) next = 0;
                if (next < waypoints.Length && waypoints[next] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }
    }
}
