using Raccoon.InputCtr;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Raccoon.Player
{

    public class PlayerMovement : MonoBehaviour
    {
        float multiSpeedRun = 0f;
        public float MultiSpeedRun => multiSpeedRun;

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
        [FormerlySerializedAs("m_ClimbSpeed")]
        public float climbSpeed = 1f;
        public float capsuleRadius = 1;

        [Header("Climp")]
        [Header("Raycast phát hiện thang")]
        [SerializeField] private LayerMask ladderLayer;
        [SerializeField] private float rayDistance = 0.6f;
        [Tooltip("Chiều cao ray kiểm tra ở ngực, dùng để bắt đầu leo thang")]
        [SerializeField] private float chestHeight = 1f;
        private bool ladderInRange;

        Rigidbody rb;
        PlayerInput input;
        Vector3 moveDir, camForward;

        Vector3 dirForward;
        Vector3 dirRight;
        bool isMoving;


        float turnAmount;
        float forwardAmount;
        float origGroundCheckDistance;

        [SerializeField]UnityEvent<float, float, bool> actionAnimMove;
        [SerializeField]UnityEvent jumpedAction;
        [SerializeField]UnityEvent jumpAction;

        public bool canMove = true;

        MoveAround moveAround;
        JumpHandle jumpHandle;

        private void Start()
        {
            canMove = true;
            input = new PlayerInput();
            origGroundCheckDistance = groundCheckDistance;
            moveAround = new MoveAround();
            jumpHandle = new JumpHandle(numberJump);
        }

        public void Init(Rigidbody rg, CapsuleCollider capsule)
        {
            this.rb = rg;
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
            actionAnimMove?.Invoke(forwardAmount, turnAmount, isGrounded);
        }
        private void FixedUpdate()
        {
            if (!canMove) input.Reset();

            CheckGroundStatus();
            CalculateMoveDir();

            CheckClamp();

            FixedUpdateImpl();
        }
        void CheckClamp()
        {
            RaycastHit hit;
            ladderInRange = CheckLadderRay(chestHeight, out hit);
            if (ladderInRange)
            {
                Debug.DrawRay(hit.point, hit.normal * 2, Color.red);
                float approachDot = Vector3.Dot(moveDir.normalized, -hit.normal);

                if (approachDot > 0.3f)
                {
                    // moveDir đang hướng VÀO thang -> cho leo
                    Debug.Log("clamp ");
                }
                else
                {
                    // moveDir hướng RA XA hoặc đi ngang qua thang -> đi bộ bình thường, không leo
                    Debug.Log("move out ");

                }
            }
        }


        private bool CheckLadderRay(float height, out RaycastHit hit)
        {
            Vector3 origin = transform.position + Vector3.up * height;
            Debug.DrawRay(origin, transform.forward * rayDistance, Color.cyan);
            return Physics.Raycast(origin, transform.forward, out hit, rayDistance, ladderLayer);
        }
        protected virtual void FixedUpdateImpl()
        {
            CheckIsMove();
            CalculateForwardAroundAndTurnAround(moveDir);

            moveAround.Move(rb, isMoving, moveDir, climbSpeed, multiSpeedRun, input.isRunning);
            Jump();
            ApplyExtraTurnRotation();

            UplineGravityInAir();

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
            if (GroundCheck())
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }
        protected virtual bool GroundCheck()
        {
            // 0.1f is a small offset to start the ray from inside the character
            // it is also good to note that the transform position in the sample assets is at the base of the character
            Vector3 pos = transform.position + (Vector3.up * 0.1f * transform.lossyScale.y);
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
            {
                return true;
            }
            pos.x += capsuleRadius;
            pos.z += capsuleRadius;
#if UNITY_EDITOR
            // heelper to visualise the ground check ray in the scne view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
                return true;
            pos.z -= capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
                return true;
            pos.x -= capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
                return true;
            pos.z += capsuleRadius * 2f;
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(pos, pos + (Vector3.down * groundCheckDistance), Color.yellow);
#endif
            if (Physics.Raycast(pos, Vector3.down, groundCheckDistance, layerCheckGround))
                return true;

            return false;
        }

        protected virtual void ApplyExtraTurnRotation()
        {
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * climbSpeed * Time.deltaTime, 0);
        }

        void UplineGravityInAir()
        {
            if (isGrounded) return;

            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier) * Time.fixedDeltaTime;

            groundCheckDistance = rb.linearVelocity.y < 0 ? origGroundCheckDistance : 0.1f;
        }
        void Jump()
        {
            if (input.isJump)
            {
                jumpHandle.Jump(rb, jumpPower,ActionJump);
            }
            if (isGrounded) jumpHandle.ResetJump();
            input.isJump = false;
        }

        void ActionJump()
        {
            isGrounded = false;
            groundCheckDistance = 0.1f;
            jumpAction?.Invoke();
        }


    }
}