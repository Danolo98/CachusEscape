using UnityEngine;


namespace Cachu.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioTac : MonoBehaviour
    {
        public static AudioTac I { get; private set; }
        AudioSource src;
        public AudioClip tacClip; // un click/chasquido corto
        void Awake() { if (I != null) { Destroy(gameObject); return; } I = this; DontDestroyOnLoad(gameObject); src = GetComponent<AudioSource>(); }
        public void Tac(float volume = 1f, float pitch = 1f) { if (!tacClip) return; src.pitch = pitch; src.PlayOneShot(tacClip, volume); }
    }
}