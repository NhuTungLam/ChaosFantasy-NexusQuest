using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

[System.Serializable]
public class RegisterPayload
{
    public string username;
    public string password;
    public string email;
}

public class RegisterRequest : MonoBehaviour
{
    public static string currentUsername;
    public static int currentUserId;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    public void OnRegisterClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBoard.Show("Username or Password cannot be empty!");
            return;
        }

        StartCoroutine(RegisterCoroutine(username, password));
    }

    IEnumerator RegisterCoroutine(string username, string password)
    {
        var payload = new RegisterPayload { username = username, password = password,email = "lam@" };
        string json = JsonUtility.ToJson(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest("http://localhost:5058/api/account/register", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse new user ID
                var loginResult = JsonUtility.FromJson<LoginResult>(request.downloadHandler.text);
                currentUsername = username;
                currentUserId = loginResult.id;

                MessageBoard.Show("Registration successful!");

                // Fetch the newly created profile
                PlayerProfileFetcher.Instance.FetchProfile(currentUserId, (profile) =>
                {
                    if (profile != null)
                    {
                        MessageBoard.Show($"Profile created for {currentUsername}");
                        MainMenu.Instance.HideRegister();
                    }
                    else
                    {
                        MainMenu.Instance.ShowLoginFromRegister();
                        MessageBoard.Show("Profile fetch failed. Try logging in with the new username and password.");
                        Debug.LogError($"[Register] Profile fetch returned null for user ID {currentUserId}.");
                    }
                });

            }
            else if (request.responseCode == 409)
            {
                MessageBoard.Show("Username already exists!");
            }
            else
            {
                Debug.LogError($"[Register] Error {request.responseCode}: {request.downloadHandler.text}");
                MessageBoard.Show("Failed. Unknown Error!");
            }
        }
    }


}
