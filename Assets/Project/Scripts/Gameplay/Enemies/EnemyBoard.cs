using UnityEngine;

using CurseOfNaga.Global.Template;
using static CurseOfNaga.Global.UniversalConstant;
using System.Collections.Generic;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class EnemyBoard
    {

        public static string PERFORM_ATTACK = "PerformAttack";

        public Transform Self;
        public EnemyStatus Status;
        public byte AttackTypeBase;
        public EnemyAttackType AttackType;

        public byte SelectedComboIndex;
        public int CurrentAttackIndex;
        public List<MeleeCombo> MeleeCombos;
        public float[] AnimClipLengths;

        public float DamageMultiplier;
        public Animator EnemyAnimator;

        public const int ALREADY_PLAYING = (1 << 20), PLAY_FINISHED = (1 << 21);

        public EnemyBoard(Transform self, List<MeleeCombo> combos, Animator animator, float[] clipLengths)
        {
            Self = self;
            Status = EnemyStatus.IDLE;
            AttackType = EnemyAttackType.NOT_ATTACKING;
            SelectedComboIndex = 255;
            CurrentAttackIndex = 0;
            MeleeCombos = combos;
            EnemyAnimator = animator;
            AnimClipLengths = clipLengths;
        }
    }
}