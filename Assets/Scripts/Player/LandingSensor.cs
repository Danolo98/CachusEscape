using UnityEngine;
using Cachu.World;


namespace Cachu.Player
{
    public class LandingSensor : MonoBehaviour
    {
        [SerializeField] Transform foot;
        [SerializeField] float radius = 0.25f;
        [SerializeField] LayerMask branchMask;


        public BranchTarget currentBranch { get; private set; }
        public bool IsGrounded => currentBranch != null;


        void Update()
        {
            Collider[] cols = Physics.OverlapSphere(foot.position, radius, branchMask);
            currentBranch = null;
            foreach (var c in cols) { if (c.TryGetComponent(out BranchTarget b)) { currentBranch = b; break; } }
        }


        void OnDrawGizmosSelected() { if (!foot) return; Gizmos.color = Color.green; Gizmos.DrawWireSphere(foot.position, radius); }
    }
}