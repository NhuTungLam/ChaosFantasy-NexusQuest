using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ClassSelectUI : MonoBehaviour
{
    public CharacterData[] availableCharacters;
    private int currentCharIndex = 0;

    public TextMeshProUGUI charSelTitle;
    public Image charSelPortrait;
    public CharacterData CurrentCharacterData;

    public SceneController sceneController;
    private void Start()
    {
        currentCharIndex = 0;
        ChangeCurrentClass();
    }
    /*public void SelectClass(int index)
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
    }*/
    public void GoLeft()
    {
        currentCharIndex--;
        if (currentCharIndex < 0)
            currentCharIndex = availableCharacters.Length - 1;
        ChangeCurrentClass();
    }
    public void GoRight()
    {
        currentCharIndex++;
        if (currentCharIndex >= availableCharacters.Length)
            currentCharIndex = 0;
        ChangeCurrentClass();
    }
    public void ChangeCurrentClass()
    {
        CurrentCharacterData = availableCharacters[currentCharIndex];
        charSelTitle.text = CurrentCharacterData.name;
        charSelPortrait.sprite = CurrentCharacterData.PlayerSprite;
    }
    public void SelectClass()
    {
        CharacterSelector.Instance.characterData = CurrentCharacterData;
        sceneController.StartGame();
    }

}
