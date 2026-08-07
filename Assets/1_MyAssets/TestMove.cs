using Raccoon.InputCtr;
using StarterAssets;
using UnityEngine;

public class TestMove : MonoBehaviour
{

    public SimpleMovementController movement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnEnable()
    {
        EventBus.Subscribe<PlayerInput>(UpdateValueInput);
    }

    public void UpdateValueInput(PlayerInput value)
    {
        if (value == null) return;
        Vector2 result = new Vector2(value.horizontalAxis, value.verticalAxis); 
        movement.SetMoveInput(result);
        movement.Jump = value.isJump;

    }
}
