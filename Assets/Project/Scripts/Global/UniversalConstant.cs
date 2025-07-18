using UnityEngine;

namespace CurseOfNaga.Global
{
    public class UniversalConstant
    {
        public enum GameStatus
        {
            NOT_LOADED = 0,
            LOADED_ENVIRONMENT = 1 << 0,
            LOADED_ENEMIES = 1 << 1,
            LOAD_COMPLETE = 1 << 2,
        }

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
            INVOKE_TRIGGER = 1 << 13,
            LEFT_TRIGGER = 1 << 14,
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

        public enum EnvironmentType
        {
            TREE_0 = 0, TREE_1, TREE_2, TREE_3, TREE_4, TREE_5, TREE_6, TREE_7, TREE_8,
            BUSH_0,
            GRASS_0, GRASS_1, GRASS_2, GRASS_3,
            FLOWER_0, FLOWER_1, FLOWER_2, FLOWER_3,
            ROCK_0
        }

        public enum Layer
        {
            DEFAULT = 0, ENEMY = 7, WEAPON = 8, INTERACTABLE = 9, TRIGGER = 10
        }

        public enum TriggeredEvent
        {
            DEFAULT = 0, EVENT_1, EVENT_2, EVENT_3, EVENT_4, EVENT_5,
            EVENT_6, EVENT_7, EVENT_8, EVENT_9, EVENT_10
        }
    }
}