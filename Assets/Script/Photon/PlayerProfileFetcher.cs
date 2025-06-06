using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;


public class PlayerProfileFetcher : MonoBehaviour
{
    public static PlayerProfileFetcher Instance { get; private set; }
    public static PlayerProfile CurrentProfile;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void FetchProfile(int userId)
    {
        StartCoroutine(FetchProfileCoroutine(userId));
    }

    IEnumerator FetchProfileCoroutine(int userId)
    {
        string url = $"http://localhost:5058/api/profile/{userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                CurrentProfile = JsonUtility.FromJson<PlayerProfile>(request.downloadHandler.text);
                Debug.Log($"[Profile] Class: {CurrentProfile.@class}, Level: {CurrentProfile.level}, Gold: {CurrentProfile.gold}");
            }
            else
            {
                Debug.LogError("Failed to fetch profile: " + request.error);
            }
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

        using (UnityWebRequest request = new UnityWebRequest("http://localhost:5000/api/profile/update", "POST"))
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
                Debug.LogError("Failed to update profile: " + request.error);
            }
        }
    }
}
