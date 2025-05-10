using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class LoginRequest : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject classSelectPanel;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text resultText;

    public string loginUrl = "http://localhost/chaosapi/login.php";

    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            resultText.text = "Vui lòng nhập đầy đủ username và password";
            return;
        }

        StartCoroutine(SendLoginRequest(username, password));
    }

    IEnumerator SendLoginRequest(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post(loginUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            resultText.text = "Lỗi: " + www.error;
        }
        else
        {
            string json = www.downloadHandler.text;

            if (json.StartsWith("{"))
            {
                resultText.text = "Đăng nhập thành công!";
                Debug.Log("Dữ liệu nhận được: " + json);

                PlayerProfile profile = JsonUtility.FromJson<PlayerProfile>(json);
                Debug.Log("Class: " + profile.@class + ", Level: " + profile.level);
            }
            else
            {
                resultText.text = json;
            }
        }
    }
}

[System.Serializable]
public class PlayerProfile
{
    public string @class;
    public int level;
    public int exp;
    public int gold;
}