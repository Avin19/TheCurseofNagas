using CurseOfNaga.Gameplay.Enemies;
using UnityEngine;

namespace CurseOfNaga.Gameplay.Managers.Test
{
    public class TestEnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;

        private void Start()
        {
            enemySpawner.SpawnEnemy(transform);
        }
    }
}