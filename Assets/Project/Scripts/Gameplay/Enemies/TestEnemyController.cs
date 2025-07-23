using UnityEngine;

using CurseOfNaga.BehaviourTree;
using CurseOfNaga.Gameplay.Managers;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class TestEnemyController : MonoBehaviour
    {
        private Node _rootNode;
        [SerializeField] private float _playerVisibleRange;
        [SerializeField] private float _chaseStopRange, _chaseSpeedMult;

        [SerializeField] private Transform[] _patrolPoints;
        [SerializeField] private float _patrolWaitTime;

        int debugIntVar = 0;

        private void Start()
        {

            InitializeTree();
        }

        private void InitializeTree()
        {
            CheckRangeSquared checkPlayerVisibility = new CheckRangeSquared(transform, MainGameplayManager.Instance.PlayerTransform, _playerVisibleRange);
            // Selector 

            ChaseTargetTask chasePlayer = new ChaseTargetTask(transform, MainGameplayManager.Instance.PlayerTransform, _chaseStopRange, _chaseSpeedMult);

            Vector3[] patrolPoints = new Vector3[_patrolPoints.Length];
            for (int i = 0; i < _patrolPoints.Length; i++)
                patrolPoints[i] = _patrolPoints[i].position;

            PatrolAreaTask patrolArea = new PatrolAreaTask(patrolPoints, _patrolWaitTime);
            Sequence chasePlayerSequence = new Sequence(new Node[] {
                checkPlayerVisibility, chasePlayer
            });

            _rootNode = new Selector();
        }


        private void Update()
        {
            _rootNode.Evaluate(debugIntVar);
        }
    }
}