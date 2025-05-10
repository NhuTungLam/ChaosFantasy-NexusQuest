using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector Instance { get; private set; }
    public CharacterData characterData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Duplicate CharacterSelector destroyed: " + this);
            Destroy(gameObject);
        }
    }

    public static CharacterData LoadData()
    {
        if (Instance == null)
        {
            Debug.LogError("CharacterSelector.Instance is NULL. LoadData failed.");
            return null;
        }

        return Instance.characterData;
    }

    public void SelectCharacter(CharacterData character)
    {
        characterData = character;
    }

    /// <summary>
    /// Gọi khi không còn cần giữ singleton nữa (ví dụ: logout)
    /// </summary>
    public static void DestroySelector()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}
