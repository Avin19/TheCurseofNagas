// #define DEBUG_VISIBLE_AREA
// #define DEBUG_WORLD_POINT
#define TEST_CUTSCENE
#define TEST_GAME_1

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay
{
    public class MainGameplayManager : MonoBehaviour
    {
        [Serializable]
        internal class ObjectiveInfo
        {
            public ObjectiveType type;
            public Transform transform;
        }

        #region Singleton
        private static MainGameplayManager _instance;
        public static MainGameplayManager Instance { get => _instance; }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(this.gameObject);
        }
        #endregion Singleton

        [SerializeField] private List<ObjectiveInfo> _objectives;
        private List<ObjectiveInfo> _inactiveObjectives;

#if DEBUG_WORLD_POINT
        public Vector3[] ObjectiveWorlPos;
#endif
        [SerializeField] private Camera _mainCamera;
        public Transform PlayerTransform;

        private GameStatus _gameStatus;
        public GameStatus GameStatus { get => _gameStatus; }

        private CancellationTokenSource _cts;
        public Action<PlayerStatus> OnObjectiveVisible;
        public Action<EnemyStatus, int, float> OnEnemyStatusUpdate;

        private void OnDestroy()
        {
            if (_cts != null) _cts.Cancel();
        }

        void Start()
        {
#if DEBUG_WORLD_POINT
            ObjectiveWorlPos = new Vector3[_objectives.Length];
#endif

#if TEST_GAME_1
            SetGameStatus_Test();
#endif

            _cts = new CancellationTokenSource();
            _inactiveObjectives = new List<ObjectiveInfo>();
        }

        void Update()
        {
            CheckObjectivesVisibility();
        }

        public GameStatus SetGameStatus(GameStatus gameStatus)
        {
            _gameStatus |= gameStatus;
            return _gameStatus;
        }

        private void CheckObjectivesVisibility()
        {
            Vector3 viewPointPos;

            for (int i = 0; i < _objectives.Count; i++)
            {
                // if (_objectives[i] == null) continue;

                viewPointPos = _mainCamera.WorldToViewportPoint(_objectives[i].transform.position);

                //Skip those out of the view
                if (Mathf.Min(viewPointPos.x, viewPointPos.y) < 0f          // For Objects out-of-camera and behind
                    || Mathf.Max(viewPointPos.x, viewPointPos.y) > 1f       // For Objects out-of-camera and in-front
                    || viewPointPos.z < 0f)                                 // For Objects behind-camera
                    continue;
#if DEBUG_WORLD_POINT
                ObjectiveWorlPos[i] = worlPointPos;
#endif

                UpdateObjective(i);
            }
        }

        private void UpdateObjective(in int index)
        {
            switch (_objectives[index].type)
            {
                case ObjectiveType.ACTIVE:
                    //Check some conditions and process accordingly

                    goto case (ObjectiveType)100;

                // Do Nothing
                case ObjectiveType.INACTIVE:
                    break;

                case ObjectiveType.CURRENT:
                    //Check some conditions and process accordingly

                    goto case (ObjectiveType)100;

                case ObjectiveType.INVOKE_CUTSCENE:
                    OnObjectiveVisible?.Invoke(PlayerStatus.INVOKED_CUTSCENE);
#if TEST_CUTSCENE
                    DisableCutScene();
#endif
                    goto case (ObjectiveType)100;

                // Remove from active
                case (ObjectiveType)100:
                    _objectives[index].type = ObjectiveType.INACTIVE;
                    _inactiveObjectives.Add(_objectives[index]);
                    _objectives.RemoveAt(index);

                    break;
            }

        }

#if TEST_CUTSCENE
        private async void DisableCutScene()
        {
            await Task.Delay(5000);
            if (_cts.IsCancellationRequested) return;

            OnObjectiveVisible?.Invoke(PlayerStatus.IDLE);
        }
#endif

#if TEST_GAME_1
        private async void SetGameStatus_Test()
        {
            await Task.Delay(2000);
            if (_cts.IsCancellationRequested) return;

            _gameStatus &= ~GameStatus.LOAD_COMPLETE;
            _gameStatus |= GameStatus.LOAD_COMPLETE;
        }
#endif

        // https://iquilezles.org/articles/distfunctions2d
        // float sdBox(in vec2 p, in vec2 b)
        // {
        //     vec2 d = abs(p) - b;
        //     return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
        // }

#if DEBUG_VISIBLE_AREA
        private void OnDrawGizmos()
        {
            Vector2 _visibleArea = new Vector2(102.20f, 57.7f);
            Vector3 OffsetPosDebug = new Vector3(0f, 0f, 40.7f);
            Vector3 offsetPos = PlayerTransform.position + OffsetPosDebug;
            Gizmos.DrawWireCube(offsetPos, new Vector3(_visibleArea.x * 2, 5f, _visibleArea.y * 2));
        }
#endif
    }
}