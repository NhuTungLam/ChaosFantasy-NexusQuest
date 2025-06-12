using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonPickup : MonoBehaviour
{
    public static DungeonPickup Instance;
    public TextMeshPro pickUpText;
    private void Awake()
    {
        Instance = this;
        pickUpText = transform.Find("text").GetComponent<TextMeshPro>();
    }
    public static void ShowPickup(string text, Vector2 pos)
    {
        Instance.transform.position = pos + new Vector2(0, 1);
        Instance.pickUpText.text = text;
    }
    public static void HidePickup()
    {
        Instance.transform.position = new Vector2(-1000, -1000);
    }
}
