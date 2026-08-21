using Raccoon.Controller;
using UnityEngine;

public class CoinGO : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == PlayerController.instance.transform)
        {
            VfxCtrl.instance.SpawnRandomVfx(Raccoon.EnumHolder.TypeVFX.Coin, transform.position + Vector3.up * 1.25f);
            PlayerController.instance.OnContactCoin();
            gameObject.SetActive(false);
        }
    }
}
