using UnityEngine;

namespace Raccoon.Player
{
    /// <summary>
    /// Nhận dữ liệu di chuyển từ PlayerMovement và đẩy vào Animator.
    /// 
    /// Cách kết nối:
    /// - Kéo GameObject chứa PlayerAnimation vào 3 UnityEvent trên PlayerMovement:
    ///     actionAnimMove -> PlayerAnimation.OnAnimMove(float, float, bool)
    ///     jumpAction     -> PlayerAnimation.OnJumpStart()
    ///     jumpedAction   -> PlayerAnimation.OnLanded()   (nếu bạn bắn event này khi chạm đất)
    /// - climbing / isMantling là field public trên PlayerMovement nên được đọc trực tiếp
    ///   mỗi frame (không cần event riêng).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Blend / Damping")]
        [Tooltip("Thời gian làm mượt khi chuyển giá trị Forward/Turn")]
        [SerializeField] private float moveDampTime = 0.15f;

        [Header("Climb Animation")]
        [Tooltip("Tốc độ leo thang tối thiểu để coi là đang di chuyển trên thang (dùng cho blend tree climb nếu có)")]
        [SerializeField] private float climbMoveDampTime = 0.1f;

        // Cache tên tham số Animator sang hash để tránh string lookup mỗi frame
        private static readonly int HashForward = Animator.StringToHash("Forward");
        private static readonly int HashTurn = Animator.StringToHash("Turn");
        private static readonly int HashGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int HashJump = Animator.StringToHash("Jump");
        private static readonly int HashClimbing = Animator.StringToHash("IsClimb");
        private static readonly int HashMantling = Animator.StringToHash("Mantling");
        private static readonly int HashSpeedMultiplier = Animator.StringToHash("SpeedMultiplier");
        private static readonly int HashSpeedClimb = Animator.StringToHash("SpeedClimb");

        private bool prevClimbing;
        private bool prevMantling;

        private void Reset()
        {
            animator = GetComponent<Animator>();
            playerMovement = GetComponentInParent<PlayerMovement>();
            
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (playerMovement == null) playerMovement = GetComponentInParent<PlayerMovement>();

            if(playerMovement != null)
            {
                playerMovement.jumpAction.AddListener(OnJumpStart);
                playerMovement.actionAnimMove.AddListener(OnAnimMove);
                playerMovement.climbAction.AddListener(UpdateSpeedClimb);
            }
        }

        private void OnDisable()
        {
            playerMovement.jumpAction.RemoveListener(OnJumpStart);
            playerMovement.actionAnimMove.RemoveListener(OnAnimMove);
            playerMovement.climbAction.RemoveListener(UpdateSpeedClimb);
        }
        private void Update()
        {
            UpdateClimbAndMantleState();
        }
        


        /// <summary>
        /// Gọi từ UnityEvent actionAnimMove(forwardAmount, turnAmount, isGrounded) của PlayerMovement.
        /// </summary>
        public void OnAnimMove(float forwardAmount, float turnAmount, bool isGrounded)
        {
            if (animator == null) return;

            animator.SetFloat(HashForward, forwardAmount, moveDampTime, Time.deltaTime);
            animator.SetFloat(HashTurn, turnAmount, moveDampTime, Time.deltaTime);
            animator.SetBool(HashGrounded, isGrounded);

            if (playerMovement != null)
            {
                // 1 + multiSpeedRun để Animator có thể dùng làm hệ số tăng tốc clip chạy (Animator.speed hoặc blend)
                animator.SetFloat(HashSpeedMultiplier, 1f + playerMovement.MultiSpeedRun);
            }
        }

        /// <summary>
        /// Gọi từ UnityEvent jumpAction của PlayerMovement (bắn ra ngay lúc bắt đầu nhảy).
        /// </summary>
        public void OnJumpStart()
        {
            if (animator == null) return;
            animator.ResetTrigger(HashJump);
            animator.SetTrigger(HashJump);
        }

        /// <summary>
        /// Gọi từ UnityEvent jumpedAction của PlayerMovement (nếu bạn dùng event này để báo hiệu đã tiếp đất).
        /// </summary>
        public void OnLanded()
        {
            if (animator == null) return;
            animator.ResetTrigger(HashJump);
        }

        /// <summary>
        /// climbing và isMantling là field public trên PlayerMovement (không có event riêng)
        /// nên được poll trực tiếp mỗi frame ở đây.
        /// </summary>
        private void UpdateClimbAndMantleState()
        {
            if (playerMovement == null || animator == null) return;

            bool climbing = playerMovement.climbing;
            bool mantling = playerMovement.isMantling;

            if (climbing != prevClimbing)
            {
                animator.SetBool(HashClimbing, climbing);
                prevClimbing = climbing;
            }

            if (mantling != prevMantling)
            {
                animator.SetBool(HashMantling, mantling);
                prevMantling = mantling;
            }
        }

        void UpdateSpeedClimb(float hor, float ver)
        {
            if (ver > 0.1f) ver = 1;
            else if (ver < -0.1f) ver = -1;
            else ver = 0;
            animator.SetFloat(HashSpeedClimb, ver);
        }
    }
}
