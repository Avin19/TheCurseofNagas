using System.Collections.Generic;
using System.Threading;

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
        [SerializeField, Range(1f, 5f)] private float _spawnRadius;
        [SerializeField, Range(1, 10)] private int _maxSpawnCount;

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

        public void SpawnEnemy(Transform controller)
        {
            //Similar to random points in PoissonDiscSampler
            Vector3 randDirVec = Vector3.zero;
            float randomAngle;

            GameObject spawnedEnemy;

            //Spawn at every Spawn Point
            for (int spawnPointId = 0; spawnPointId < _spawnPoints.Length; spawnPointId++)
            {
                //Spawn within the Range
                for (int spawnId = 0; spawnId < _maxSpawnCount; spawnId++)
                {
                    spawnedEnemy = Object.Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)], controller.transform);

                    randomAngle = Random.Range(0, 360);
                    randDirVec.x = MathF.Cos(randomAngle * (MathF.PI / 180));
                    randDirVec.z = MathF.Sin(randomAngle * (MathF.PI / 180));

                    spawnedEnemy.transform.position = _spawnPoints[spawnPointId].position + (randDirVec
                        * Random.Range(0, _spawnRadius));
                    spawnedEnemy.name = $"SEnemy_{spawnPointId}_{spawnId}";
                    _spawnedEnemies.Add(spawnedEnemy);
                    // _spawnPoints[spawnPointId].position;
                }
            }
        }
    }
}