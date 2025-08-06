using UnityEngine;

namespace CurseOfNaga.Global
{
    [System.Serializable]
    public struct EntityInfo
    {
        public float Health, Damage;
    }

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
            ACTIVATE_INTERACTION = 1 << 13,
        }

        public enum EnemyStatus
        {
            IDLE = 0,
            MOVING = 1 << 0,
            DODGING = 1 << 1,
            HAVE_A_TARGET = 1 << 2,
            INVESTIGATE_AREA = 1 << 3,
            PLAYER_VISIBLE = 1 << 4,
            LOST_PLAYER = 1 << 5,
            CHASING_PLAYER = 1 << 6,
            REACHED_PLAYER = 1 << 7,
            ATTACKING_PLAYER = 1 << 8,
            ENEMY_WITHIN_PLAYER_RANGE = 1 << 9,
            PLAYER_WITHIN_RANGE = 1 << 10,
            PLAYER_ATTACKING = 1 << 11,
            DEAD = 1 << 12,
            ATTACK_DECIDED = 1 << 13,
            ATTACK_AT_HIT_POINT = 1 << 14,
            BLOCKING_ATTACK = 1 << 15,
        }

        public enum EnemyAttackType
        {
            // MELEE = 2 | RANGED_ATTACK = 128
            NOT_ATTACKING = 0, MELEE = 1 << 1,
            NORMAL_M0 = 3, NORMAL_M1 = 4, NORMAL_M2 = 5, NORMAL_M3 = 6, NORMAL_M4 = 7, NORMAL_M5 = 8,
            HEAVY_M0 = 13, HEAVY_M1 = 14, HEAVY_M2 = 15, HEAVY_M3 = 16, HEAVY_M4 = 17, HEAVY_M5 = 18,
            RANGED_ATTACK = 1 << 7, SHOOT1 = 129, THROW1 = 130
        }

        public enum EnemyType
        {
            ENEMY_0, ENEMY_1, ENEMY_2, ENEMY_3
        }

        public enum CombatDecision
        {
            NOT_DECIDED = 0, ATTACK = 1, DEFEND = 2, STRAFE = 3, WAIT = 4,
        }

        public enum ComboType
        {
            DEFAULT = 0,
            COMBO_0 = 1, COMBO_1 = 2, COMBO_2 = 3, COMBO_3 = 4, COMBO_4 = 5, COMBO_5 = 6,
            COMBO_6 = 7, COMBO_7 = 8, COMBO_8 = 9, COMBO_9 = 10, COMBO_10 = 11, COMBO_11 = 12,
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

        public enum InteractionType
        {
            NONE = 0, PROMPT_TRIGGERED, INTERACTION_REQUEST,
            INTERACTING_WITH_NPC, PICKING_UP_OBJECT, PUTTING_DOWN_OBJECT,
            USE_ITEM,
            INVOKE_TRIGGER, LEFT_TRIGGER,
        }

        public enum SaveStatus
        {
            DEFAULT = 0,
            SAVE_SUCCESSFUL, SAVE_FAILED,
            LOAD_SUCCESSFUL, LOAD_FAILED
        }

        public enum AnimationClipStatus
        {
            PLAYING, FINISHED, REACHED_HIT_POINT
        }
    }
}