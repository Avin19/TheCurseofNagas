#define DEBUG_1
using UnityEngine;

using CurseOfNaga.Global;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.NPC
{
    //TODO: Make this abstract
    public class BaseNPCController : MonoBehaviour, IInteractable
    {
        [SerializeField] protected int _UID, _NpcID;                //UID : id by array | npcID : id by character
        [SerializeField] protected string _Name;
        public int UID { get => _UID; }

        private void Start()
        {

        }

        public void Initialize(string npcName, int uid)
        {
            _Name = npcName;
            _UID = uid;

            int.TryParse(npcName.Substring(0, 3), out _NpcID);
        }

        // Can have code to determine if the NPC wants to interact with the Player or not
        public InteractionType Interact(InteractionType interactionType, out int npcID)
        {
            InteractionType type = InteractionType.INTERACTING_WITH_NPC;
            switch (interactionType)
            {
                //Stop doing any activity and focus on the player
                case InteractionType.INTERACTION_REQUEST:

                    break;

                //Player has left or finished the interaction
                case InteractionType.FINISHING_INTERACTION:
                    type = InteractionType.NONE;

                    break;
            }

            npcID = _NpcID;
#if DEBUG_1
            Debug.Log($"Making Interaction | Id: {transform.GetInstanceID()}");
#endif

            return type;
        }
    }
}