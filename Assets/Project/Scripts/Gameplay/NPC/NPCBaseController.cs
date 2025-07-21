using UnityEngine;

using CurseOfNaga.Global;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.NPC
{
    public class BaseNPCController : MonoBehaviour, IInteractable
    {
        private void Start()
        {

        }

        // Can have code to determine if the NPC wants to interact with the Player or not
        public InteractionType Interact()
        {
            Debug.Log($"Making Interaction | Id: {transform.GetInstanceID()}");
            return InteractionType.INTERACTING_WITH_NPC;
        }
    }
}