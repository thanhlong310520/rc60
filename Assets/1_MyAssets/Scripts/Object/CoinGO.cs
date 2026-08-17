using Raccoon.Controller;
using UnityEngine;

public class CoinGO : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == PlayerController.instance.transform)
        {
            PlayerController.instance.OnContactCoin();
            gameObject.SetActive(false);
        }
    }
}
