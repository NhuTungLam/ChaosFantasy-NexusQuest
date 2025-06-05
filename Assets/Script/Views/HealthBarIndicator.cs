using UnityEngine;
using UnityEngine.UI;

public class HealthBarIndicator : MonoBehaviour
{
    public Image image;
    public Sprite[] indicators; // 0 = empty, 8 = full

    [Range(0, 1f)]
    public float testFill = 1f; 

    public void SetHealth(float percent)
    {
        int index = Mathf.Clamp(Mathf.FloorToInt(percent * indicators.Length), 0, indicators.Length - 1);
        image.sprite = indicators[index];
    }

    void Update()
    {
        SetHealth(testFill);
    }
}
