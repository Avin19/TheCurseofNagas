
using UnityEngine;

public class GameInput : MonoBehaviour
{
    private PlayerController playerInput;

    void Awake()
    {
        playerInput = new PlayerController();
        playerInput.Player.Enable();
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInput.Player.Move.ReadValue<Vector2>();
        // Normalized Vector
        inputVector = inputVector.normalized;

        return inputVector;
    }



}
