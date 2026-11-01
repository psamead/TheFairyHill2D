using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Reversible gate: E hides the boards and clears the tall blocker until Niamh leaves the area.</summary>
    public sealed class VillageGate : PlayerInteractionTarget
    {
        [SerializeField] private SpriteRenderer boards;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Collider2D blocker;
        private bool opened;

        public void Configure(SpriteRenderer gateRenderer, Sprite closed, Sprite open, Collider2D invisibleWall)
        {
            boards = gateRenderer;
            closedSprite = closed;
            openSprite = open;
            blocker = invisibleWall;
            if (boards != null) boards.sprite = closedSprite;
            if (boards != null) boards.enabled = true;
            if (blocker != null) blocker.enabled = true;
            SetPrompt("Press E to push open the gate");
        }

        public override bool CanInteract => base.CanInteract && !opened;

        public override void Interact(PlayerController2D player)
        {
            if (opened) return;
            opened = true;
            if (boards != null) boards.sprite = openSprite;
            if (blocker != null) blocker.enabled = false;
            if (player != null) player.ShowMessage("The gate swings open.");
        }

        public void CloseAfterPlayerLeaves()
        {
            if (!opened) return;
            opened = false;
            if (boards != null) boards.sprite = closedSprite;
            if (blocker != null) blocker.enabled = true;
        }
    }

}
