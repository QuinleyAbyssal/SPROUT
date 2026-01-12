public interface IInteractable
{
    void Interact();

    // Check to see if the object is currently in a state to be interacted with.
    // (e.g., Is the NPC busy? Is the door locked? Has the item already been picked up?)
    bool CanInteract();
}