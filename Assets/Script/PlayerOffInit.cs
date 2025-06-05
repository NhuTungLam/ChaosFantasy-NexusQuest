using UnityEngine;

public class PlayerOffInit : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Transform weaponHolder;

    void Start()
    {
        CharacterData selectedClass = CharacterSelector.LoadData();

        if (selectedClass == null)
        {
            Debug.LogError("No class selected!");
            return;
        }

        if (spriteRenderer != null)
            spriteRenderer.sprite = selectedClass.PlayerSprite;

        if (animator != null)
            animator.runtimeAnimatorController = selectedClass.AnimationController;

        // G?i ?úng h? th?ng chu?n: dùng CharacterHandler.Init
        var handler = GetComponent<CharacterHandler>();
        if (handler != null)
        {
            handler.weaponHolder = weaponHolder; // Gán ?? init bi?t n?i g?n v? khí
            handler.Init(selectedClass);
        }
        else
        {
            Debug.LogWarning("No CharacterHandler found on PlayerOffline prefab!");
        }
    }
}
