using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Base for nearby actions invoked with E.</summary>
    public abstract class PlayerInteractionTarget : MonoBehaviour
    {
        [SerializeField] private string prompt = "Press E to interact";
        public string Prompt => prompt;
        public virtual bool CanInteract => isActiveAndEnabled;

        protected void SetPrompt(string value) => prompt = value;
        public abstract void Interact(PlayerController2D player);
    }
}
