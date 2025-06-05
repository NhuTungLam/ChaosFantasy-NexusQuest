using TMPro;
using UnityEngine;

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

        if (MockAuthService.Register(username, password))
        {
            resultText.text = "Đăng ký thành công!";
        }
        else
        {
            resultText.text = "Username đã tồn tại.";
        }
    }
}
