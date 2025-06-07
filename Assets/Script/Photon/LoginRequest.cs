using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

[System.Serializable]
public class LoginPayload
{
    public string username;
    public string password;
}

[System.Serializable]
public class LoginResult
{
    public int id;
}

public class LoginRequest : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public static string currentUsername;
    public static int currentUserId;

    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBoard.Show("Username or Password cannot be empty!");
            return;
        }

        StartCoroutine(LoginCoroutine(username, password));
    }

    IEnumerator LoginCoroutine(string username, string password)
    {
        var payload = new LoginPayload { username = username, password = password };
        string json = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest("http://localhost:5058/api/account/login", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginResult res = JsonUtility.FromJson<LoginResult>(request.downloadHandler.text);
                currentUsername = username;
                currentUserId = res.id;
                MessageBoard.Show("Login Successfully!");

                PlayerProfileFetcher.Instance.FetchProfile(currentUserId, (profile) =>
                {
                    MessageBoard.Show($"Loaded profile for {currentUsername}: Lv {profile.level}, Gold {profile.gold}");
                    MainMenu.Instance.HideLogin();
                });

            }
            else
            {
                MessageBoard.Show("Wrong username or password!");
            }
        }
    }
}
