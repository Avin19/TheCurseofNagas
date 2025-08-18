// #define TESTING
#define INTERACTABLE_NPC

using UnityEngine;
using static CurseOfNaga.Global.UniversalConstant;


#if INTERACTABLE_NPC
using CurseOfNaga.Global;
using CurseOfNaga.DialogueSystem.Test;
#endif

namespace CurseOfNaga.Gameplay.Player.Test
{
    public class PlayerTest : MonoBehaviour
    {
        private PlayerController playerInput;
        [SerializeField] private float _movSpeed = 7f;
        [SerializeField] private Transform _camera;

#if INTERACTABLE_NPC
        private InteractionType _currInteractableType;
        private IInteractable _currentInteractable;
        private int _currInteractionUID;
        private const int _NOT_ACTIVE = 0, _DUMMY_VALUE = -1;
#endif

        private void Start()
        {
            playerInput = new PlayerController();
            playerInput.Player.Enable();

#if INTERACTABLE_NPC
            playerInput.Player.Interact.performed += (ctx) => Interact();
            // playerInput.Player.Interact.canceled += (ctx) => SetAction(ctx, InputStatus.INTERACT);
#endif

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

#if INTERACTABLE_NPC
        private void Interact()
        {
            Debug.Log($"Player Interact Requested");
            if (_currentInteractable == null)
                return;

            int otherID;
            _currInteractableType = _currentInteractable.Interact(InteractionType.INTERACTION_REQUEST, out otherID);
            _currInteractionUID = _currentInteractable.UID;
            TestDialogueMainManager.Instance.OnPlayerInteraction?
                .Invoke(InteractionType.INTERACTING_WITH_NPC, _currInteractionUID, otherID);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Entered Collider: {other.name} | layer: {other.gameObject.layer}");
            switch (other.gameObject.layer)
            {
                case (int)Layer.INTERACTABLE:
                    TestDialogueMainManager.Instance.OnPlayerInteraction?.Invoke(
                            InteractionType.PROMPT_TRIGGERED, 1, _DUMMY_VALUE);
                    _currInteractableType = InteractionType.PROMPT_TRIGGERED;
                    _currentInteractable = other.transform.parent.GetComponent<IInteractable>();

                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"Exited Collider: {other.name} | layer: {other.gameObject.layer}");
            switch (other.gameObject.layer)                   //other.gameobject can be a bit consuming
            {
                case (int)Layer.INTERACTABLE:
                    TestDialogueMainManager.Instance.OnPlayerInteraction?.Invoke(
                            InteractionType.PROMPT_TRIGGERED, 0, _DUMMY_VALUE);
                    int otherID;
                    _currInteractableType = _currentInteractable.Interact(
                            InteractionType.FINISHING_INTERACTION, out otherID);
                    _currentInteractable = null;
                    _currInteractionUID = -1;

                    break;
            }
        }
#endif

        private void LateUpdate()
        {
            Vector3 finalPos = transform.position;
            finalPos.z -= 8.4f;
            finalPos.y = _camera.position.y;
            _camera.position = finalPos;
        }
    }
}