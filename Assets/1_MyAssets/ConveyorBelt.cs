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
        [Tooltip("Tốc độ đẩy player (m/s) theo hướng forward (trục Z xanh dương) của platform này")]
        public float speed = 3f;

        // Vận tốc cộng thêm cho player mỗi FixedUpdate.
        public Vector3 ConveyorVelocity => transform.forward * speed;


        private void OnCollisionStay(Collision collision)
        {
            if(collision.transform == PlayerController.instance.transform)
            {

                PlayerController.instance.movement.SetExtendVelocity(new Vector3(ConveyorVelocity.x, 0, ConveyorVelocity.z));
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
            Vector3 dir = transform.forward * Mathf.Max(1f, speed);
            Gizmos.DrawLine(origin, origin + dir);
            Gizmos.DrawSphere(origin + dir, 0.1f);
        }
#endif
    }
}
