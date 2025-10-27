using UnityEngine;
using System.Collections.Generic;
namespace Cachu.World
{
    public class ChunkPool<T> where T : Component
    {
        readonly T prefab; readonly Transform root; readonly Stack<T> pool = new();
        public ChunkPool(T prefab, Transform root) { this.prefab = prefab; this.root = root; }
        public T Get(Vector3 pos, Quaternion rot) { var go = pool.Count > 0 ? pool.Pop() : Object.Instantiate(prefab, root); go.transform.SetPositionAndRotation(pos, rot); go.gameObject.SetActive(true); return go; }
        public void Release(T item) { item.gameObject.SetActive(false); pool.Push(item); }
    }
}