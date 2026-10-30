using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Routes the player to the next level when entering an exit trigger.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class ChapterSceneExit : PlayerInteractionTarget
    {
        [SerializeField] private string destinationScene;
        [SerializeField] private string destinationSpawn = "Spawn_FromPrevious";
        [SerializeField] private bool requireInteraction;
        [SerializeField] private bool returnToMenu;
        private bool activated;

        public void Configure(string destination, bool waitForKey, bool backToMenu = false, string spawn = "Spawn_FromPrevious")
        {
            destinationScene = destination;
            requireInteraction = waitForKey;
            returnToMenu = backToMenu;
            destinationSpawn = spawn;
            SetPrompt(waitForKey ? "Press E to return to the Main Menu" : "Entering the forest path…");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (requireInteraction || activated || other.GetComponentInParent<PlayerController2D>() == null) return;
            activated = true;
            LoadDestination();
        }

        public override void Interact(PlayerController2D player)
        {
            if (!requireInteraction || activated) return;
            activated = true;
            LoadDestination();
        }

        private void LoadDestination()
        {
            ChapterSceneLoader loader = FindFirstObjectByType<ChapterSceneLoader>();
            if (loader == null) return;
            if (returnToMenu) loader.ReturnToMenu();
            else loader.LoadLevel(destinationScene, destinationSpawn);
        }
    }
}
