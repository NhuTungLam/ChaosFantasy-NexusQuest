using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using DG.Tweening;
using DG.Tweening.Core;

public class SceneController : MonoBehaviour
{
    public RectTransform bgRect;     // Drag your UI Image RectTransform here
    public float topY = 1500f;       // How high to bounce
    public float bottomY = -1500f;   // How low to bounce
    public float duration = 60f;      // Time to move between top and bottom

    void Start()
    {
        // Start from bottom position
        bgRect.anchoredPosition = Vector2.zero;

        // Create infinite bounce loop between topY and bottomY
        bgRect.DOAnchorPosY(topY, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    public void StartGame()
    {
        BlackScreen.Instance.BlackIn();
        this.Invoke(() => SceneManager.LoadScene("Nexus"), 1.2f);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
