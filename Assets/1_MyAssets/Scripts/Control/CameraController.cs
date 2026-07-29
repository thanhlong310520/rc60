using Raccoon.InputCtr;
using Raccoon.Utils;
using UnityEngine;
using UnityEngine.InputSystem.HID;

namespace Raccoon.Controller
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        float cameraX, cameraY;
        float zoomStartTime;
        float wheel = 0;
        float capsuleHeight = 1;
        public Transform target;
        [Header("Camera")]
        public Camera m_Camera;
        public Vector2 cameraFixedRotationAngles;
        public float cameraDistance = 5f;
        public float cameraMinDistance = 3f;
        public float cameraMaxDistance = 20f;
        public float cameraZoomMultiplier = 10f;
        public float cameraZoomDuration = 0.9f;
        public float cameraXSpeed = 2f;
        public float cameraYSpeed = 2f;
        public float cameraYMinLimit = -20f;
        public float cameraYMaxLimit = 80f;
        public float cameraOrthoMinSize = 1;
        public float cameraOrthoMaxSize = 20;
        public float cameraOrthoDistance = 150;
        public bool avoidObstacles;
        public Vector3 targetOffset = new Vector3(0, 1f, 0); // lệch lên đầu
        public Vector3 cameraOffset = Vector3.zero; // lệch camera (trái/phải/sau)

        CameraInput input;

        public LayerMask obstacleLayer;


        public CameraShake camShake;

        private void Start()
        {
            input = new CameraInput();
            EventBus.Subscribe<CameraInput>(UpdateInput);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<CameraInput>(UpdateInput);
        }
        

        private void LateUpdate()
        {
            UpdateCamera(true);
        }
        protected virtual void UpdateCamera(bool smooth)
        {
            float oldCameraX = cameraX;
            float oldCameraY = cameraY;

            if (input != null)
            {
                float w = input.mouseScrollWheel * cameraZoomMultiplier;
                if (w != 0)
                {
                    zoomStartTime = Time.time;
                    wheel += w;
                }
            }
            wheel *= 0.9f;
            if (wheel < 0.001f && wheel > -0.001f)
            {
                wheel = 0;
            }

            Quaternion rotation;
            cameraX += input.mouseX * cameraXSpeed;
            cameraX = cameraX % 360;
            cameraY -= input.mouseY * cameraYSpeed;
            cameraY = ClampAngle(cameraY, cameraYMinLimit, cameraYMaxLimit);
            
            if (cameraFixedRotationAngles.x != 0)
            {
                cameraY = cameraFixedRotationAngles.x;
            }
            if (cameraFixedRotationAngles.y != 0)
            {
                cameraX = cameraFixedRotationAngles.y;
            }
            rotation = Quaternion.Euler(cameraY, cameraX, 0);
            Vector3 targetPos = target.position + targetOffset;
            Vector3 position;
            // orthographic support
            if (m_Camera.orthographic)
            {
                float newSize = Mathf.Lerp(m_Camera.orthographicSize, m_Camera.orthographicSize + wheel, Time.deltaTime);
                newSize = Mathf.Clamp(newSize, cameraOrthoMinSize, cameraOrthoMaxSize);
                m_Camera.orthographicSize = newSize;
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -cameraOrthoDistance);
                position = rotation * negDistance + targetPos;
                position += rotation * cameraOffset;
            }
            else
            {
                cameraDistance += wheel;
                float distance = Vector3.Distance(targetPos, transform.position);
                Vector3 direction = (targetPos - transform.position) / distance;
                cameraDistance = Mathf.Clamp(cameraDistance, cameraMinDistance, cameraMaxDistance);

                Vector3 negDistance = new Vector3(0.0f, 0.0f, -cameraDistance);
                position = rotation * negDistance + targetPos;
                position += rotation * cameraOffset;
                // check there's no voxel under camera to avoid clipping with ground
                Vector3 pos = position;
                pos.y -= 0.25f;

            }

            Vector3 dir = position - targetPos;

            float dis = GetDistanceObstacle(targetPos, dir, dir.magnitude);
            position = position.GetPoint(dir * (-1), dis);

            // move camera
            if (smooth)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, cameraZoomDuration);
                transform.position = Vector3.Lerp(transform.position, position, cameraZoomDuration);
            }
            else
            {
                transform.rotation = rotation;
                transform.position = position;
            }
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }


        float GetDistanceObstacle(Vector3 originPos, Vector3 dir, float maxdir)
        {
            RaycastHit hitInfo;
            var onHit = Physics.Raycast(originPos, dir.normalized, out hitInfo, maxdir, obstacleLayer);
            if (!onHit) return 0;
            return maxdir - hitInfo.distance + 0.2f;
        }

        public void UpdateInput(CameraInput input)
        {
            this.input = input;

        }

        public void SetTarget(Vector3 offset)
        {
            cameraOffset = offset;
        }
    }
}