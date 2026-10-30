using UnityEngine;

namespace TheFairyHills.Gameplay
{
    /// <summary>Small session state shared by the persistent Core scene and loaded levels.</summary>
    public sealed class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }
        public bool HasIronCross { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void CollectIronCross() => HasIronCross = true;
    }
}
