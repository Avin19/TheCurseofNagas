using UnityEngine;

namespace CurseOfNaga.Global
{
    public class UniversalConstant
    {
        public enum GameStatus { DEFAULT, LOADED }

        public enum ObjectiveType { ACTIVE, INACTIVE, CURRENT, INVOKE_CUTSCENE }

        public enum PlayerStatus
        {
            IDLE = 0,
            MOVING = 1 << 0,
            FACING_LEFT = 1 << 1,
            FACING_RIGHT = 1 << 2,
            INVOKED_CUTSCENE = 1 << 3,
            IN_CUTSCENE = 1 << 4,
            PERFORMING_ACTION = 1 << 5,                 // Player cant move while performing this
            PERFORMING_ADDITIVE_ACTION = 1 << 6,         // Player can move while performing this
            JUMPING = 1 << 7,
            ROLLING = 1 << 8,
            ATTACKING = 1 << 9,
            INTERACTING = 1 << 10,
            USING_ITEM = 1 << 11,
            ENEMY_FOUND = 1 << 12,
        }

        public enum EnemyStatus
        {
            IDLE = 0,
            MOVING = 1 << 1,
            DODGING = 1 << 2,
            ATTACKING = 1 << 3,
            INVESTIGATE_AREA = 1 << 4,
            PLAYER_VISIBLE = 1 << 5,
            CHASING_PLAYER = 1 << 6,
            REACHED_PLAYER = 1 << 7,
            ATTACKING_PLAYER = 1 << 8,
            ENEMY_WITHIN_PLAYER_RANGE = 1 << 9,
            PLAYER_ATTACKING = 1 << 10,
            DEAD = 1 << 11,
        }
    }
}