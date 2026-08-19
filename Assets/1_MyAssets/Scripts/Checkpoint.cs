using Raccoon.Controller;
using System.Collections;
using System.Collections.Generic;
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

    public GameObject coin;

    public float timeAlive = 2f;    
    public List<GameObject> listVfxOnHit;

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
        if(coin != null)
        {
            coin.SetActive(!isSaved);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == PlayerController.instance.transform)
        {
            mapController?.OnPlayerHitCheckpoint(this);
            Saved(true);
        }
    }

    public void PlayVfx(bool turnOff)
    {
        StartCoroutine(PlayVfxCoroutine(turnOff));
    }
    IEnumerator PlayVfxCoroutine(bool turnOff)
    {
        foreach (var vfx in listVfxOnHit)
        {
            vfx.SetActive(true);
        }
        yield return new WaitForSeconds(timeAlive);
        if (turnOff)
        {
            foreach (var vfx in listVfxOnHit)
            {
                vfx.SetActive(false);
            }
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
