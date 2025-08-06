using CurseOfNaga.Gameplay.Enemies;
using UnityEngine;

namespace CurseOfNaga.Gameplay.Managers.Test
{
    public class TestEnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField, Range(1f, 5f)] private float _enspSpawnRadius;
        [SerializeField, Range(1, 10)] private int _enspMaxSpawnCount;
        [SerializeField, Range(0.25f, 5f)] private float _enspSpawnInterval;

        private void Start()
        {
            enemySpawner.Initialize(ref _enspSpawnRadius, ref _enspMaxSpawnCount, ref _enspSpawnInterval);
            enemySpawner.SpawnEnemies(transform);
        }
    }
}