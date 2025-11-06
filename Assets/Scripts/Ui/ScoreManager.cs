using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cachu.Game
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private int winScore = 50;
        [SerializeField] private string nextSceneName = "CinematicScene";

        private static int totalJumps = 0;

        private void Start()
        {
            UpdateUI();
        }

        public void AddJump()
        {
            totalJumps++;
            UpdateUI();

            if (totalJumps >= winScore)
                LoadNextScene();
        }

        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Saltos: {totalJumps}";
        }

        private void LoadNextScene()
        {
            SceneManager.LoadScene(nextSceneName);
        }

        public static void ResetScore()
        {
            totalJumps = 0;
        }
    }
}
