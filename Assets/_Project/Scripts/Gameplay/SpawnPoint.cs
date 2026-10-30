using UnityEngine;

namespace TheFairyHills.Gameplay
{
    public sealed class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnId = "Spawn_Start";
        public string SpawnId => spawnId;
        public void Configure(string id) => spawnId = id;
    }
}
