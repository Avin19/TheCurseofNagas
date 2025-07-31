using CurseOfNaga.Global;
using UnityEngine;

namespace CurseOfNaga.Gameplay.Enemies.Test
{
    public class TestDamageableObject : MonoBehaviour, IDamageable
    {
        [SerializeField] private EntityInfo _entityInfo;

        public float ReceiveDamage(float damage)
        {
            _entityInfo.Health -= damage;

            if (_entityInfo.Health <= 0)
                gameObject.SetActive(false);

            return _entityInfo.Health;
        }

        private void Start()
        {
        }
    }
}