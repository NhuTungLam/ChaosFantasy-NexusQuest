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
    public GameObject loginPanel;
    public GameObject classSelectPanel;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text resultText;

    public static string currentUsername;
    public static int currentUserId;

    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            resultText.text = "Vui lòng nhập đầy đủ username và password";
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
                resultText.text = "Đăng nhập thành công!";

                loginPanel.SetActive(false);
                classSelectPanel.SetActive(true);
            }
            else
            {
                resultText.text = "Sai tài khoản hoặc mật khẩu.";
            }
        }
    }
}
