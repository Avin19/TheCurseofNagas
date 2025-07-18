using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay
{
    [Serializable]
    public class GameplayEventManager
    {
        [Serializable]
        internal class InvokableEventsData
        {
            public Transform TriggerTransform;
            public TriggeredEvent Event;
        }

        [Serializable]
        internal class HiddenObjectsData
        {
            public Transform HiddenObj;
            public float FinalPosY;
        }

        [SerializeField] private InvokableEventsData[] _eventData;
        [SerializeField] private HiddenObjectsData[] _hiddenObjects;

        private CancellationTokenSource _cts;

        ~GameplayEventManager()
        {
            _cts.Cancel();
            MainGameplayManager.Instance.OnPlayerEnterTrigger -= RespondToPlayer;
        }

        public GameplayEventManager()
        {
            _cts = new CancellationTokenSource();
        }

        public void InitializeCallbacks()
        {
            MainGameplayManager.Instance.OnPlayerEnterTrigger += RespondToPlayer;
        }

        public void RespondToPlayer(PlayerStatus status, int transformID)
        {
            switch (status)
            {
                case PlayerStatus.INVOKE_TRIGGER:
                    for (int i = 0; i < _eventData.Length; i++)
                    {
                        if (_eventData[i].TriggerTransform.GetInstanceID() == transformID)
                            InvokeEvent(_eventData[i].Event);
                    }

                    break;
            }
        }

        private void InvokeEvent(TriggeredEvent triggeredEvent)
        {
            Debug.Log($"Invoking Event: {triggeredEvent}");
            switch (triggeredEvent)
            {
                case TriggeredEvent.EVENT_1:
                    _ = MakeObjectDescend((int)(triggeredEvent - 1));

                    break;
            }
        }

        // Wall descends to floor or something goes into the ground or something else
        private async Task<int> MakeObjectDescend(int hiddenObjIndex)
        {
            float timeElapsed = 0f;
            const float SPEED_MULT = 0.5f;
            Vector3 currentPos = _hiddenObjects[hiddenObjIndex].HiddenObj.position;
            float startPosY = currentPos.y;

            while (true)
            {
                timeElapsed += Time.deltaTime * SPEED_MULT;

                if (timeElapsed > 1)
                    break;

                currentPos.y = Mathf.Lerp(startPosY, _hiddenObjects[hiddenObjIndex].FinalPosY, timeElapsed);
                _hiddenObjects[hiddenObjIndex].HiddenObj.position = currentPos;

                await Task.Yield();
                if (_cts.IsCancellationRequested)
                    return 0;
            }
            currentPos.y = _hiddenObjects[hiddenObjIndex].FinalPosY;
            _hiddenObjects[hiddenObjIndex].HiddenObj.position = currentPos;

            return 1;
        }
    }
}