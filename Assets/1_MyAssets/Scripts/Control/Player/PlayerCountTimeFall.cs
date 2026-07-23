using Raccoon.Controller;
using UnityEngine;

public class PlayerCountTimeFall : MonoBehaviour
{
    public float timeFall = 5f;
    float currentTimeFall = 0;
    private void Start()
    {
        ResetTimeFall();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerController.instance.PlayerState != EPlayerState.Normal) return;
        if (PlayerController.instance.movement.isGrounded || PlayerController.instance.movement.climbing || PlayerController.instance.movement.isMantling)
        {
            ResetTimeFall();
            return;
        }
        if (PlayerController.instance.rb.linearVelocity.y >= 0)
        {
            ResetTimeFall();
            return;
        }

        CountTimeFall();
    }

    public void ResetTimeFall()
    {
        currentTimeFall = timeFall;
    }

    void CountTimeFall()
    {
        currentTimeFall -= Time.deltaTime;
        if(currentTimeFall < 0)
        {
            PlayerController.instance.Dead();
        }
    }
}
