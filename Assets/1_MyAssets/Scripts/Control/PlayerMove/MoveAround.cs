using UnityEngine;

public class MoveAround
{
    public void Move(Rigidbody rb, bool isMoving, Vector3 moveDir, float climbSpeed, float multiSpeedRun, bool isRunning)
    {
        if (isMoving)
        {
            var apply = CalculateApplyVelocity(moveDir, climbSpeed, multiSpeedRun, isRunning);
            apply.y = rb.linearVelocity.y;
            rb.linearVelocity = apply;
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

    }

    Vector3 CalculateApplyVelocity(Vector3 moveDir, float climbSpeed, float multiSpeedRun, bool isRunning)
    {
        float speed = 1;
        moveDir.Normalize();
        if (isRunning)
        {
            speed += multiSpeedRun / 10f;
        }

        return moveDir * climbSpeed * speed;
    }
}
