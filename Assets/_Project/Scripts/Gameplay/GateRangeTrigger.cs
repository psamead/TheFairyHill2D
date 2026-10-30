using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Forwards the gate-area trigger exit to its owning gate.</summary>
    public sealed class GateRangeTrigger : MonoBehaviour
    {
        [SerializeField] private VillageGate gate;
        public void Configure(VillageGate owner) => gate = owner;

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController2D>() != null && gate != null)
                gate.CloseAfterPlayerLeaves();
        }
    }
}
