using System.Collections;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private float accumulatedDamage = 0f;
    private float animationCooldown = 1.5f;
    private bool isAccumulating = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    public void TakeDamage(float dmg)
    {
        DamagePopUp.Create(transform.position, Mathf.RoundToInt(dmg));
        StartCoroutine(FlashCoroutine());

        accumulatedDamage += dmg;

        if (!isAccumulating)
            StartCoroutine(DelayedAnimationTrigger());
    }

    private IEnumerator FlashCoroutine()
    {
        _spriteRenderer.material.SetFloat("_FlashAmount", 1f);
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.material.SetFloat("_FlashAmount", 0f);
    }

    private IEnumerator DelayedAnimationTrigger()
    {
        isAccumulating = true;
        yield return new WaitForSeconds(0.2f);

        int hurtPhase = 0;
        if (accumulatedDamage > 20) hurtPhase = 1;
        if (accumulatedDamage > 50) hurtPhase = 2;

        _animator.Play($"dummy_hurt_{hurtPhase}");
        accumulatedDamage = 0f;

        yield return new WaitForSeconds(animationCooldown); // prevent new trigger too soon
        isAccumulating = false;
    }
}
