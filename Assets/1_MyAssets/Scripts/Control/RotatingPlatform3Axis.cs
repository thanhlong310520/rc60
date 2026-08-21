using UnityEngine;

namespace Raccoon.Environment
{
    /// <summary>
    /// Platform xoay liên tục, tốc độ độc lập theo cả 3 trục X/Y/Z (độ/giây).
    /// Dùng Rigidbody kinematic + MoveRotation để player đứng trên có thể đọc đúng
    /// tốc độ xoay qua Rigidbody.angularVelocity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RotatingPlatform3Axis : MonoBehaviour
    {
        [Header("Rotation Speed (độ/giây) theo từng trục")]
        [Tooltip("VD: (0, 90, 0) chỉ xoay quanh Y giống bản gốc. (30, 60, 0) xoay đồng thời 2 trục.")]
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);

        [Header("Space")]
        [Tooltip("Self: xoay theo hệ trục cục bộ hiện tại của object (giống transform.Rotate(.., Space.Self)).\n" +
                 "World: xoay quanh 3 trục cố định của thế giới.")]
        [SerializeField] private Space rotationSpace = Space.Self;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void FixedUpdate()
        {
            Vector3 deltaEuler = rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(deltaEuler);

            // Tương đương transform.Rotate(deltaEuler, rotationSpace) nhưng an toàn cho Rigidbody kinematic
            Quaternion newRotation = rotationSpace == Space.Self
                ? rb.rotation * deltaRotation
                : deltaRotation * rb.rotation;

            rb.MoveRotation(newRotation);
        }
    }
}
