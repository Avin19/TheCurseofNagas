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
        internal enum HiddenObjectStatus
        {
            DEACTIVE = 0,
            ACTIVE = 1 << 0,
            ACTIVATED = 1 << 1,
            ONE_TIME = 1 << 2,           // Once activated, will remain active. Eg: Door will remain open once opened
            MULTIPLE = 1 << 3,           // Requires activation everytime. Eg: Sliding/Rotating door will close once lever is let go
            // SEQUENCE = 1 << 4,          // Some sequence is repeated maybe?
        }

        [Serializable]
        internal class InvokableEventsData
        {
            public Transform TriggerTransform;
            public TriggeredEvent Event;
        }

        //Maybe import this data from Json?
        [Serializable]
        internal class HiddenObjectsData
        {
            public Transform HiddenObj;
            public float FinalPosY;
            public HiddenObjectStatus AvailableStates;                  //This should not be modifiable
            [HideInInspector] public HiddenObjectStatus CurrentStatus;
        }

        [SerializeField] private InvokableEventsData[] _eventData;
        [SerializeField] private HiddenObjectsData[] _hiddenObjects;

        private CancellationTokenSource _cts;

        ~GameplayEventManager()
        {
            _cts.Cancel();
            MainGameplayManager.Instance.OnPlayerInteraction -= RespondToPlayer;
        }

        public GameplayEventManager()
        {
            _cts = new CancellationTokenSource();
        }

        public void InitializeCallbacks()
        {
            MainGameplayManager.Instance.OnPlayerInteraction += RespondToPlayer;
        }

        //Maybe reduce to only InteractionType
        // public void RespondToPlayer(PlayerStatus status, InteractionType interactionType, int value)
        public void RespondToPlayer(InteractionType interactionType, int value)
        {
            // switch (status)
            // {
            //     case PlayerStatus.INTERACTING:

            //         break;
            // }

            switch ((InteractionType)value)
            {
                case InteractionType.INTERACTING_WITH_NPC:


                    break;

                case InteractionType.INVOKE_TRIGGER:
                    for (int i = 0; i < _eventData.Length; i++)
                    {
                        if (_eventData[i].TriggerTransform.GetInstanceID() == value)
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

                case TriggeredEvent.EVENT_2:
                    _ = MakeObjectRotate((int)(triggeredEvent - 1));

                    break;
            }
        }

        // Wall descends to floor or something goes into the ground or something else
        private async Task<int> MakeObjectDescend(int hiddenObjIndex)
        {
            float timeElapsed = 0f, timeElapsed2 = 0f;
            const float VER_SPEED_MULT = 0.5f, HOR_SPEED_MULT = 70f;
            const float HOR_DISPLACEMENT_MULT = 0.01f;
            Vector3 currentPos = _hiddenObjects[hiddenObjIndex].HiddenObj.localPosition;
            Vector2 startPos = new Vector2(currentPos.x, currentPos.y);

            while (true)
            {
                timeElapsed += Time.deltaTime * VER_SPEED_MULT;
                timeElapsed2 += Time.deltaTime * HOR_SPEED_MULT;

                if (timeElapsed > 1)
                    break;

                currentPos.y = Mathf.Lerp(startPos.y, _hiddenObjects[hiddenObjIndex].FinalPosY, timeElapsed);            // Going Down

                // Shaking sideways
                // currentPos.x += Mathf.Sin(timeElapsed2) * HOR_DISPLACEMENT_MULT;
                // currentPos.x += Mathf.Sin(Time.deltaTime * HOR_SPEED_MULT) * HOR_DISPLACEMENT_MULT;
                currentPos.x = startPos.x + (Mathf.Sin(timeElapsed2) * HOR_DISPLACEMENT_MULT);

                _hiddenObjects[hiddenObjIndex].HiddenObj.localPosition = currentPos;

                await Task.Yield();
                if (_cts.IsCancellationRequested)
                    return 0;
            }
            currentPos.y = _hiddenObjects[hiddenObjIndex].FinalPosY;
            _hiddenObjects[hiddenObjIndex].HiddenObj.localPosition = currentPos;

            return 1;
        }

        // Wall descends to floor or something goes into the ground or something else
        private async Task<int> MakeObjectRotate(int hiddenObjIndex)
        {
            float timeElapsed = 0f;
            const float TURN_SPEED_MULT = 0.5f;
            Vector3 currentRot = _hiddenObjects[hiddenObjIndex].HiddenObj.localEulerAngles;
            float startRotY = currentRot.y;

            while (true)
            {
                timeElapsed += Time.deltaTime * TURN_SPEED_MULT;

                if (timeElapsed > 1)
                    break;

                currentRot.y = Mathf.Lerp(startRotY, _hiddenObjects[hiddenObjIndex].FinalPosY, timeElapsed);            //Going Around
                _hiddenObjects[hiddenObjIndex].HiddenObj.localEulerAngles = currentRot;

                await Task.Yield();
                if (_cts.IsCancellationRequested)
                    return 0;
            }
            currentRot.y = _hiddenObjects[hiddenObjIndex].FinalPosY;
            _hiddenObjects[hiddenObjIndex].HiddenObj.localEulerAngles = currentRot;

            return 1;
        }
    }
}