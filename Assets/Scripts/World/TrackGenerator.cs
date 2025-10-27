using UnityEngine;
using System.Collections.Generic;


namespace Cachu.World
{
    public class TrackGenerator : MonoBehaviour
    {
        [Header("Refs")] public Transform root; public BranchTarget branchPrefab; public PatternSO[] patterns;
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
                    var gone = active.Dequeue(); Destroy(gone.gameObject);
                }
                else break;
            }
        }


        PatternSO PickPattern()
        {
            float sum = 0; foreach (var p in patterns) sum += Mathf.Max(0.0001f, p.weight);
            double roll = rng.NextDouble() * sum; float acc = 0;
            foreach (var p in patterns) { acc += Mathf.Max(0.0001f, p.weight); if (roll <= acc) return p; }
            return patterns[0];
        }


        void SpawnPattern(PatternSO pat)
        {
            if (pat == null || pat.localPositions == null || pat.localPositions.Length == 0) return;
            BranchTarget last = active.Count > 0 ? GetLast() : null;
            Vector3 basePos = last ? last.transform.position : new Vector3(laneX, baseY, player.position.z + 6f);


            BranchTarget prev = last;
            for (int i = 0; i < pat.localPositions.Length; i++)
            {
                Vector3 local = pat.localPositions[i];
                Vector3 world = basePos + new Vector3(local.x, local.y, Mathf.Max(local.z, zStride));
                var b = Instantiate(branchPrefab, world, Quaternion.identity, root);
                if (prev) prev.next = b; prev = b; active.Enqueue(b);
            }
        }


        BranchTarget GetLast() { BranchTarget last = null; foreach (var b in active) last = b; return last; }
    }
}