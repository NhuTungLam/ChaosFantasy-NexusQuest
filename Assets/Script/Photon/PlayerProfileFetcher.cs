using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Photon.Pun;
using static DungeonApiClient;
using UnityEngine.SceneManagement;
public class PlayerProfileFetcher : MonoBehaviour
{
    public static PlayerProfileFetcher Instance { get; private set; }
    public static PlayerProfile CurrentProfile;
    public int baseReward = 20;
    public int goldPerKill = 3;
    public int goldPerRoom = 5;
    public int deathPenalty = 4;
    private const string PlayerPrefsUserIdKey = "LastLoggedInUserId";
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.activeSceneChanged += (prev, current) =>
            {
                if (current.name == "Login")
                {
                    MainMenu.Instance.ShowPlayerProfile(CurrentProfile);
                }
            };
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
            if (PlayerPrefs.HasKey("AuthToken"))
            {
                AuthToken.token = PlayerPrefs.GetString("AuthToken");   
            }
            if (request.result == UnityWebRequest.Result.Success)
            {
                CurrentProfile = JsonUtility.FromJson<PlayerProfile>(request.downloadHandler.text);
                PhotonNetwork.NickName = CurrentProfile.username;

                PlayerPrefs.SetInt(PlayerPrefsUserIdKey, userId); // Save to PlayerPrefs
                PlayerPrefs.Save();


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
            Debug.LogWarning(CurrentProfile.gold);
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

            if (!string.IsNullOrEmpty(AuthToken.token))
                request.SetRequestHeader("Authorization", "Bearer " + AuthToken.token);


            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[Profile] Updated successfully.");
            }
            else
            {
                Debug.LogError($"[UpdateProfile] Failed with code {request.responseCode}: {request.error}");
                Debug.LogError(request.downloadHandler.text);
            }
        }
    }


    public void SignOut()
    {
        Debug.Log("[Profile] Signed out.");

        CurrentProfile = null;
        PlayerPrefs.DeleteKey(PlayerPrefsUserIdKey);
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.Save();

        // Optional: go back to login screen or main menu
        if (MainMenu.Instance != null)
        {
            MessageBoard.Show("You have signed out");
            MainMenu.Instance.ShowPlayerProfile(null);
            //MainMenu.Instance.GoToLoginScreen(); // ← implement this if needed
        }
    }
    private static (int newLevel, int remainingExp) CalculateLevelAndRemainingExp(int currentLevel, int expToAdd)
    {
        int level = currentLevel;
        int exp = expToAdd;

        while (true)
        {
            // Tính EXP cần thiết để lên cấp tiếp theo
            int requiredExp = 50 + (level - 1) * 20;

            // Nếu đủ exp để lên level
            if (exp >= requiredExp)
            {
                exp -= requiredExp;
                level++;
            }
            else
            {
                break;
            }
        }

        return (level, exp);
    }

    public static void UpdateReward(int gold , int exp)
    {
        if (CurrentProfile == null) return;
        int level = CurrentProfile.level ;
        int currentExp = CurrentProfile.exp ;
        CurrentProfile.gold += gold;
        var cal = CalculateLevelAndRemainingExp(level, currentExp + exp);
        CurrentProfile.level = cal.newLevel;
        CurrentProfile.exp = cal.remainingExp;
    }
    public int CalculateExp(PlayerProgressDTO dto, int roomsCleared)
    {
        int baseExp = 10;
        int killBonus = dto.enemyKills * 2;
        int roomBonus = roomsCleared * 3;
        int deathPenalty = dto.deathCount * 2;

        return Mathf.Max(0, baseExp + killBonus + roomBonus - deathPenalty);
    }

    public int CalculateGold(PlayerProgressDTO dto, int roomsCleared)
    {
        int reward = baseReward
            + (dto.enemyKills * goldPerKill)
            + (roomsCleared * goldPerRoom)
            - (dto.deathCount * deathPenalty);

        return Mathf.Max(0, reward); // không cho < 0
    }
}
