using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 🔥 DOTween para la animación de fade

namespace Cachu.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Refs")]
        public CanvasGroup menuCanvas; // referencia al canvas principal
        public Text titleText;         // opcional: para animar texto o mostrar título
        public Text pressText;         // texto de "Presiona Espacio"

        private bool gameStarted = false;
        private static bool menuShown = false;
        private static MainMenuUI instance;

        private void Awake()
        {
            // Evitar duplicados entre resets o recargas
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (menuCanvas == null)
            {
                Debug.LogError("⚠️ MainMenuUI: Falta asignar CanvasGroup.");
                return;
            }

            // Si ya se mostró antes, ocultarlo inmediatamente
            if (menuShown)
            {
                menuCanvas.alpha = 0f;
                menuCanvas.interactable = false;
                menuCanvas.blocksRaycasts = false;
                gameObject.SetActive(false);
                return;
            }

            // Primera vez: mostrar menú
            menuShown = true;
            gameStarted = false;

            // Congelar tiempo
            Time.timeScale = 0f;

            // Configurar Canvas
            menuCanvas.alpha = 1f;
            menuCanvas.interactable = true;
            menuCanvas.blocksRaycasts = true;
        }

        private void Update()
        {
            if (gameStarted || !menuCanvas) return;

            // Animar texto "Press Space" parpadeando
            if (pressText)
            {
                float a = Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2f));
                pressText.color = new Color(
                    pressText.color.r,
                    pressText.color.g,
                    pressText.color.b,
                    a
                );
            }

            // Detectar barra espaciadora
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            if (gameStarted) return;
            gameStarted = true;

            Debug.Log("🚀 Iniciando juego — ocultando menú con fade...");

            // Reanudar tiempo después del fade
            if (menuCanvas)
            {
                menuCanvas.interactable = false;
                menuCanvas.blocksRaycasts = false;

                // 🔥 Animación de fade con DOTween
                menuCanvas.DOFade(0f, 0.8f).SetUpdate(true).OnComplete(() =>
                {
                    Time.timeScale = 1f;
                    menuCanvas.gameObject.SetActive(false);
                });
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }
}
