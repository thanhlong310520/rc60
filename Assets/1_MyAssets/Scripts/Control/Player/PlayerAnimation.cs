using System.Collections.Generic;
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
    public class PlayerAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<SkinGO> skinGo;
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
        private static readonly int IsFalling = Animator.StringToHash("IsFalling");
        private static readonly int HashJump = Animator.StringToHash("Jump");
        private static readonly int HashClimbing = Animator.StringToHash("IsClimb");
        private static readonly int HashMantling = Animator.StringToHash("Mantling");
        private static readonly int HashSpeedMultiplier = Animator.StringToHash("SpeedMultiplier");
        private static readonly int HashSpeedClimb = Animator.StringToHash("SpeedClimb");
        private static readonly int HashDance = Animator.StringToHash("Dance");

        private bool prevClimbing;
        private bool prevMantling;

        private void Reset()
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
            
        }

        private void Awake()
        {
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
        public void OnAnimMove(float forwardAmount, float turnAmount, bool isGrounded, bool isFalling)
        {
            if (skinGo == null) return;

            SetFloatAnim(HashForward, forwardAmount, moveDampTime, Time.deltaTime);
            SetFloatAnim(HashTurn, turnAmount, moveDampTime, Time.deltaTime);
            SetBoolAnim(HashGrounded, isGrounded);
            SetBoolAnim(IsFalling, isFalling);

            if (playerMovement != null)
            {
                float speed = 1;
                if (playerMovement.IsRunning) speed += playerMovement.MultiSpeedRun * 0.1f;

                // 1 + multiSpeedRun để Animator có thể dùng làm hệ số tăng tốc clip chạy (Animator.speed hoặc blend)
                SetFloatAnim(HashSpeedMultiplier, speed);
            }
        }

        /// <summary>
        /// Gọi từ UnityEvent jumpAction của PlayerMovement (bắn ra ngay lúc bắt đầu nhảy).
        /// </summary>
        public void OnJumpStart()
        {
            if (skinGo == null) return;
            ResetTriggerAnim(HashJump);
            SetTriggerAnim(HashJump);
        }


        /// <summary>
        /// Gọi từ UnityEvent jumpedAction của PlayerMovement (nếu bạn dùng event này để báo hiệu đã tiếp đất).
        /// </summary>
        public void OnLanded()
        {
            if (skinGo == null) return;
            ResetTriggerAnim(HashJump);
        }

        /// <summary>
        /// climbing và isMantling là field public trên PlayerMovement (không có event riêng)
        /// nên được poll trực tiếp mỗi frame ở đây.
        /// </summary>
        private void UpdateClimbAndMantleState()
        {
            if (playerMovement == null || skinGo == null) return;

            bool climbing = playerMovement.climbing;
            bool mantling = playerMovement.isMantling;

            if (climbing != prevClimbing)
            {
                SetBoolAnim(HashClimbing, climbing);
                prevClimbing = climbing;
            }

            if (mantling != prevMantling)
            {
                SetBoolAnim(HashMantling, mantling);
                prevMantling = mantling;
            }
        }

        public void OnDance(bool isDance)
        {
            SetBoolAnim(HashDance, isDance);
        }

        void UpdateSpeedClimb(float hor, float ver)
        {
            if (ver > 0.1f) ver = 1;
            else if (ver < -0.1f) ver = -1;
            else ver = 0;
            SetFloatAnim(HashSpeedClimb, ver);
        }

        public void SetFloatAnim(int paramName, float value)
        {
            foreach (var a in skinGo)
            {
                if (a == null) return;
                a.animator.SetFloat(paramName, value);
            }
        }
        public void SetFloatAnim(int paramName, float value, float dampTime, float deltaTime)
        {
            foreach (var a in skinGo)
            {
                if (a == null) return;
                a.animator.SetFloat(paramName, value, dampTime, deltaTime);
            }
        }
        public void SetBoolAnim(int paramName, bool value)
        {
            foreach (var a in skinGo)
            {
                if (a == null) return;
                a.animator.SetBool(paramName, value);
            }
        }
        public void SetTriggerAnim(int paramName)
        {
            foreach (var a in skinGo)
            {
                if (a == null) return;
                a.animator.SetTrigger(paramName);
            }
        }
        public void ResetTriggerAnim(int paramName)
        {
            foreach (var a in skinGo)
            {
                if (a == null) return;
                a.animator.ResetTrigger(paramName);
            }
        }

        public void SetSkin(GameObject go)
        {
            if(skinGo == null) skinGo = new List<SkinGO>();

            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var s = go.GetComponent<SkinGO>();

            if (s != null)
            {
                skinGo.Add(s);
            }
        }
    }
}
