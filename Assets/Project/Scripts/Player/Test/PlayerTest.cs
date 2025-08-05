// #define TESTING

using UnityEngine;

namespace CurseOfNaga.Gameplay.Player.Test
{
    public class PlayerTest : MonoBehaviour
    {
        private PlayerController playerInput;
        [SerializeField] private float _movSpeed = 7f;
        [SerializeField] private Transform _camera;

        private void Start()
        {
            playerInput = new PlayerController();
            playerInput.Player.Enable();

#if TESTING
            _movSpeed = 25f;
#endif
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector2 inputVector = playerInput.Player.Move.ReadValue<Vector2>();
            inputVector = inputVector.normalized;            // Normalized Vector

            Vector3 moveDir;

            moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
            transform.position += moveDir * _movSpeed * Time.deltaTime;
        }

        private void LateUpdate()
        {
            Vector3 finalPos = transform.position;
            finalPos.z -= 8.4f;
            finalPos.y = _camera.position.y;
            _camera.position = finalPos;
        }
    }
}