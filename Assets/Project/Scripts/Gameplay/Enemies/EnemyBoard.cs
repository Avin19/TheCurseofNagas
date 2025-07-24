using UnityEngine;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class EnemyBoard
    {
        public Transform Self;
        public EnemyStatus Status;

        public EnemyBoard(Transform self)
        {
            Self = self;
            Status = EnemyStatus.IDLE;
        }
    }
}