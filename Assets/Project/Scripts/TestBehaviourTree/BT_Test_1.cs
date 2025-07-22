// #define YOUTUBE_IMPLEMENTATION

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace CurseOfNaga.TestBehaviourTree
{
    public class BT_Test_1 : MonoBehaviour
    {
        private Node _topNode;
        private CheckNode_1[] checkNode_1Arr;
        private DoNode_1[] doNode_1Arr;
        [SerializeField] private int totalQuantity = 6;

        [SerializeField] private NodeState[] _setCheckNodesArr, _setDoNodesArr;

        //----------- Chase/Shoot
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _chaseRange, _shootRange;
        //-----------

        private CancellationTokenSource _cts;

        private void OnDestroy()
        {
            if (_cts != null) _cts.Cancel();
        }

        private void Start()
        {
            _cts = new CancellationTokenSource();

            checkNode_1Arr = new CheckNode_1[totalQuantity];
            doNode_1Arr = new DoNode_1[totalQuantity];

            _setCheckNodesArr = new NodeState[totalQuantity];
            _setDoNodesArr = new NodeState[totalQuantity];

            ConstructBehaviourTree();
            EvaluateBT();
        }

        private void ConstructBehaviourTree()
        {

            for (int i = 0; i < totalQuantity; i++)
            {
                checkNode_1Arr[i] = new CheckNode_1();
                doNode_1Arr[i] = new DoNode_1();
            }

            ChaseNode chaseNode = new ChaseNode(_playerTransform, transform);
            RangeNode chasingRangeNode = new RangeNode(_chaseRange, _playerTransform, transform);

            ShootNode shootNode = new ShootNode();
            RangeNode shootingRangeNode = new RangeNode(_shootRange, _playerTransform, transform);

#if YOUTUBE_IMPLEMENTATION
            Sequence chaseSequence = new Sequence(new List<Node> { chasingRangeNode, chaseNode });
            Sequence shootSequence = new Sequence(new List<Node> { shootingRangeNode, shootNode });

            _topNode = new Selector(new List<Node> { shootSequence, chaseSequence });
#else
            Sequence checkSequence_0 = new Sequence(new List<Node> { checkNode_1Arr[0], doNode_1Arr[0], checkNode_1Arr[1], });
            Sequence checkSequence_1 = new Sequence(new List<Node> { checkNode_1Arr[2], doNode_1Arr[1], checkNode_1Arr[3], });
            Sequence checkSequence_2 = new Sequence(new List<Node> { checkNode_1Arr[4], doNode_1Arr[2], checkNode_1Arr[5], });

            _topNode = new Selector(new List<Node> { checkSequence_0, checkSequence_1, checkSequence_2 });
#endif
        }

        private int count = 0;
        private async void EvaluateBT()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (_cts.IsCancellationRequested) return;

                for (int i = 0; i < totalQuantity; i++)
                {
                    checkNode_1Arr[i].SetNode = _setCheckNodesArr[i];
                    doNode_1Arr[i].SetNode = _setDoNodesArr[i];
                }

                _topNode.Evaluate(count);
                count++;
            }
        }
    }
}