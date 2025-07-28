using UnityEngine;

using CurseOfNaga.Global.Template;
using static CurseOfNaga.Global.UniversalConstant;
using System.Collections.Generic;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class EnemyBoard
    {
        public Transform Self;
        public EnemyStatus Status;
        public byte AttackTypeBase;
        public EnemyAttackType AttackType;

        public byte SelectedComboIndex, CurrentAttackIndex;
        public List<MeleeCombo> MeleeCombos;

        public EnemyBoard(Transform self, List<MeleeCombo> combos)
        {
            Self = self;
            Status = EnemyStatus.IDLE;
            AttackType = EnemyAttackType.NOT_ATTACKING;
            SelectedComboIndex = 255;
            CurrentAttackIndex = 0;
            MeleeCombos = combos;
        }
    }
}