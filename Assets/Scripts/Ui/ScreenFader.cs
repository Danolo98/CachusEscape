using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Cachu.UI
{
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader I;

        [Header("Refs")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.6f;
        [SerializeField] private Color fadeColor = Color.black;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(gameObject);

            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            }
        }

        public void FadeOutThenIn(System.Action midAction = null)
        {
            if (fadeImage == null) return;

            fadeImage.DOFade(1f, fadeDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    midAction?.Invoke();
                    fadeImage.DOFade(0f, fadeDuration * 0.5f)
                        .SetEase(Ease.InQuad);
                });
        }
    }
}
