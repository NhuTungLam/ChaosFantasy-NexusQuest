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

            // Auto-load class từ PlayerPrefs nếu có
            string savedClass = PlayerPrefs.GetString("AutoSelectedCharacter", null);
            if (!string.IsNullOrEmpty(savedClass))
            {
                var loaded = Resources.Load<CharacterData>("Characters/" + savedClass);
                if (loaded != null)
                {
                    characterData = loaded;
                    Debug.Log("[CharacterSelector] Auto-loaded character from PlayerPrefs: " + savedClass);
                }
            }
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

        // Nếu đang test local, có thể lưu lại tên class để auto load
        PlayerPrefs.SetString("AutoSelectedCharacter", character.name);
        PlayerPrefs.Save();
    }

    public static void DestroySelector()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}
