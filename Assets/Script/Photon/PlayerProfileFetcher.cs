using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class PlayerProfileFetcher : MonoBehaviour
{
    public static PlayerProfileFetcher Instance { get; private set; }
    public static PlayerProfile CurrentProfile;

    private const string PlayerPrefsUserIdKey = "LastLoggedInUserId";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Try auto-load
            if (PlayerPrefs.HasKey(PlayerPrefsUserIdKey))
            {
                int savedId = PlayerPrefs.GetInt(PlayerPrefsUserIdKey);
                Debug.Log($"[ProfileFetcher] Auto-fetching saved user ID: {savedId}");
                FetchProfile(savedId);
            }
        }
        else Destroy(gameObject);
    }

    public void FetchProfile(int userId, System.Action<PlayerProfile> onDone = null)
    {
        StartCoroutine(FetchProfileCoroutine(userId, onDone));
    }

    IEnumerator FetchProfileCoroutine(int userId, System.Action<PlayerProfile> onDone = null)
    {
        string url = $"http://localhost:5058/api/profile/{userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                CurrentProfile = JsonUtility.FromJson<PlayerProfile>(request.downloadHandler.text);
                PlayerPrefs.SetInt(PlayerPrefsUserIdKey, userId); // Save to PlayerPrefs
                PlayerPrefs.Save();

                Debug.Log($"[Profile] Class: {CurrentProfile.@class}, Level: {CurrentProfile.level}, Gold: {CurrentProfile.gold}");

                onDone?.Invoke(CurrentProfile);

            }
            else
            {
                Debug.LogError("Failed to fetch profile: " + request.error);
                onDone?.Invoke(null);
            }
        }
        if (MainMenu.Instance != null)
        {
            MainMenu.Instance.ShowPlayerProfile(CurrentProfile);
        }
    }

    public void UpdateProfile()
    {
        if (CurrentProfile != null)
        {
            StartCoroutine(UpdateProfileCoroutine(CurrentProfile));
        }
    }

    IEnumerator UpdateProfileCoroutine(PlayerProfile profile)
    {
        string json = JsonUtility.ToJson(profile);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest("http://localhost:5058/api/profile/update", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Profile] Updated successfully.");
            }
            else
            {
                MainMenu.Instance.ShowPlayerProfile(CurrentProfile);
            }
        }
    }

    public void SignOut()
    {
        Debug.Log("[Profile] Signed out.");

        CurrentProfile = null;
        PlayerPrefs.DeleteKey(PlayerPrefsUserIdKey);
        PlayerPrefs.Save();

        // Optional: go back to login screen or main menu
        if (MainMenu.Instance != null)
        {
            MessageBoard.Show("You have signed out");
            MainMenu.Instance.ShowPlayerProfile(null);
            //MainMenu.Instance.GoToLoginScreen(); // ← implement this if needed
        }
    }
}
