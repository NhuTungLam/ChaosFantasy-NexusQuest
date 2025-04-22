using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class RegisterRequest : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text resultText;

    public string registerUrl = "http://localhost/chaosapi/register.php";

    public void OnRegisterClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            resultText.text = "Vui lòng nhập đầy đủ username và password";
            return;
        }

        StartCoroutine(SendRegisterRequest(username, password));
    }

    IEnumerator SendRegisterRequest(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post(registerUrl, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            resultText.text = "Lỗi: " + www.error;
        }
        else
        {
            resultText.text = www.downloadHandler.text;
        }
    }
}
