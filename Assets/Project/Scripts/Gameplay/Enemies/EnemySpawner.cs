using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using MathF = System.MathF;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class EnemySpawner
    {
        // List of Enemy prefabs
        // List of spawn points
        // Spawn around the point within a range 

        [SerializeField] private GameObject[] _enemyPrefabs;
        [SerializeField] private Transform[] _spawnPoints;
        private float _spawnRadius;
        private int _maxSpawnCount;
        private float _spawnInterval;

        private List<GameObject> _spawnedEnemies;

        private CancellationTokenSource _cts;

        ~EnemySpawner()
        {
            _cts.Cancel();
        }

        public EnemySpawner()
        {
            _cts = new CancellationTokenSource();
            _spawnedEnemies = new List<GameObject>();
        }

        public void Initialize(ref float spawnRadius, ref int maxSpawnCount, ref float spawnInterval)
        {
            _spawnRadius = spawnRadius;
            _maxSpawnCount = maxSpawnCount;
            _spawnInterval = spawnInterval;
        }

        public async void SpawnEnemies(Transform controller)
        {
            //Similar to random points in PoissonDiscSampler
            Vector3 randDirVec = Vector3.zero;
            float randomAngle;

            GameObject spawnedEnemy;

            for (int spawnId = 0; spawnId < _maxSpawnCount; spawnId++)
            {
                //Spawn at every Spawn Point
                for (int spawnPointId = 0; spawnPointId < _spawnPoints.Length; spawnPointId++)
                {
                    spawnedEnemy = Object.Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)], controller.transform);

                    //Spawn within the Range
                    randomAngle = Random.Range(0, 360);
                    randDirVec.x = MathF.Cos(randomAngle * (MathF.PI / 180));
                    randDirVec.z = MathF.Sin(randomAngle * (MathF.PI / 180));
                    randDirVec *= Random.Range(0, _spawnRadius);

                    spawnedEnemy.transform.position = _spawnPoints[spawnPointId].position + randDirVec;
                    spawnedEnemy.name = $"SEnemy_{spawnPointId}_{spawnId}";
                    spawnedEnemy.SetActive(true);
                    _spawnedEnemies.Add(spawnedEnemy);
                    // _spawnPoints[spawnPointId].position;
                }

                await Task.Delay((int)(_spawnInterval * 1000));
                if (_cts.IsCancellationRequested) return;
            }
        }
    }
}