using UnityEngine;

public static class MockAuthService
{
    public static bool Register(string username, string password)
    {
        string key = "user_" + username;
        if (PlayerPrefs.HasKey(key))
            return false;

        PlayerProfile profile = new PlayerProfile
        {
            username = username,
            password = password,
            @class = null
        };

        PlayerPrefs.SetString(key, JsonUtility.ToJson(profile));
        PlayerPrefs.Save();
        return true;
    }

    public static bool Login(string username, string password, out PlayerProfile profile)
    {
        string key = "user_" + username;
        if (!PlayerPrefs.HasKey(key))
        {
            profile = null;
            return false;
        }

        profile = JsonUtility.FromJson<PlayerProfile>(PlayerPrefs.GetString(key));
        return profile.password == password;
    }

    public static void SetClass(string username, string className)
    {
        string key = "user_" + username;
        PlayerProfile profile = JsonUtility.FromJson<PlayerProfile>(PlayerPrefs.GetString(key));
        profile.@class = className;
        PlayerPrefs.SetString(key, JsonUtility.ToJson(profile));
        PlayerPrefs.Save();
    }

    public static string GetClass(string username)
    {
        string key = "user_" + username;
        if (!PlayerPrefs.HasKey(key)) return null;
        PlayerProfile profile = JsonUtility.FromJson<PlayerProfile>(PlayerPrefs.GetString(key));
        return profile.@class;
    }
}
