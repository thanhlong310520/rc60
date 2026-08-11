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
    public Transform pointPlayerStay;

    public MeshRenderer meshRenderer;
    public Material matRed;
    public Material matGreen;
    public MapController mapController;


    public Transform GetPointPlayerStay()
    {
        if (pointPlayerStay != null)
            return pointPlayerStay;
        return transform;
    }

    public void Init(MapController mapController, bool isSaved)
    {
        this.mapController = mapController;
        Saved(isSaved);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == PlayerController.instance.transform)
        {
            mapController?.OnPlayerHitCheckpoint(this);
            Saved(true);
        }
    }

    public void Saved(bool isSaved)
    {
        if(meshRenderer != null)
        {
            meshRenderer.material = isSaved ? matGreen : matRed;
        }
    }
}
