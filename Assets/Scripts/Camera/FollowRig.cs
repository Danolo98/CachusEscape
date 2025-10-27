using UnityEngine;


namespace Cachu.CameraRig
{
    public class FollowRig : MonoBehaviour
    {
        public Transform target; public Vector3 offset = new(0f, 4f, -8f); public float smooth = 6f;
        void LateUpdate() { if (!target) return; var desired = target.position + offset; transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime); transform.LookAt(target.position + Vector3.forward * 6f); }
    }
}