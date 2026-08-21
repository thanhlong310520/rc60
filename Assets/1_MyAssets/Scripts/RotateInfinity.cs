using UnityEngine;

public class RotateInfinity : MonoBehaviour
{
    [Header("Rotation Axis")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Header("Speed")]
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Space")]
    [SerializeField] private Space rotationSpace = Space.Self;

    private void Update()
    {
        transform.Rotate(
            rotationAxis * rotationSpeed * Time.deltaTime,
            rotationSpace
        );
    }

}