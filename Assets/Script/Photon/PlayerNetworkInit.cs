using UnityEngine;
public class PlayerNetworkInit : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    void Start()
    {
        CharacterData selectedClass = CharacterSelector.LoadData();


        if (selectedClass == null)
        {
            Debug.LogError("No class selected! CharacterSelector returned null.");
            return;
        }

        if (spriteRenderer) spriteRenderer.sprite = selectedClass.PlayerSprite;
        if (animator) animator.runtimeAnimatorController = selectedClass.AnimationController;

        if (selectedClass.StartingWeapon)
        {
            Instantiate(selectedClass.StartingWeapon, transform.position, Quaternion.identity, transform);
        }
    }
}