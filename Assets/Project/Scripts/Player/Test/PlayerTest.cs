// #define TESTING
// #define DEBUG_1
#define DIALOGUE_TEST

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
        //UID is for the object ID in the pooled/initialized array| ObjID is for the specific type of Interacting Obj
        // private int _currInteractUID, _currInteractObjID;
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
#if DEBUG_1
            Debug.Log($"Player Interact Requested");
#endif
            if (_currentInteractable == null)
                return;

            //Only set if the Interaction has not yet started
            if (_currInteractableType == InteractionType.PROMPT_TRIGGERED)
            {
                int objID;
                _currInteractableType = _currentInteractable.Interact(InteractionType.INTERACTION_REQUEST, out objID);
                // _currInteractUID = _currentInteractable.UID;
#if DIALOGUE_TEST
                TestDialogueMainManager.Instance.OnPlayerInteraction?
                    .Invoke(_currInteractableType, _currentInteractable.UID, objID);
#endif
            }
#if DIALOGUE_TEST
            else if ((TestDialogueMainManager.Instance.CurrPlayerStatus & PlayerStatus.MAKING_CHOICE) == 0)
            {
                TestDialogueMainManager.Instance.OnPlayerInteraction?
                    .Invoke(_currInteractableType, _SET_VAL, _DEFAULT_VAL);
            }
#endif
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Entered Collider: {other.name} | layer: {other.gameObject.layer}");
            switch (other.gameObject.layer)
            {
                case (int)Layer.INTERACTABLE:
#if DIALOGUE_TEST
                    TestDialogueMainManager.Instance.OnPlayerInteraction?.Invoke(
                            InteractionType.PROMPT_TRIGGERED, _SET_VAL, _DEFAULT_VAL);
#endif
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
#if DIALOGUE_TEST
                    TestDialogueMainManager.Instance.OnPlayerInteraction?.Invoke(
                            InteractionType.FINISHING_INTERACTION, _UNSET_VAL, _DEFAULT_VAL);
#endif
                    int otherID;
                    _currInteractableType = _currentInteractable.Interact(
                            InteractionType.FINISHING_INTERACTION, out otherID);
                    _currentInteractable = null;

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