using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class EnemyBaseController : MonoBehaviour
    {
        protected float _OgHealth = 100f;
        protected float _Health;

        private EnemyStatus _enemyStatus;
        public float _speedMult = 2f;                 //Set to protected
        public int _VISIBILITYTHRESHOLD = 10;              //Set to const?
        public float _PROXIMITYTHRESHOLD = 10;              //Set to const?

        #region Animation
        private Animator _enemyAC;
        private const string _ENEMY_STATUS = "Enemy_Status";
        private const string _IDLE = "Enemy_Idle", _MOVE = "Enemy_Move", _DODGE = "Enemy_Dodge",
            _ATTACK = "Enemy_Attack", _INVESTIGATE = "Enemy_Investigate";
        #endregion Animation

        private CancellationTokenSource _decisionCts;

        private void OnDisable()
        {
            MainGameplayManager.Instance.OnEnemyStatusUpdate -= HandleStatusChange;
        }

        private void OnEnable()
        {
            MainGameplayManager.Instance.OnEnemyStatusUpdate += HandleStatusChange;

            _decisionCts = new CancellationTokenSource();

            if (_enemyAC != null)
                MakeDecision();
        }

        void Start()
        {
            _enemyAC = transform.GetComponent<Animator>();
            MakeDecision();

            _enemyStatus = EnemyStatus.IDLE;
            _Health = _OgHealth;
        }

        private void Update()
        {
            if ((MainGameplayManager.Instance.GameStatus & GameStatus.LOADED) == 0)
                return;

            Vector3 tempVec = transform.position - MainGameplayManager.Instance.PlayerTransform.position;

            if (tempVec.sqrMagnitude <= _VISIBILITYTHRESHOLD * _VISIBILITYTHRESHOLD)
            {
                if ((_enemyStatus & EnemyStatus.PLAYER_VISIBLE) == 0)
                {
                    _enemyStatus &= ~EnemyStatus.IDLE;
                    _enemyStatus |= EnemyStatus.PLAYER_VISIBLE;
                    _enemyStatus |= EnemyStatus.CHASING_PLAYER;
                    PlayAnimation(EnemyStatus.MOVING);
                }
                ChasePlayer();
            }
            else if ((_enemyStatus & EnemyStatus.PLAYER_VISIBLE) != 0)
            {
                // PlayAnimation();
                _enemyStatus &= ~EnemyStatus.PLAYER_VISIBLE;
                _enemyStatus &= ~EnemyStatus.CHASING_PLAYER;
            }
            // else if ((_enemyStatus & EnemyStatus.REACHED_PLAYER) != 0)
            // {

            // }

            // if ((_enemyStatus & EnemyStatus.PLAYER_VISIBLE) != 0)
            // {
            //     _enemyStatus |= EnemyStatus.CHASING_PLAYER;
            //     MoveTowardsPlayer();
            // }
        }

        private void ChasePlayer()
        {
            // if ((_enemyStatus & EnemyStatus.PLAYER_VISIBLE) == 0) return;

            Vector3 playerDir = transform.position - MainGameplayManager.Instance.PlayerTransform.position;

            // Enemy is close enough to Player
            if (playerDir.sqrMagnitude <= _PROXIMITYTHRESHOLD * _PROXIMITYTHRESHOLD)
            {
                _enemyStatus |= EnemyStatus.REACHED_PLAYER;
                // MakeDecision();
                return;
            }

            transform.position -= playerDir.normalized * _speedMult * Time.deltaTime;
        }

        // Decision on what to do
        private async void MakeDecision()
        {
            //Consider possibilities of what enemy can do and then execute accordingly
            int randomDecision;
            //Attack Routine
            if ((_enemyStatus & EnemyStatus.REACHED_PLAYER) != 0)
            {
                randomDecision = Random.Range(0, 10);

                if (randomDecision < 3)
                {
                    GetAroundPlayer();
                }
                else
                {
                    AttackPlayer();
                }
            }
            //Investigate Area
            else if ((_enemyStatus & EnemyStatus.PLAYER_VISIBLE) != 0
                && (_enemyStatus & EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE) == 0)
            {
                InvestigateTheArea();
            }
            //Normal Routine
            else
            {
                randomDecision = Random.Range(0, 10);

                if (randomDecision < 3)
                {
                    StayAndLookAround();
                }
                else
                {
                    RoamAroundTheArea();
                }
            }

            int randomDecisionTime = Random.Range(500, 5000);       //0.5s - 5s

            await Task.Delay(randomDecisionTime);
            // MakeDecision();
        }

        private void StayAndLookAround()
        {
            //Stay in one place and look around the area
            //Would not have to do much, as the NPC will just stand and look around
            //Maybe play some different IDLE animation
            PlayAnimation(EnemyStatus.IDLE);
        }

        private void RoamAroundTheArea()
        {
            //Start to roam around designated area?
            //How to mark the designated area?
            //  [=] Follow point to point? Designated paths for enemy to follow
            //      {+} Would need to store the paths for different enemies then
            //  [=] Select any random point and go there?
            PlayAnimation(EnemyStatus.MOVING);
        }

        private void InvestigateTheArea()
        {
            //Stay at one place and look for player by spinning around and searching everything?
            //Stay at one place and look at a specific direction for the player?
            //Cooldown meter for investigation
            //  [=] This should cancel the MakeDecision call
            PlayAnimation(EnemyStatus.INVESTIGATE_AREA);
        }

        //Async as attacks would need to be chained
        private async void AttackPlayer()
        {
            //Decide which kind of attack to do from a set of moves
            //Continous chains of attack or a single attack?
            //Delayed attack?
            //  [=] Need some defense stance?
            //The enemy pulls back and starts going to and fro, bobbing around?
        }

        private void GetAroundPlayer()
        {
            //Maybe player is defending, so get around the player
            //Maybe the bobbing code will go here?
        }

        private void HandleStatusChange(EnemyStatus status, int transformID, float value)
        {
            switch (status)
            {
                case EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE:
                    if (value == 1)
                        _enemyStatus |= EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE;
                    else
                        _enemyStatus &= ~EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE;

                    break;

                case EnemyStatus.PLAYER_ATTACKING:
                    if ((_enemyStatus & EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE) != 0)
                        GetDamage(value);
                    break;
            }
        }

        private void PlayAnimation(EnemyStatus enemyStatus)
        {
            switch (enemyStatus)
            {
                case EnemyStatus.IDLE:
                    _enemyAC.SetInteger(_ENEMY_STATUS, 0);
                    _enemyAC.Play(_IDLE);

                    break;

                case EnemyStatus.MOVING:
                    _enemyAC.SetInteger(_ENEMY_STATUS, (int)enemyStatus);
                    _enemyAC.Play(_MOVE);

                    break;

                case EnemyStatus.DODGING:
                    _enemyAC.SetInteger(_ENEMY_STATUS, (int)enemyStatus);
                    _enemyAC.Play(_DODGE);

                    break;

                case EnemyStatus.ATTACKING:
                    _enemyAC.SetInteger(_ENEMY_STATUS, (int)enemyStatus);
                    _enemyAC.Play(_ATTACK);

                    break;

                case EnemyStatus.INVESTIGATE_AREA:
                    _enemyAC.SetInteger(_ENEMY_STATUS, (int)enemyStatus);
                    _enemyAC.Play(_ATTACK);

                    break;
            }
        }

        private async void UnsetAction_Async(EnemyStatus status)
        {
            switch (status)
            {
                case EnemyStatus.IDLE:
                    PlayAnimation(EnemyStatus.IDLE);

                    break;

                case EnemyStatus.ATTACKING:
                    await Task.Delay(500);
                    _enemyStatus &= ~EnemyStatus.ATTACKING;

                    goto case EnemyStatus.IDLE;
            }
        }

        public void GetDamage(float damage)
        {
            // Debug.Log($"Received Damage: {damage}");

            if (_Health <= 0)
            {
                _enemyStatus &= ~EnemyStatus.ENEMY_WITHIN_PLAYER_RANGE;
                MainGameplayManager.Instance.OnEnemyStatusUpdate?.Invoke(EnemyStatus.DEAD, transform.GetInstanceID(), -1);
                gameObject.SetActive(false);
            }

            _Health -= damage;
            //Pushed back from damage
        }
    }
}