using UnityEngine;
using System.Collections.Generic;

namespace Cachu.World
{
    public class TrackGenerator : MonoBehaviour
    {
        [Header("Refs")]
        public Transform root;

        [Tooltip("Lista de posibles prefabs de rama (elige una al azar en cada spawn)")]
        public BranchTarget[] branchPrefabs; // 👈 nuevo arreglo

        [Tooltip("Prefab de respaldo (opcional, por compatibilidad)")]
        public BranchTarget branchPrefab; // fallback

        public PatternSO[] patterns;

        [Header("Chunk")]
        public float laneX = 0f; // mantenemos centro en X (ramas centradas)
        public float baseY = 2.0f; // altura base del túnel
        public float zStride = 4.0f; // separación aproximada entre ramas

        [Header("Streaming")]
        public Transform player;
        public int keepAhead = 30; // cuántas ramas mantener por delante
        public int keepBehind = 10; // cuántas dejamos antes de reciclar

        Queue<BranchTarget> active = new();
        System.Random rng;

        void Awake() { rng = new System.Random(); }

        void Start() { EnsureAhead(); }
        void Update() { EnsureAhead(); RecycleBehind(); }

        void EnsureAhead()
        {
            while (active.Count < keepAhead)
            {
                var pat = PickPattern();
                SpawnPattern(pat);
            }
        }

        void RecycleBehind()
        {
            while (active.Count > 0)
            {
                var first = active.Peek();
                if (player.position.z - first.transform.position.z > zStride * keepBehind)
                {
                    var gone = active.Dequeue();
                    Destroy(gone.gameObject);
                }
                else break;
            }
        }

        PatternSO PickPattern()
        {
            float sum = 0;
            foreach (var p in patterns) sum += Mathf.Max(0.0001f, p.weight);
            double roll = rng.NextDouble() * sum;
            float acc = 0;
            foreach (var p in patterns)
            {
                acc += Mathf.Max(0.0001f, p.weight);
                if (roll <= acc) return p;
            }
            return patterns[0];
        }

        void SpawnPattern(PatternSO pat)
        {
            if (pat == null || pat.localPositions == null || pat.localPositions.Length == 0)
            {
                Debug.LogWarning("⚠️ TrackGenerator: patrón nulo o vacío");
                return;
            }

            BranchTarget last = active.Count > 0 ? GetLast() : null;

            // 🔧 Si no hay ramas aún, el primer bloque se genera justo debajo del jugador
            Vector3 basePos;
            if (last == null)
            {
                basePos = new Vector3(laneX, player.position.y - 1.2f, player.position.z);
                Debug.Log($"🌱 Primera rama generada en {basePos}");
            }
            else
            {
                basePos = last.transform.position;
            }

            BranchTarget prev = last;

            for (int i = 0; i < pat.localPositions.Length; i++)
            {
                Vector3 local = pat.localPositions[i];
                Vector3 world = basePos + new Vector3(local.x, local.y, local.z);

                // 🎲 Elegir un prefab al azar de la lista
                BranchTarget prefabToUse = null;
                if (branchPrefabs != null && branchPrefabs.Length > 0)
                    prefabToUse = branchPrefabs[rng.Next(branchPrefabs.Length)];
                else
                    prefabToUse = branchPrefab;

                var b = Instantiate(prefabToUse, world, Quaternion.identity, root);

                if (b == null)
                {
                    Debug.LogError("🚨 TrackGenerator: BranchPrefab no tiene BranchTarget!");
                    continue;
                }

                if (prev != null)
                {
                    prev.next = b;
                    Debug.Log($"🔗 Vinculado {prev.name} → {b.name}");
                }

                prev = b;
                active.Enqueue(b);
                basePos = world;
            }

            // 🌿 Si esta es la primera rama, forzamos al jugador a estar sobre ella
            if (active.Count == pat.localPositions.Length)
            {
                var firstBranch = active.Peek();
                if (firstBranch != null)
                {
                    player.position = firstBranch.LandPosition + Vector3.up * 0.1f;
                    Debug.Log($"🦝 Jugador colocado sobre la primera rama en {player.position}");
                }
            }
        }

        BranchTarget GetLast()
        {
            BranchTarget last = null;
            foreach (var b in active) last = b;
            return last;
        }
    }
}
