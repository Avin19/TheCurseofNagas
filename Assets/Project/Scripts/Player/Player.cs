// #define TESTING

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using CurseOfNaga.Gameplay.Enemies;
using CurseOfNaga.Gameplay.Managers;
using CurseOfNaga.Global;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Player
{
    public class Player : MonoBehaviour
    {
        internal enum HitStatus { DID_NOT_HIT, HIT }

        [Serializable]
        internal struct HitInfo
        {
            public int ID;
            public HitStatus Status;
        }

        [SerializeField] private GameInput gameInput;
        [SerializeField] private float _movSpeed = 7f;
        // [SerializeField] private float jumpMult = 2f;
        [SerializeField] private float _rollSpeed = 5f;

        private PlayerStatus _playerStatus;
        // [SerializeField] private Transform _weaponPlacement;
        [SerializeField] private Transform _playerMain;
        [SerializeField] private BoxCollider _playerCollider;

        // private Animator _playerAC;
        private PlayerAnimationController _animationController;

        private Rigidbody _playerRb;
        private Vector3 inputVector;

        private InteractionType _currentInteractionStatus;
        private IInteractable _currentInteractable;

        // private readonly Vector3 _LEFTFACING = new Vector3(-40f, 180f, 0f);
        // private readonly Vector3 _RIGHTFACING = new Vector3(40f, 0f, 0f);
        private const float _LEFT_FACING_WEAPON_PLACEMENT = 1.45f;
        private const float _RIGHT_FACING_WEAPON_PLACEMENT = -1.2f;
        // private const int _ENEMY_LAYER = 7;

        #region AnimationValues
        private const float _ROLL_COLLIDER_Y_POS = 1.25f, _ROLL_COLLIDER_Y_SIZE = 2.5f;
        private const float _ROLL_COLLIDER_Y_POS_OG = 3.1f, _ROLL_COLLIDER_Y_SIZE_OG = 6.19f;
        #endregion AnimationValues

        private void OnDisable()
        {
            MainGameplayManager.Instance.OnObjectiveVisible -= UpdatePlayerStatus;
            gameInput.OnInputDone -= HandleInput;
            // MainGameplayManager.Instance.OnEnemyStatusUpdate -= UpdateEnemyInfo;
        }

        private void Start()
        {
#if TESTING
            _movSpeed = 25f;
#endif
            _playerRb = GetComponent<Rigidbody>();
            // _playerAC = GetComponent<Animator>();

            _animationController = new PlayerAnimationController();
            _animationController.Initialize(transform.GetChild(0).GetComponent<Animator>(), transform.GetChild(0));

            MainGameplayManager.Instance.OnObjectiveVisible += UpdatePlayerStatus;
            gameInput.OnInputDone += HandleInput;
            // MainGameplayManager.Instance.OnEnemyStatusUpdate += UpdateEnemyInfo;
        }

        private void Update()
        {
            HandleAction();

            if ((_playerStatus & PlayerStatus.PERFORMING_ACTION) == 0)
            {
                HandleMovement();
            }
        }

        // private void FixedUpdate()
        // {
        //     //- Cast a sphere and check if any interactable things are within it
        //     //  [=] This will waste some resource, as we are only searching in x-z axis. We will never be 
        //     //      looking for anything above the ground
        //     //- A simple position check in the x-z axis should do the trick instead
        //     //  [=] Problem will come if any dynamic object is needed to be added and the player would not 
        //     //      detect the item if it is not registered
        // }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Detected Collider: {other.name} | layer: {other.gameObject.layer} | ");
            int colliderID;
            switch (other.gameObject.layer)                   //other.gameobject can be a bit consuming
            {
                case (int)Layer.ENEMY:
                    colliderID = other.transform.parent.GetInstanceID();
                    MainGameplayManager.Instance.OnEnemyStatusUpdate?.Invoke(EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE, colliderID, 1);
                    Debug.Log($"Instance ID: {other.transform.parent.GetInstanceID()}");

                    break;

                case (int)Layer.INTERACTABLE:
                    // colliderID = other.transform.parent.GetInstanceID();
                    MainGameplayManager.Instance.OnPlayerInteraction?.Invoke(InteractionType.PROMPT_TRIGGERED, 1);
                    _currentInteractionStatus = InteractionType.PROMPT_TRIGGERED;
                    _currentInteractable = other.transform.parent.GetComponent<IInteractable>();

                    break;

                case (int)Layer.TRIGGER:
                    colliderID = other.transform.parent.GetInstanceID();
                    MainGameplayManager.Instance.OnPlayerInteraction?.Invoke(InteractionType.INVOKE_TRIGGER, colliderID);
                    Debug.Log($"Instance ID: {other.transform.parent.GetInstanceID()}");

                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            int colliderID;
            switch (other.gameObject.layer)                   //other.gameobject can be a bit consuming
            {
                case (int)Layer.ENEMY:
                    colliderID = other.transform.parent.GetInstanceID();
                    MainGameplayManager.Instance.OnEnemyStatusUpdate?.Invoke(EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE, colliderID, 0);

                    break;

                case (int)Layer.INTERACTABLE:
                    _currentInteractionStatus = InteractionType.NONE;
                    _currentInteractable = null;

                    break;

                case (int)Layer.TRIGGER:
                    colliderID = other.transform.parent.GetInstanceID();
                    MainGameplayManager.Instance.OnPlayerInteraction?.Invoke(InteractionType.LEFT_TRIGGER, colliderID);

                    break;
            }
        }

        private void HandleInput(PlayerStatus status, float value)
        {
            // Debug.Log($"Status: {status} | Value: {value}");
            // if ((gameInput._currentInputStatus & InputStatus.ATTACK) != 0)
            switch (status)
            {
                case PlayerStatus.JUMPING:
                    if (value > 0)
                    {
                        _playerStatus |= PlayerStatus.JUMPING;
                        _playerStatus |= PlayerStatus.PERFORMING_ACTION;
                        // _playerRb.AddForce(Vector3.up * jumpMult, ForceMode.Impulse);            //Does not feels/looks good
                        // PlayAnimation(PlayerStatus.JUMPING);
                        _animationController.PlayAnimation(PlayerStatus.JUMPING);
                        UnsetAction_Async(PlayerStatus.JUMPING);
                    }
                    // else
                    // {
                    //     _playerStatus &= ~PlayerStatus.JUMPING;
                    //     _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;
                    // }

                    break;

                case PlayerStatus.ROLLING:
                    if (value > 0)
                    {
                        _playerStatus |= PlayerStatus.ROLLING;
                        _playerStatus |= PlayerStatus.PERFORMING_ACTION;

                        Vector3 finalVec;

                        //Collider Size = y(2.5)
                        finalVec = _playerCollider.size;
                        finalVec.y = _ROLL_COLLIDER_Y_SIZE;
                        _playerCollider.size = finalVec;

                        //Collider Position = y(1.25)
                        finalVec = _playerCollider.transform.position;
                        finalVec.y = _ROLL_COLLIDER_Y_POS;
                        _playerCollider.transform.localPosition = finalVec;

                        //Player Main Pos = y(-1.84)
                        // finalVec = _playerMain.transform.localPosition;          //Maybe use ref for temp vec3
                        // finalVec.y = _ROLL_Y_VALUE;
                        // _playerMain.transform.localPosition = finalVec;

                        // PlayAnimation(PlayerStatus.ROLLING);
                        _animationController.PlayAnimation(PlayerStatus.ROLLING);
                        UnsetAction_Async(PlayerStatus.ROLLING);
                    }
                    // else
                    // {
                    //     _playerStatus &= ~PlayerStatus.ROLLING;
                    //     // _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;
                    // }

                    break;

                case PlayerStatus.ATTACKING:
                    if (value > 0)
                    {
                        _playerStatus |= PlayerStatus.ATTACKING;
                        _playerStatus |= PlayerStatus.PERFORMING_ADDITIVE_ACTION;
                        // PlayAnimation(PlayerStatus.ATTACKING);
                        _animationController.PlayAnimation(PlayerStatus.ATTACKING);
                        UnsetAction_Async(PlayerStatus.ATTACKING);

                        // Check for Enemy-Hit
                        MainGameplayManager.Instance.OnEnemyStatusUpdate?.Invoke(EnemyStatus.PLAYER_ATTACKING, -1, 10f);
                    }
                    // else
                    // {
                    //     _playerStatus &= ~PlayerStatus.ATTACKING;
                    //     // _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;
                    // }

                    break;

                /*
                * MOVING | JUMPING | ROLLING | INTERACTING
                *
                * Flow:
                *   - Player comes within the range of intertacting with something
                *   - Player sees a prompt trigger at the top of the screen
                *   - Player presses the necessary button to trigger the interaction
                *   - Player is interacting with the Object/ NPC/ item
                *
                * Conditions:
                *   - There can be long-press situations also
                *
                * Actions are divided into 2 types: Additive | Non-Additive
                *   [=] Non-Additive actions 
                *       {+} Performed one at a time | continously.
                *       {+} Eg: Rolling: Player can't attack/interact/jump/use item during rolling
                *   [=] Additive actions
                *       {+} Performed continously | One on top of another action | Not more than 2 actions tho
                *       {+} Eg: Attack can be done while running
                */
                case PlayerStatus.INTERACTING:
                    if (value > 0
                        && (_playerStatus & PlayerStatus.INTERACTING) == 0)
                    {
                        _playerStatus |= PlayerStatus.INTERACTING;
                        _playerStatus |= PlayerStatus.PERFORMING_ACTION;

                        // PlayAnimation(PlayerStatus.INTERACTING);
                        _animationController.PlayAnimation(PlayerStatus.INTERACTING);

                        _currentInteractionStatus = InteractionType.INTERACTION_REQUEST;
                        _currentInteractionStatus = _currentInteractable.Interact();
                        // UnsetAction_Async(PlayerStatus.INTERACTING);
                    }
                    // else if (value <= 0.1
                    //     && (_playerStatus & PlayerStatus.INTERACTING) != 0)
                    // {
                    //     _playerStatus &= ~PlayerStatus.INTERACTING;
                    //     _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;
                    // }

                    break;
            }
        }

        private void HandleAction()
        {
            if ((_playerStatus & PlayerStatus.ROLLING) != 0)
            {
                transform.position += inputVector.normalized * _rollSpeed * Time.deltaTime;
            }
            else if ((_playerStatus & PlayerStatus.INTERACTING) != 0)
            {
                //Sending out a raycast to check if an NPC/Object is there?
            }
        }

        private async void UnsetAction_Async(PlayerStatus status)
        {
            switch (status)
            {
                case PlayerStatus.IDLE:
                    // PlayAnimation(PlayerStatus.IDLE);
                    _animationController.PlayAnimation(PlayerStatus.IDLE);

                    break;

                case PlayerStatus.ROLLING:
                    {
                        await Task.Delay(500);

                        //REset values
                        Vector3 finalVec;

                        //Collider Size = y(2.5)
                        finalVec = _playerCollider.size;
                        finalVec.y = _ROLL_COLLIDER_Y_SIZE_OG;
                        _playerCollider.size = finalVec;

                        //Collider Position = y(1.25)
                        finalVec = _playerCollider.transform.position;
                        finalVec.y = _ROLL_COLLIDER_Y_POS_OG;
                        _playerCollider.transform.localPosition = finalVec;

                        //Player Main Pos = y(-1.84)
                        // finalVec = _playerMain.transform.localPosition;          //Maybe use ref for temp vec3
                        // finalVec.y = 0f;
                        // _playerMain.transform.localPosition = finalVec;

                        _playerStatus &= ~PlayerStatus.ROLLING;
                        _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;
                    }

                    goto case PlayerStatus.IDLE;

                case PlayerStatus.INTERACTING:
                    await Task.Delay(500);
                    _playerStatus &= ~PlayerStatus.INTERACTING;
                    _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;
                    _currentInteractionStatus = InteractionType.NONE;

                    goto case PlayerStatus.IDLE;

                case PlayerStatus.JUMPING:
                    await Task.Delay(500);
                    _playerStatus &= ~PlayerStatus.JUMPING;
                    _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;

                    goto case PlayerStatus.IDLE;

                case PlayerStatus.ATTACKING:
                    await Task.Delay(500);
                    _playerStatus &= ~PlayerStatus.ATTACKING;
                    _playerStatus &= ~PlayerStatus.PERFORMING_ADDITIVE_ACTION;

                    goto case PlayerStatus.IDLE;
            }
        }

        private void HandleMovement()
        {
            inputVector = gameInput.GetMovementVector();
            Vector3 moveDir;

            if (Mathf.Max(Mathf.Abs(inputVector.x), Mathf.Abs(inputVector.y)) > 0f)
            {
                // if ((_playerStatus & PlayerStatus.MOVING) == 0)
                {
                    _playerStatus &= ~PlayerStatus.IDLE;
                    _playerStatus |= PlayerStatus.MOVING;

                    _animationController.PlayAnimation(PlayerStatus.MOVING);

                    if (inputVector.x < 0f)
                    // && ((_playerStatus & PlayerStatus.FACING_RIGHT) != 0)
                    {
                        if ((_playerStatus & PlayerStatus.FACING_LEFT) == 0)
                        {
                            _playerStatus &= ~PlayerStatus.FACING_RIGHT;
                            _playerStatus |= PlayerStatus.FACING_LEFT;

                            // moveDir = _weaponPlacement.localPosition;
                            // moveDir.x = _LEFT_FACING_WEAPON_PLACEMENT;
                            // _weaponPlacement.localPosition = moveDir;

                            // _playerMain.localEulerAngles = _LEFTFACING;
                            _animationController.RotatePlayer(false);
                            // Debug.Log($"Facing Left | inputVector: {inputVector}");
                        }
                    }
                    else if (inputVector.x > 0f)
                    // && ((_playerStatus & PlayerStatus.FACING_LEFT) != 0)
                    {
                        if ((_playerStatus & PlayerStatus.FACING_RIGHT) == 0)
                        {
                            _playerStatus &= ~PlayerStatus.FACING_LEFT;
                            _playerStatus |= PlayerStatus.FACING_RIGHT;

                            // moveDir = _weaponPlacement.localPosition;
                            // moveDir.x = _RIGHT_FACING_WEAPON_PLACEMENT;
                            // _weaponPlacement.localPosition = moveDir;

                            // _playerMain.localEulerAngles = _RIGHTFACING;
                            _animationController.RotatePlayer(true);
                            // Debug.Log($"Facing Right | inputVector: {inputVector}");
                        }
                    }
                }
            }
            else
            {
                _playerStatus &= ~PlayerStatus.MOVING;
                _playerStatus |= PlayerStatus.IDLE;

                _animationController.PlayAnimation(PlayerStatus.IDLE);
            }

            moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
            transform.position += moveDir * _movSpeed * Time.deltaTime;
        }

        private void UpdatePlayerStatus(PlayerStatus playerStatus)
        {
            switch (playerStatus)
            {
                case PlayerStatus.IDLE:
                    _playerStatus = PlayerStatus.IDLE;
                    break;

                case PlayerStatus.MOVING:
                    break;

                case PlayerStatus.INVOKED_CUTSCENE:
                    _playerStatus |= PlayerStatus.PERFORMING_ACTION;
                    break;

                case PlayerStatus.IN_CUTSCENE:
                    break;
            }

            _playerStatus |= playerStatus;
        }
    }
}