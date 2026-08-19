using UnityEngine;

public class PlayerEffect : MonoBehaviour
{
    public bool isFalling = false;
    public GameObject trail;
    public void PlayEffectJump()
    {
        // Play jump effect
        VfxCtrl.instance.SpawnRandomVfx(Raccoon.EnumHolder.TypeVFX.Jump, transform.position);
    } 

    public void PlayEffectMove(float forwardAmount, float turnAmount, bool isGrounded, bool isFalling)
    {
        // Play move effect
        if(this.isFalling != isFalling)
        {
            this.isFalling = isFalling;
            if (!isFalling)
            {
                // Play landing effect
                VfxCtrl.instance.SpawnRandomVfx(Raccoon.EnumHolder.TypeVFX.InGround, transform.position + Vector3.up);

            }
        }
        trail.SetActive(isGrounded);
    }
}
