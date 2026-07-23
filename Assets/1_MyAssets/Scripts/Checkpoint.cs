using Raccoon.Controller;
using UnityEngine;

/// <summary>
/// Gắn component này lên từng GameObject checkpoint trong scene
/// (object cần có Collider và bật Is Trigger).
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Checkpoint : MonoBehaviour
{
    [Header("Thông tin checkpoint")]
    public string id;
    public bool isEndPoint; // true nếu là điểm cuối (win game)

    public MapController mapController;


    public void Init(MapController mapController)
    {
        this.mapController = mapController;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == PlayerController.instance.transform)
        {
            mapController.OnPlayerHitCheckpoint(this);
        }
    }
}
