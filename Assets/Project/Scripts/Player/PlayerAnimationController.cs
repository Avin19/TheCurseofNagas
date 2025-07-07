
using System;
using System.Threading.Tasks;

using UnityEngine;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Player
{
    [Serializable]
    public class PlayerAnimationController
    {
        // private GameInput _gameInput;
        // private Vector2 inputVector;

        private Transform _playerMain;
        private Animator _playerAnimator;
        private const string isWalking = "IsWalking";

        #region AnimationValues
        private readonly Vector3 _LEFTFACING = new Vector3(-32f, 180f, 0f);
        private readonly Vector3 _RIGHTFACING = new Vector3(32f, 0f, 0f);

        private const float _ROLL_Y_VALUE = -0.3f;
        private const string _PLAYER_STATUS = "PlayerStatus";
        private const string _IDLE = "Player_Idle", _ROLL = "Player_Roll", _JUMP = "Player_Jump",
             _INTERACT = "Player_Interact", _ATTACK = "Player_Attack";
        #endregion AnimationValues

        public void Initialize(Animator playerAnimator, Transform playerMain)
        {
            _playerAnimator = playerAnimator;
            _playerMain = playerMain;
        }

        public void Update(in Vector2 inputVector)
        {
            // inputVector = _gameInput.GetMovementVector();

            AnimationController(inputVector);
        }

        private void AnimationController(Vector2 input)
        {
            if (input != Vector2.zero)
            {
                _playerAnimator.SetBool(isWalking, true);

            }
            else
            {
                _playerAnimator.SetBool(isWalking, false);
            }
        }

        public void RotatePlayer(bool isFacingRight)
        {
            if (isFacingRight)
                _playerMain.localEulerAngles = _RIGHTFACING;
            else
                _playerMain.localEulerAngles = _LEFTFACING;
        }

        public void PlayAnimation(PlayerStatus status)
        {
            switch (status)
            {
                case PlayerStatus.IDLE:
                    _playerAnimator.SetInteger(_PLAYER_STATUS, 0);
                    // _playerAnimator.Play(_IDLE);

                    break;

                case PlayerStatus.MOVING:
                    _playerAnimator.SetInteger(_PLAYER_STATUS, (int)status);
                    // _playerAnimator.Play(_IDLE);

                    break;

                case PlayerStatus.JUMPING:
                    _playerAnimator.SetInteger(_PLAYER_STATUS, (int)status);
                    // _playerAnimator.Play(_JUMP);

                    break;

                case PlayerStatus.ROLLING:
                    {
                        //Player Main Pos = y(-1.84)
                        Vector3 rollPos = _playerMain.transform.localPosition;          //Maybe use ref for temp vec3
                        rollPos.y = _ROLL_Y_VALUE;
                        _playerMain.transform.localPosition = rollPos;

                        _playerAnimator.SetInteger(_PLAYER_STATUS, (int)status);
                        // _playerAnimator.Play(_ROLL);

                        ResetValuesAfterAnimation(status);
                    }

                    break;

                case PlayerStatus.ATTACKING:
                    _playerAnimator.SetInteger(_PLAYER_STATUS, (int)status);
                    // _playerAnimator.Play(_ATTACK);

                    break;

                case PlayerStatus.INTERACTING:
                    _playerAnimator.SetInteger(_PLAYER_STATUS, (int)status);
                    // _playerAnimator.Play(_INTERACT);

                    break;
            }
        }

        private async void ResetValuesAfterAnimation(PlayerStatus status)
        {
            switch (status)
            {
                case PlayerStatus.ROLLING:
                    {
                        await Task.Delay(500);

                        //REset values
                        Vector3 rollPos = _playerMain.transform.localPosition;          //Maybe use ref for temp vec3
                        rollPos.y = 0f;
                        _playerMain.transform.localPosition = rollPos;
                    }

                    break;
            }
        }
    }
}