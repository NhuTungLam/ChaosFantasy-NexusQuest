using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BlackScreen : MonoBehaviour
{
    public static BlackScreen Instance;
    private Image _bScreen;
    private void Awake()
    {
        _bScreen = GetComponent<Image>();
        Instance = this;

        SceneManager.activeSceneChanged += (s, a) => BlackOut();
    }

    public void BlackIn()
    {
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        _bScreen.color = new Color32(0, 0, 0, 0);
        _bScreen.DOFade(1f, 1f);
    }
    public void BlackOut()
    {
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        _bScreen.color = new Color32(0, 0, 0, 255);
        _bScreen.DOFade(0f, 1f).OnComplete(() => GetComponent<RectTransform>().anchoredPosition = new Vector2(2000, 2000));
    }
}
