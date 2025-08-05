using UnityEngine;

namespace CurseOfNaga.Gameplay.Enemies.Test
{
    public class SimpleEnemyTest : MonoBehaviour
    {
        [SerializeField, Range(0.15f, 2f)] private float _moveSpeed = 1f;
        [SerializeField, Range(1f, 10f)] private float _minRange = 3f, _maxRange = 7f;
        private Vector3[] _movePoints;
        private int _currMoveIndex;

        private void Start()
        {
            FillMovePoints();
        }

        private void FillMovePoints()
        {
            int moveLength = 6;
            _movePoints = new Vector3[moveLength];
            Vector2 randomVec;
            float randomAngle;

            for (int i = 0; i < moveLength; i++)
            {
                randomAngle = Random.Range(0, 360);
                randomVec.x = Mathf.Cos(randomAngle);
                randomVec.y = Mathf.Sin(randomAngle);

                randomVec *= Random.Range(_minRange, _maxRange);
                _movePoints[i] = transform.position + new Vector3(randomVec.x, 0f, randomVec.y);
            }
        }

        private void Update()
        {
            if (Vector3.SqrMagnitude(transform.position - _movePoints[_currMoveIndex]) < 1f)
            {
                _currMoveIndex = ((_currMoveIndex + 1) >= _movePoints.Length) ? 0 : ++_currMoveIndex;
            }

            // Vector3 finalPos = transform.position;
            Vector3 dirVec = (_movePoints[_currMoveIndex] - transform.position).normalized;
            // finalPos = dirVec * Time.deltaTime * _moveSpeed;
            transform.position += dirVec * Time.deltaTime * _moveSpeed;
        }
    }
}