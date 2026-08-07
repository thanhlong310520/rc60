using System.Collections;
using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Class xử lý di chuyển nhân vật bằng CharacterController: đi/chạy, nhảy, gravity, grounded check,
    /// và leo thang (climb + mantle) chuyển từ PlayerMovement.cs (bản Rigidbody) sang dùng chung
    /// physic CharacterController của class này (không dùng Rigidbody).
    /// Hướng di chuyển được truyền vào từ bên ngoài qua thuộc tính MoveInput (Vector2).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SimpleMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Tốc độ đi bình thường (m/s)")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Tốc độ chạy (m/s)")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Độ mượt khi xoay nhân vật theo hướng di chuyển")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Tốc độ tăng/giảm gia tốc")]
        public float SpeedChangeRate = 10.0f;

        [Header("Gravity")]
        [Tooltip("Trọng lực riêng của nhân vật. Mặc định Unity là -9.81f")]
        public float Gravity = -15.0f;

        [Header("Jump")]
        [Tooltip("Độ cao nhân vật có thể nhảy")]
        public float JumpHeight = 1.2f;

        [Tooltip("Thời gian chờ trước khi có thể nhảy lại. Để 0 nếu muốn nhảy liên tục")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Thời gian chờ trước khi chuyển sang trạng thái rơi tự do. Hữu ích khi đi xuống cầu thang")]
        public float FallTimeout = 0.15f;

        [Tooltip("Số lần nhảy tối đa liên tiếp trước khi phải chạm đất lại. 1 = nhảy thường, 2 = double jump, 3+ = triple jump...")]
        public int MaxJumpCount = 1;

        [Header("Grounded Check")]
        [Tooltip("Nhân vật có đang chạm đất hay không (chỉ để đọc, không tự set từ ngoài)")]
        public bool Grounded = true;

        [Tooltip("Offset theo trục Y của vị trí check grounded, dùng cho địa hình gồ ghề")]
        public float GroundedOffset = -0.14f;

        [Tooltip("Bán kính của vùng check grounded, nên trùng với bán kính CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("Layer nào được tính là mặt đất")]
        public LayerMask GroundLayers;

        [Header("Input")]
        [Tooltip("Hướng di chuyển, gán từ script khác. x = ngang, y = dọc (giống input joystick/WASD)")]
        public Vector2 MoveInput;

        [Tooltip("Bật chạy (sprint) hay không, gán từ script khác")]
        public bool Sprint;

        [Tooltip("Set true để nhảy, gán từ script khác (giống nhấn nút Jump). Sẽ tự reset về false sau khi xử lý")]
        public bool Jump;

        [Tooltip("Nếu true, tốc độ sẽ nhân theo độ lớn của MoveInput (analog). Nếu false, luôn full tốc độ khi có input.")]
        public bool AnalogMovement = false;

        // camera để tính hướng di chuyển tương đối theo góc nhìn (có thể để null nếu không cần)
        public Transform CameraTransform;

        [Header("Moving Platform")]
        [Tooltip("Player tự xoay theo platform khi đứng trên (vd: vòng xoay, sàn xoay)")]
        public bool RotateWithPlatform = true;

        [Tooltip("Bật nếu platform còn di chuyển theo trục Y (thang máy) và muốn player theo luôn cả trục Y. " +
                 "Tắt nếu chỉ muốn gravity/jump tự quản lý trục Y (phù hợp cho sàn xoay đứng yên theo Y).")]
        public bool FollowPlatformVerticalMotion = false;

        // Rigidbody của platform player đang đứng lên (lấy từ GroundedCheck).
        // Platform cần có Rigidbody (nên để isKinematic = true) và được di chuyển bằng
        // rb.MovePosition()/rb.MoveRotation() thì GetPointVelocity()/angularVelocity mới chính xác.
        private Rigidbody _currentPlatformRb;

        [Header("Climb")]
        [Tooltip("Layer nào được tính là thang/tường leo được")]
        [SerializeField] private LayerMask ladderLayer;

        [Tooltip("Tốc độ leo thang (m/s)")]
        public float climbSpeed = 1f;

        [Tooltip("Khoảng cách raycast phát hiện thang")]
        [SerializeField] private float rayDistance = 0.6f;

        [Tooltip("Chiều cao ray kiểm tra ở ngực, dùng để bắt đầu leo thang")]
        [SerializeField] private float chestHeight = 1f;

        [Tooltip("Chiều cao ray kiểm tra ở đầu, dùng để xác định điểm kết thúc leo thang")]
        [SerializeField] private float overHeadHeight = 1.5f;

        [Tooltip("Thời gian tween lên đỉnh tường (mantle)")]
        public float mantleDuration = 0.4f;

        [Tooltip("Đang leo thang hay không (chỉ để đọc)")]
        public bool climbing;

        [Tooltip("Đang trèo lên đỉnh tường (mantle) hay không (chỉ để đọc)")]
        public bool isMantling;

        private Vector3 currentLadderNormal;
        private Vector3 moveDirWorld;

        private CharacterController _controller;
        private float _speed;
        private float _verticalVelocity;
        private float _targetRotation;
        private float _rotationVelocity;
        private const float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // số lần đã nhảy kể từ lần chạm đất gần nhất (dùng cho double/triple jump)
        private int _jumpsUsed;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (CameraTransform == null && Camera.main != null)
            {
                CameraTransform = Camera.main.transform;
            }

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            GroundedCheck();
            CalculateMoveDirWorld();
            CheckClimb();

            if (isMantling) return; // đang trèo lên đỉnh thì không xử lý input khác

            if (climbing)
            {
                HandleClimbing();
                CheckLedgeTop();
            }
            else
            {
                JumpAndGravity();
                Move();
            }
        }

        /// <summary>
        /// Gán hướng di chuyển từ bên ngoài. Có thể gọi thay cho việc set trực tiếp MoveInput.
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            MoveInput = input;
        }

        private void CalculateMoveDirWorld()
        {
            Vector3 inputDirection = new Vector3(MoveInput.x, 0.0f, MoveInput.y).normalized;
            float cameraYaw = CameraTransform != null ? CameraTransform.eulerAngles.y : 0.0f;
            moveDirWorld = Quaternion.Euler(0.0f, cameraYaw, 0.0f) * inputDirection;
        }

        private void Move()
        {
            float targetSpeed = Sprint ? SprintSpeed : MoveSpeed;

            if (MoveInput == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = AnalogMovement ? MoveInput.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 inputDirection = new Vector3(MoveInput.x, 0.0f, MoveInput.y).normalized;

            if (MoveInput != Vector2.zero)
            {
                float cameraYaw = CameraTransform != null ? CameraTransform.eulerAngles.y : 0.0f;

                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraYaw;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            ApplyMovingPlatform();
        }

        // Cộng vận tốc + xoay của platform (nếu player đang đứng trên 1 vật có Rigidbody) vào player.
        // Dùng GetPointVelocity nên đúng cả với platform tịnh tiến (xe, thang máy) lẫn xoay (vòng xoay).
        private void ApplyMovingPlatform()
        {
            if (_currentPlatformRb == null) return;

            Vector3 platformPointVelocity = _currentPlatformRb.GetPointVelocity(transform.position);
            if (!FollowPlatformVerticalMotion)
                platformPointVelocity.y = 0f;

            // CharacterController không có "velocity" để cộng dồn như Rigidbody,
            // nên áp dụng vận tốc platform bằng cách di chuyển thêm 1 đoạn = vận tốc * deltaTime
            _controller.Move(platformPointVelocity * Time.deltaTime);

            if (RotateWithPlatform)
            {
                // Chỉ lấy tốc độ xoay quanh trục Y để tránh player bị nghiêng theo platform
                float yawRateRad = Vector3.Dot(_currentPlatformRb.angularVelocity, Vector3.up);
                float yawDegrees = yawRateRad * Mathf.Rad2Deg * Time.deltaTime;
                if (!Mathf.Approximately(yawDegrees, 0f))
                    transform.Rotate(Vector3.up, yawDegrees, Space.World);
            }
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

            // dùng OverlapSphere thay vì CheckSphere để lấy được collider/Rigidbody của mặt đất/platform
            Collider[] groundHits = Physics.OverlapSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            Grounded = groundHits.Length > 0;
            _currentPlatformRb = Grounded ? groundHits[0].attachedRigidbody : null;
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset timeout rơi
                _fallTimeoutDelta = FallTimeout;

                // vừa chạm đất -> reset lại số lần nhảy đã dùng (double/triple jump có lại từ đầu)
                _jumpsUsed = 0;

                // không cho vận tốc dọc giảm vô hạn khi đang đứng trên đất
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // nhảy lần đầu (từ mặt đất)
                if (Jump && _jumpTimeoutDelta <= 0.0f)
                {
                    DoJump();
                }

                // timeout giữa 2 lần nhảy
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset timeout nhảy khi đang ở trên không (để nhảy tiếp không bị dính timeout của lần trước)
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }

                // nhảy thêm trên không (double/triple jump), miễn còn lượt và đã hết timeout
                if (Jump && _jumpsUsed < MaxJumpCount && _jumpTimeoutDelta <= 0.0f)
                {
                    DoJump();
                }
            }

            // luôn tự reset input Jump sau khi xử lý xong (giống hành vi 1 lần bấm = 1 lần nhảy)
            Jump = false;

            // áp dụng trọng lực theo thời gian nếu chưa đạt vận tốc tối đa
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void DoJump()
        {
            // v = căn(2 * g * h)
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            _jumpsUsed++;
            _jumpTimeoutDelta = JumpTimeout;
        }

        #region Climb

        private void CheckClimb()
        {
            RaycastHit hit;
            bool ladderInRange = CheckLadderRay(chestHeight, out hit);

            if (ladderInRange)
            {
                if (Grounded)
                {
                    // chỉ bắt đầu leo nếu nhân vật đang đi thẳng về phía thang (không phải đi ngang qua)
                    float approachDot = Vector3.Dot(moveDirWorld.normalized, -hit.normal);
                    climbing = approachDot > 0.3f;
                }
                else
                {
                    // đang lơ lửng mà chạm thang -> bám vào leo luôn
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

        private bool CheckLadderRay(float height, out RaycastHit hit)
        {
            Vector3 origin = transform.position + Vector3.up * height;
            return Physics.Raycast(origin, transform.forward, out hit, rayDistance, ladderLayer);
        }

        private void StartClimbing(RaycastHit ladderHit)
        {
            currentLadderNormal = ladderHit.normal;
            transform.rotation = Quaternion.LookRotation(-currentLadderNormal, Vector3.up);

            // reset vận tốc rơi để không bị "giật" khi vừa bám vào thang
            _verticalVelocity = 0.0f;
        }

        private void HandleClimbing()
        {
            // mặt luôn hướng vào thang, không xoay theo input nữa
            transform.rotation = Quaternion.LookRotation(-currentLadderNormal, Vector3.up);

            float v = MoveInput.y;
            if (v > 0.1f) v = 1f;
            else if (v < -0.1f) v = -1f;
            else v = 0f;

            Vector3 climbMove = Vector3.up * v * climbSpeed * Time.deltaTime;
            _controller.Move(climbMove);
        }

        private void CheckLedgeTop()
        {
            // bước 1: kiểm tra phía trên còn tường chặn không
            RaycastHit hit;
            bool wallAboveBlocked = CheckLadderRay(overHeadHeight, out hit);

            if (wallAboveBlocked) return; // vẫn còn tường phía trên -> chưa tới đỉnh

            // bước 2: nếu phía trên không còn tường, thử tìm mặt sàn ở phía trước-trên
            Vector3 downOrigin = transform.position + Vector3.up * overHeadHeight + transform.forward * overHeadHeight + Vector3.up * 0.5f;
            RaycastHit floorHit;
            bool foundFloor = Physics.Raycast(downOrigin, Vector3.down, out floorHit, 1.5f, GroundLayers);

            if (foundFloor)
            {
                // đã xác định được điểm đứng trên đỉnh tường -> bắt đầu trèo lên
                StartCoroutine(Mantle(floorHit.point));
            }
        }

        private IEnumerator Mantle(Vector3 ledgePoint)
        {
            isMantling = true;
            climbing = false;

            // tắt CharacterController trong lúc tween để tự set transform.position mà không bị va chạm cản lại
            _controller.enabled = false;

            Vector3 startPos = transform.position;
            Vector3 endPos = ledgePoint + Vector3.up * (_controller.height * 0.5f + 0.05f);

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
            _controller.enabled = true;
            isMantling = false;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }
    }
}