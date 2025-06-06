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
}

public class RegisterRequest : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text resultText;

    public void OnRegisterClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            resultText.text = "Vui lòng nhập đầy đủ username và password";
            return;
        }

        StartCoroutine(RegisterCoroutine(username, password));
    }

    IEnumerator RegisterCoroutine(string username, string password)
    {
        var payload = new RegisterPayload { username = username, password = password };
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
                resultText.text = "Đăng ký thành công!";
            }
            else if (request.responseCode == 409)
            {
                resultText.text = "Username đã tồn tại.";
            }
            else
            {
                Debug.LogError($"[Register] Error {request.responseCode}: {request.downloadHandler.text}");
                resultText.text = "Đăng ký thất bại (lỗi khác).";
            }

        }
    }

}
