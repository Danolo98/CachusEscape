using UnityEngine;
using UnityEngine.SceneManagement;


namespace Cachu.Core
{
    public enum GameState { Playing, Failed, Paused }
    public class GameFlow : MonoBehaviour
    {
        public static GameFlow I { get; private set; }
        [Range(0, 10)] public int lives = 10; // 3 llamas
        public int score; // saltos encadenados
        public GameState state { get; private set; }


        void Awake() { if (I != null) { Destroy(gameObject); return; } I = this; DontDestroyOnLoad(gameObject); state = GameState.Playing; }


        public void AddScore(int s) { if (state != GameState.Playing) return; score += s; }


        public void Miss() { if (state != GameState.Playing) return; lives--; if (lives <= 0) Fail(); }


        public void Fail() { if (state != GameState.Playing) return; state = GameState.Failed; Time.timeScale = 0f; Debug.Log("Game Over"); }


        public void Restart() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    }
}