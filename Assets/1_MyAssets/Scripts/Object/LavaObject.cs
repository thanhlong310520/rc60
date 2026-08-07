using Raccoon.Controller;
using UnityEngine;

public class LavaObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == PlayerController.instance.transform)
        {
            PlayerController.instance.Dead();
        }
    }
}
