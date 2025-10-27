using UnityEngine;


namespace Cachu.World
{
    public class BranchTarget : MonoBehaviour
    {
        [Tooltip("Siguiente rama a la que se saltará si el timing es correcto")] public BranchTarget next;
        [Tooltip("Punto preciso donde aterriza el jugador")] public Transform landPoint;
        [Tooltip("Punto hacia el que se orienta la cámara (opcional)")] public Transform focusPoint;


        public Vector3 LandPosition => landPoint ? landPoint.position : transform.position;


        void OnValidate() { if (!landPoint) landPoint = transform; }
    }
}