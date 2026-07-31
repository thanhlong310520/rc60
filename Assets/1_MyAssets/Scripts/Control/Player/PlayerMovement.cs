using Raccoon.InputCtr;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Raccoon.Player
{

    public class PlayerMovement : MonoBehaviour
    {
        public float multiSpeedRun = 1f;
        public float MultiSpeedRun => multiSpeedRun;
        public float moveSpeed = 1f;

        public int numberJump = 2;
        [FormerlySerializedAs("m_JumpPower")]
        public float jumpPower = 12f;
        [FormerlySerializedAs("m_MovingTurnSpeed")]
        public float movingTurnSpeed = 360;
        [FormerlySerializedAs("m_StationaryTurnSpeed")]
        public float stationaryTurnSpeed = 180;
        [Tooltip("Player is on ground.")]
        public bool isGrounded;
        public LayerMask layerCheckGround;
        [FormerlySerializedAs("m_GravityMultiplier")]
        [Range(1f, 4f)] public float gravityMultiplier = 2f;

        [FormerlySerializedAs("m_GroundCheckDistance")]
        public float groundCheckDistance = 0.1f;
        [Tooltip("Góc dốc tối đa (độ) được tính là mặt đất đi được. Lớn hơn góc này coi như tường, không tính grounded.")]
        public float maxSlopeAngle = 55f;
        [Tooltip("Bán kính SphereCast dùng để check ground = capsuleRadius * hệ số này (nên < 1 để tránh quẹt tường sát bên).")]
        [Range(0.1f, 1f)] public float groundCheckRadiusMultiplier = 0.9f;
        private Vector3 groundNormal = Vector3.up;
        public Vector3 GroundNormal => groundNormal;
        [FormerlySerializedAs("m_ClimbSpeed")]
        public float climbSpeed = 1f;
        public float capsuleRadius = 1;

        [Header("Climp")]
        [Header("Raycast phát hiện thang")]
        [SerializeField] private LayerMask ladderLayer;
        [SerializeField] private float rayDistance = 0.6f;
        [Tooltip("Chiều cao ray kiểm tra ở ngực, dùng để bắt đầu leo thang")]
        [SerializeField] private float chestHeight = 1f;
        [Tooltip("Chiều cao ray kiểm tra ở đầu, dùng để end leo thang")]
        [SerializeField] private float overHeadHeight = 1.5f;
        public float mantleDuration = 0.4f;     // thời gian tween lên đỉnh tường


        private bool ladderInRange;

        public bool climbing;
        public bool isMantling;
        Rigidbody rb;
        CapsuleCollider capsuleCollider;
        PlayerInput input;
        Vector3 moveDir, camForward;

        Vector3 dirForward;
        Vector3 dirRight;
        bool isMoving;
        public bool IsRunning;

        float turnAmount;
        float forwardAmount;

        public float TurnAmount => turnAmount;
        public float ForwardAmount => forwardAmount;

        float origGroundCheckDistance;

        [SerializeField] public UnityEvent<float, float, bool,bool> actionAnimMove;
        [SerializeField] public UnityEvent jumpedAction;
        [SerializeField] public UnityEvent jumpAction;
        [SerializeField] public UnityEvent<float, float> climbAction;

        bool canMove = false;

        [Header("Moving Platform")]
        [Tooltip("Player tự xoay theo platform khi đứng trên (vd: vòng xoay, sàn xoay)")]
        [SerializeField] private bool rotateWithPlatform = true;
        [Tooltip("Bật nếu platform còn di chuyển theo trục Y (thang máy) và muốn player theo luôn cả trục Y. " +
                 "Tắt nếu chỉ muốn gravity/jump tự quản lý trục Y (phù hợp cho sàn xoay đứng yên theo Y).")]
        [SerializeField] private bool followPlatformVerticalMotion = false;

        // Rigidbody của platform player đang đứng lên (lấy từ raycast ground check).
        // Platform cần có Rigidbody (nên để isKinematic = true) và được di chuyển bằng
        // rb.MovePosition()/rb.MoveRotation() thì GetPointVelocity()/angularVelocity mới chính xác.
        private Rigidbody currentPlatformRb;

        MoveAround moveAround;
        JumpHandle jumpHandle;

        private void Start()
        {
            isMantling = false;
            input = new PlayerInput();
            origGroundCheckDistance = groundCheckDistance;
            moveAround = new MoveAround();
            jumpHandle = new JumpHandle(numberJump);
        }

        public void Init(Rigidbody rg, CapsuleCollider capsule)
        {
            this.rb = rg;
            capsuleCollider = capsule;
            capsuleRadius = capsule.radius * transform.lossyScale.x;
        }

        public void SetMultiSpeedRun(float set)
        {
            multiSpeedRun = set;
        }

        public void SetCanMove(bool canmove)
        {
            canMove = canmove;
        }
        public void SetInput(PlayerInput input)
        {
            this.input = input;
            IsRunning = input.isRunning;

        }
        public void SetDirFoward(Vector3 dirForward)
        {
            this.dirForward = dirForward;
        }
        public void SetDirRight(Vector3 dirRight)
        {
            this.dirRight = dirRight;
        }
        private void Update()
        {
            actionAnimMove?.Invoke(forwardAmount, turnAmount, isGrounded, isFalling);
            if (!canMove)
            {
                ResetMove();
                input.Reset();
                return;
            }

            CheckGroundStatus();
            CalculateMoveDir();
            CheckClimb();

        }
        private void FixedUpdate()
        {
            if (!canMove)
            {
                ResetMove();
                input.Reset();
                return;
            }

            if (isMantling) return; // đang trèo lên đỉnh thì không xử lý input khác
            if (climbing)
            {
                ResetMove();
                HandleClimbing();
                CheckLedgeTop();
            }
            else
            {
                FixedUpdateImpl();
            }
            CheckFalling();
        }

        #region Climb
        void CheckClimb()
        {
            RaycastHit hit;
            ladderInRange = CheckLadderRay(chestHeight, out hit);
            if (ladderInRange)
            {
                Debug.DrawRay(hit.point, hit.normal * 2, Color.red);
                if (isGrounded)
                {
                    float approachDot = Vector3.Dot(moveDir.normalized, -hit.normal);

                    if (approachDot > 0.3f)
                    {
                        climbing = true;

                    }
                    else
                    {
                        climbing = false;
                    }
                }
                else
                {
                    climbing = true;
                }

            }
            else
            {
                climbing = false;
            }
            if (climbing)
            {
                StartClimbing(hit);
            }


        }

        private Vector3 currentLadderNormal;
        private Vector3 currentLadderRight; // tính 1 lần khi StartClimbing, lưu lại để không đổi giữa chừng

        private void StartClimbing(RaycastHit ladderHit)
        {

            if (currentPlatformRb == null) currentPlatformRb = ladderHit.rigidbody;
            currentLadderNormal = ladderHit.normal;
            currentLadderRight = Vector3.Cross(Vector3.up, currentLadderNormal).normalized;

            transform.rotation = Quaternion.LookRotation(-currentLadderNormal, Vector3.up);

        }
        private void HandleClimbing()
        {
            // Mặt luôn hướng vào thang, không xoay theo input nữa
            transform.rotation = Quaternion.LookRotation(-currentLadderNormal, Vector3.up);

            float h = input.horizontalAxis;
            float v = input.verticalAxis;

            if (v > 0.1f) v = 1;
            else if (v < -0.1f) v = -1;
            else v = 0;

            // Không qua camera/moveDir nữa: dùng thẳng input làm 2 trục leo
            float verticalInput = v;   // >0: leo lên, <0: leo xuống
            //float horizontalInput = -h; // >0: qua phải, <0: qua trái
            float horizontalInput = 0;




            //// Chặn shimmy đi lố khỏi mép thang (trái/phải)
            //if (Mathf.Abs(horizontalInput) > 0.01f)
            //{
            //    Vector3 sideOrigin = rb.position + Vector3.up * chestHeight;
            //    Vector3 sideDir = horizontalInput > 0f ? currentLadderRight : -currentLadderRight;

            //    RaycastHit sideHit;
            //    bool sideHasLadder = Physics.Raycast(sideOrigin + sideDir * sideCheckOffset,
            //                                          -currentLadderNormal, out sideHit, rayDistance, ladderLayer);
            //    if (!sideHasLadder) horizontalInput = 0f;
            //}

            Vector3 climbMove = (Vector3.up * verticalInput + currentLadderRight * horizontalInput)
                                 * climbSpeed * Time.deltaTime;

            Vector3 targetPos = rb.position + climbMove;

            climbAction?.Invoke(horizontalInput, verticalInput);
            rb.MovePosition(targetPos);
        }

        void ResetMove()
        {
            rb.linearVelocity = Vector3.zero;
        }
        private bool CheckLadderRay(float height, out RaycastHit hit)
        {
            Vector3 origin = transform.position + Vector3.up * height;
            Debug.DrawRay(origin, transform.forward * rayDistance, Color.cyan);
            return Physics.Raycast(origin, transform.forward, out hit, rayDistance, ladderLayer);
        }

        void CheckLedgeTop()
        {


            // Bước 1: kiểm tra phía trên còn tường chặn không
            RaycastHit hit;
            bool wallAboveBlocked = CheckLadderRay(overHeadHeight, out hit);

            if (wallAboveBlocked) return; // vẫn còn tường phía trên -> chưa tới đỉnh

            // Bước 2: nếu phía trên không còn tường, thử tìm mặt sàn ở phía trước-trên
            Vector3 downOrigin = transform.position + Vector3.up * overHeadHeight + transform.forward * overHeadHeight + Vector3.up * 0.5f;
            RaycastHit floorHit;
            bool foundFloor = Physics.Raycast(downOrigin, Vector3.down, out floorHit, 1.5f, layerCheckGround);

            Debug.DrawRay(downOrigin, Vector3.down * 1.5f, foundFloor ? Color.blue : Color.magenta);

            if (foundFloor)
            {
                // Đã xác định được điểm đứng trên đỉnh tường -> bắt đầu trèo lên
                StartCoroutine(Mantle(floorHit.point));
            }
        }

        IEnumerator Mantle(Vector3 ledgePoint)
        {
            isMantling = true;
            climbing = false;
            capsuleCollider.isTrigger = true;
            ResetMove();
            Vector3 startPos = transform.position;
            // điểm đích: đứng đúng bề mặt đỉnh tường, cộng thêm chiều cao capsule/2
            Vector3 endPos = ledgePoint + Vector3.up * (capsuleCollider.height * 0.25f + 0.05f);

            float elapsed = 0f;
            while (elapsed < mantleDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / mantleDuration;

                // có thể thay bằng animation curve nếu muốn mượt hơn
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;
            capsuleCollider.isTrigger = false;
            isMantling = false;
        }
        #endregion
        protected virtual void FixedUpdateImpl()
        {
            CheckIsMove();
            CalculateForwardAroundAndTurnAround(moveDir);

            // Khi đang đứng trên mặt dốc, chiếu hướng di chuyển lên mặt phẳng dốc (theo groundNormal)
            // thay vì giữ nguyên vector di chuyển nằm ngang. Nếu không làm vậy, trên dốc nghiêng,
            // vector di chuyển ngang sẽ "đâm" vào mặt dốc, khiến va chạm liên tục đẩy player
            // lên/xuống -> đi giật, nảy nhẹ hoặc mất grounded ngay khi vừa bắt đầu leo dốc.
            Vector3 slopeAdjustedMoveDir = moveDir;
            if (isGrounded && moveDir.sqrMagnitude > 0.0001f)
            {
                Vector3 projected = Vector3.ProjectOnPlane(moveDir, groundNormal);
                if (projected.sqrMagnitude > 0.0001f)
                    slopeAdjustedMoveDir = projected.normalized * moveDir.magnitude;
            }


            ApplyMovingPlatform();

            moveAround.Move(rb, isMoving, slopeAdjustedMoveDir, moveSpeed, multiSpeedRun, input.isRunning, GetExtendVelocity());

            ApplyExtraTurnRotation();

            UplineGravityInAir();

            Jump();

        }
        Vector3 GetExtendVelocity()
        {
            return platformPointVelocity + extendVelocity;
        }
        // Cộng vận tốc + xoay của platform (nếu player đang đứng trên 1 vật có Rigidbody) vào player.
        // Dùng GetPointVelocity nên đúng cả với platform tịnh tiến (xe, thang máy) lẫn xoay (vòng xoay).

        Vector3 extendVelocity = Vector3.zero;
        public void SetExtendVelocity(Vector3 extendV)
        {
            extendVelocity = extendV;
        }
        Vector3 platformPointVelocity;
        void ApplyMovingPlatform()
        {
            if (currentPlatformRb == null)
            {
                platformPointVelocity = Vector3.zero;
                return;
            }

            platformPointVelocity = currentPlatformRb.GetPointVelocity(rb.position);
            if (!followPlatformVerticalMotion)
            {
                platformPointVelocity.y = 0f;
            }

            if (rotateWithPlatform)
            {
                // Chỉ lấy tốc độ xoay quanh trục Y để tránh player bị nghiêng theo platform
                float yawRateRad = Vector3.Dot(currentPlatformRb.angularVelocity, Vector3.up);
                float yawDegrees = yawRateRad * Mathf.Rad2Deg * Time.deltaTime;
                if (!Mathf.Approximately(yawDegrees, 0f))
                    transform.Rotate(Vector3.up, yawDegrees, Space.World);
            }
        }

        void CheckIsMove()
        {
            if (moveDir.sqrMagnitude > 0)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }

        void CalculateMoveDir()
        {
            float h = input.horizontalAxis;
            float v = input.verticalAxis;

            camForward = Vector3.Scale(dirForward, new Vector3(1, 0, 1)).normalized;
            moveDir = v * camForward + h * dirRight;

        }

        public virtual void CalculateForwardAroundAndTurnAround(Vector3 move)
        {
            if (move.magnitude > 1f)
                move.Normalize();
            move = transform.InverseTransformDirection(move);
            move = Vector3.ProjectOnPlane(move, Vector3.up);
            turnAmount = Mathf.Atan2(move.x, move.z);
            forwardAmount = move.z;

        }

        protected virtual void CheckGroundStatus()
        {
            RaycastHit hit;
            if (GroundCheck(out hit))
            {
                isGrounded = true;
                currentPlatformRb = hit.rigidbody;
            }
            else
            {
                isGrounded = false;
                currentPlatformRb = null;
            }
        }
        protected virtual bool GroundCheck(out RaycastHit hit)
        {
            // ---- 1) SphereCast chính: bắt được cả mặt dốc/nghiêng, không chỉ raycast thẳng đứng ----
            // Xuất phát cao hơn 1 chút (bù capsuleRadius) để sphere không kẹt bên trong dốc ngay từ đầu,
            // rồi quét xuống 1 khoảng đủ dài để phủ cả phần offset đó.
            float startHeight = capsuleRadius + 0.05f;
            Vector3 sphereOrigin = transform.position + Vector3.up * startHeight * transform.lossyScale.y;
            float sphereRadius = capsuleRadius * groundCheckRadiusMultiplier;
            float castDistance = startHeight + groundCheckDistance;

#if UNITY_EDITOR
            Debug.DrawLine(sphereOrigin, sphereOrigin + Vector3.down * castDistance, Color.green);
#endif
            if (Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out hit, castDistance, layerCheckGround))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle <= maxSlopeAngle)
                {
                    groundNormal = hit.normal;
                    return true;
                }
                // Dốc quá gắt (coi như tường) -> không tính là ground, rơi xuống fallback/return false bên dưới
            }

            // ---- 2) Fallback: raycast 5 điểm cũ, hữu ích cho mép bàn/step nhỏ mà SphereCast có thể bỏ sót ----
            // 0.1f is a small offset to start the ray from inside the character
            // it is also good to note that the transform position in the sample assets is at the base of the character
            Vector3 pos = transform.position + (Vector3.up * 0.1f * transform.lossyScale.y);
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, out hit, groundCheckDistance, layerCheckGround))
            {
                groundNormal = hit.normal;
                return true;
            }
            pos.x += capsuleRadius;
            pos.z += capsuleRadius;
