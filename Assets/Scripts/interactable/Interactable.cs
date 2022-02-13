using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public KeyCode InteractionKey;

    public string InteractionDescription;
    // TODO accept only specific player/team ?

    public abstract void Interact(GameObject initiater);
}