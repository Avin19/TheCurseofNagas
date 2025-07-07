#define TESTING

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using CurseOfNaga.Gameplay.Enemies;
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
        [SerializeField] private float _rollSpeed = 2f;

        private PlayerStatus _playerStatus;
        [SerializeField] private Transform _weaponPlacement;
        [SerializeField] private Transform _playerMain;
        [SerializeField] private BoxCollider _playerCollider;

        private Animator _playerAC;
        // private Rigidbody _playerRb;
        private Vector3 inputVector;

        private readonly Vector3 _LEFTFACING = new Vector3(-40f, 180f, 0f);
        private readonly Vector3 _RIGHTFACING = new Vector3(40f, 0f, 0f);
        private const float _LEFT_FACING_WEAPON_PLACEMENT = 1.45f;
        private const float _RIGHT_FACING_WEAPON_PLACEMENT = -1.2f;
        private const int _ENEMY_LAYER = 7;

        #region AnimationValues
        private const float _ROLL_Y_VALUE = -1.84f, _ROLL_COLLIDER_Y_POS = 1.25f, _ROLL_COLLIDER_Y_SIZE = 2.5f;
        private const float _ROLL_COLLIDER_Y_POS_OG = 3.1f, _ROLL_COLLIDER_Y_SIZE_OG = 6.19f;
        private const string _PLAYER_STATUS = "PlayerStatus";
        private const string _IDLE = "Player_Idle", _ROLL = "Player_Roll", _JUMP = "Player_Jump",
             _INTERACT = "Player_Interact", _ATTACK = "Player_Attack";
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
            // _playerRb = GetComponent<Rigidbody>();
            _playerAC = GetComponent<Animator>();

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

        private void OnTriggerEnter(Collider other)
        {
            // Debug.Log($"Detected Collider: {other.name} | layer: {other.gameObject.layer}");

            if (other.gameObject.layer == _ENEMY_LAYER)                   //other.gameobject can be a bit consuming
            {
                int colliderID = other.transform.parent.GetInstanceID();
                MainGameplayManager.Instance.OnEnemyStatusUpdate?.Invoke(EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE, colliderID, 1);
            }
        }

        private void OnTriggerExit(Collider other)
        {

            if (other.gameObject.layer == _ENEMY_LAYER)                   //other.gameobject can be a bit consuming
            {
                int colliderID = other.transform.parent.GetInstanceID();
                MainGameplayManager.Instance.OnEnemyStatusUpdate?.Invoke(EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE, colliderID, 0);
            }
        }

        private void PlayAnimation(PlayerStatus playerStatus)
        {
            switch (playerStatus)
            {
                case PlayerStatus.IDLE:
                    _playerAC.SetInteger(_PLAYER_STATUS, 0);
                    _playerAC.Play(_IDLE);

                    break;

                case PlayerStatus.JUMPING:
                    _playerAC.SetInteger(_PLAYER_STATUS, (int)playerStatus);
                    _playerAC.Play(_JUMP);

                    break;

                case PlayerStatus.ROLLING:
                    _playerAC.SetInteger(_PLAYER_STATUS, (int)playerStatus);
                    _playerAC.Play(_ROLL);

                    break;

                case PlayerStatus.ATTACKING:
                    _playerAC.SetInteger(_PLAYER_STATUS, (int)playerStatus);
                    _playerAC.Play(_ATTACK);

                    break;

                case PlayerStatus.INTERACTING:
                    _playerAC.SetInteger(_PLAYER_STATUS, (int)playerStatus);
                    _playerAC.Play(_INTERACT);

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
                        finalVec = _playerMain.transform.localPosition;          //Maybe use ref for temp vec3
                        finalVec.y = _ROLL_Y_VALUE;
                        _playerMain.transform.localPosition = finalVec;

                        // PlayAnimation(PlayerStatus.ROLLING);
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

                case PlayerStatus.INTERACTING:
                    if (value > 0)
                    {
                        _playerStatus |= PlayerStatus.INTERACTING;
                        _playerStatus |= PlayerStatus.PERFORMING_ACTION;
                        // PlayAnimation(PlayerStatus.INTERACTING);
                        UnsetAction_Async(PlayerStatus.ROLLING);
                    }
                    // else
                    // {
                    //     _playerStatus &= ~PlayerStatus.INTERACTING; 
                    //     // _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;
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
        }

        private async void UnsetAction_Async(PlayerStatus status)
        {
            switch (status)
            {
                case PlayerStatus.IDLE:
                    // PlayAnimation(PlayerStatus.IDLE);

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
                        finalVec = _playerMain.transform.localPosition;          //Maybe use ref for temp vec3
                        finalVec.y = 0f;
                        _playerMain.transform.localPosition = finalVec;

                        _playerStatus &= ~PlayerStatus.ROLLING;
                        _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;
                    }

                    goto case PlayerStatus.IDLE;

                case PlayerStatus.INTERACTING:
                    await Task.Delay(1000);
                    _playerStatus &= ~PlayerStatus.INTERACTING;
                    _playerStatus &= ~PlayerStatus.PERFORMING_ACTION;

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
                            // Debug.Log($"Facing Right | inputVector: {inputVector}");
                        }
                    }
                }
            }
            else
            {
                _playerStatus &= ~PlayerStatus.MOVING;
                _playerStatus |= PlayerStatus.IDLE;
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