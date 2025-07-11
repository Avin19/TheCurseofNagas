using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float movSpeed = 7f;
    private Vector2 inputVector;
    private Vector3 movDir;


    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        inputVector = gameInput.GetMovementVector();
        movDir = new Vector3(inputVector.x, 0f, inputVector.y);

        transform.position += movDir * movSpeed * Time.deltaTime;
    }
}
