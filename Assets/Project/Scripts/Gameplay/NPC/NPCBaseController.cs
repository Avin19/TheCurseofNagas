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
        public void Interact(out InteractionType interactionType, out int interactableUID, out int npcID)
        {
            interactableUID = _UID;
            npcID = _NpcID;
            interactionType = InteractionType.INTERACTING_WITH_NPC;
            Debug.Log($"Making Interaction | Id: {transform.GetInstanceID()}");
        }
    }
}