using Raccoon.Controller;
using UnityEngine;

namespace Raccoon.Player
{
    // Gắn component này lên platform băng chuyền (platform đứng yên, KHÔNG cần Rigidbody
    // di chuyển bằng MovePosition như moving platform bình thường).
    // Player đứng lên sẽ được PlayerMovement cộng thêm vận tốc theo hướng forward
    // của chính GameObject này (xem ApplyConveyor() trong PlayerMovement.cs).
    public class ConveyorBelt : MonoBehaviour
    {
        public enum Axis
        {
            Forward,   // trục Z (xanh dương)
            Right,     // trục X (đỏ)
            Up         // trục Y (xanh lá)
        }

        [Tooltip("Trục local của platform dùng làm hướng đẩy")]
        public Axis pushAxis = Axis.Forward;

        [Tooltip("Đảo ngược hướng đẩy (ví dụ: -forward, -right...)")]
        public bool invert = false;

        [Tooltip("Tốc độ đẩy player (m/s)")]
        public float speed = 3f;

        public Vector3 ConveyorVelocity
        {
            get
            {
                Vector3 dir = pushAxis switch
                {
                    Axis.Forward => transform.forward,
                    Axis.Right => transform.right,
                    Axis.Up => transform.up,
                    _ => transform.forward
                };
                if (invert) dir = -dir;
                return dir * speed;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.transform == PlayerController.instance.transform)
            {
                PlayerController.instance.movement.SetExtendVelocity(
                    new Vector3(ConveyorVelocity.x, 0, ConveyorVelocity.z));
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.transform == PlayerController.instance.transform)
            {
                PlayerController.instance.movement.SetExtendVelocity(Vector3.zero);
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Vẽ mũi tên hướng đẩy trong Scene view cho dễ setup
            Gizmos.color = Color.cyan;
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            Vector3 dir = ConveyorVelocity.normalized * Mathf.Max(1f, speed);
            if (invert) dir = -dir;
            Gizmos.DrawLine(origin, origin + dir);
            Gizmos.DrawSphere(origin + dir, 0.1f);
        }
#endif
    }
}
