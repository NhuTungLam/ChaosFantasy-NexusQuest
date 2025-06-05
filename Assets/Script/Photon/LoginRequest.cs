using TMPro;
using UnityEngine;

public class LoginRequest : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject classSelectPanel;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text resultText;

    public static string currentUsername;

    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            resultText.text = "Vui lòng nhập đầy đủ username và password";
            return;
        }

        if (MockAuthService.Login(username, password, out var profile))
        {
            currentUsername = username;
            resultText.text = "Đăng nhập thành công!";

            if (!string.IsNullOrEmpty(profile.@class))
            {
                var data = Resources.Load<CharacterData>("Characters/" + profile.@class);
                if (data != null)
                {
                    CharacterSelector.Instance.SelectCharacter(data);
                }
            }

            loginPanel.SetActive(false);
            classSelectPanel.SetActive(true);
        }
        else
        {
            resultText.text = "Sai tài khoản hoặc mật khẩu.";
        }
    }
}
