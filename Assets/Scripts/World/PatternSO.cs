using UnityEngine;


namespace Cachu.World
{
    [CreateAssetMenu(menuName = "Cachu/Pattern", fileName = "PatternSO")]
    public class PatternSO : ScriptableObject
    {
        [Tooltip("Posiciones relativas de las ramas dentro del patr�n (en espacio del chunk). Z debe ir en aumento")]
        public Vector3[] localPositions;


        [Tooltip("Probabilidad relativa de que salga este patr�n")]
        public float weight = 1f;


        [Tooltip("Dificultad opcional (1=f�cil, 3=dif�cil)")]
        public int difficulty = 1;
    }
}