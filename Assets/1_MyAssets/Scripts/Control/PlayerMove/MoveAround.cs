using UnityEngine;

public class MoveAround
{
    public void Move(Rigidbody rb, bool isMoving, Vector3 moveDir, float climbSpeed, float multiSpeedRun, bool isRunning, Vector3 extendVelocity)
    {
        if (isMoving)
        {
            var apply = CalculateApplyVelocity(moveDir, climbSpeed, multiSpeedRun, isRunning);
            apply.y = rb.linearVelocity.y;
            rb.linearVelocity = apply + extendVelocity;
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0) + extendVelocity;
        }

    }

    Vector3 CalculateApplyVelocity(Vector3 moveDir, float climbSpeed, float multiSpeedRun, bool isRunning)
    {
        float speed = 1;
        moveDir.Normalize();
        if (isRunning)
        {
            speed += multiSpeedRun;
        }

        return moveDir * climbSpeed * speed;
    }
}
