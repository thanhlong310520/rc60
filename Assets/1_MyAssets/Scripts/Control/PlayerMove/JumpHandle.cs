using UnityEngine;
using UnityEngine.Events;

public class JumpHandle
{
    int numberJump;
    int currentJump;

    public JumpHandle(int numberJump)
    {
        this.numberJump = numberJump;
        ResetJump();
    }

    public void Jump(Rigidbody rb, bool isGround, float jumpPower,UnityAction actionJump)
    {
        if (currentJump <= 0) return;
        if (currentJump == numberJump)
        {
            if (!isGround) return;
        }
        currentJump--;
        Vector3 velocityChange = new Vector3(0, jumpPower - rb.linearVelocity.y, 0);
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
        actionJump?.Invoke();
    }

    public void ResetJump()
    {
        currentJump = numberJump;
    }
}
