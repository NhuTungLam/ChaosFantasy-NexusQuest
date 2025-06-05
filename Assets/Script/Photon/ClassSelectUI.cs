using UnityEngine;
using UnityEngine.SceneManagement;

public class ClassSelectUI : MonoBehaviour
{
    public CharacterData[] availableCharacters; // Kéo thả trong Inspector theo thứ tự Knight, Archer, Mage

    public void SelectClass(int index)
    {
        if (index < 0 || index >= availableCharacters.Length) return;

        var character = availableCharacters[index];
        CharacterSelector.Instance.SelectCharacter(character);

        if (!string.IsNullOrEmpty(LoginRequest.currentUsername))
        {
            MockAuthService.SetClass(LoginRequest.currentUsername, character.name);
        }

        Debug.Log("Chọn class thành công: " + character.name);
        SceneManager.LoadScene("Nexus");
    }
}