#if UNITY_EDITOR
            // heelper to visualise the ground check ray in the scne view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, out hit, groundCheckDistance, layerCheckGround))
            {
                groundNormal = hit.normal;
                return true;
            }
            pos.z -= capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, out hit, groundCheckDistance, layerCheckGround))
            {
                groundNormal = hit.normal;
                return true;
            }
            pos.x -= capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, out hit, groundCheckDistance, layerCheckGround))
            {
                groundNormal = hit.normal;
                return true;
            }
            pos.z += capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, out hit, groundCheckDistance, layerCheckGround))
            {
                groundNormal = hit.normal;
                return true;
            }

            groundNormal = Vector3.up;
            return false;
        }

        protected virtual void ApplyExtraTurnRotation()
        {
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * moveSpeed * Time.deltaTime, 0);
        }

        void UplineGravityInAir()
        {
            if (isGrounded)
            {
                //rb.linearVelocity += Vector3.up * (-9.8f) * Time.deltaTime;
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier) * Time.fixedDeltaTime;
                return;
            }

            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier) * Time.fixedDeltaTime;
            groundCheckDistance = rb.linearVelocity.y < 0 ? origGroundCheckDistance : 0.1f;
        }
        void Jump()
        {
            if (input.isJump)
            {
                jumpHandle.Jump(rb, isGrounded, jumpPower, ActionJump);
                isGrounded = false;
            }
            if (isGrounded && rb.linearVelocity.y < 0) jumpHandle.ResetJump();
            input.isJump = false;
        }

        void ActionJump()
        {
            isGrounded = false;
            groundCheckDistance = 0.1f;
            jumpAction?.Invoke();
        }

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;
        // timeout deltatime
        private float _fallTimeoutDelta;
        bool isFalling;
        public bool IsFalling => isFalling;
        void CheckFalling()
        {
            if (isGrounded || climbing || isMantling)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;
                isFalling = false;
            }
            else
            {

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    isFalling = true;
                }
            }
        }

    }
}