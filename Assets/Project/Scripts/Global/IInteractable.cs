using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Global
{
    public interface IInteractable
    {
        // void Interact(out InteractionType interactionType, out int interactalbeUID, out int otherID);
        int UID { get; }
        InteractionType Interact(InteractionType interactionType, out int otherID);
    }
}