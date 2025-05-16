using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHandler : MonoBehaviour
{
    public bool isDashing = false;

    [Header("Dash Settings")]
    public float dashSpeed = 30f;
    [HideInInspector] public Vector2 dashDirection;
    [HideInInspector] public Vector2 lastMoveDirection = Vector2.right;
    public CharacterData characterData;
    public WeaponData weaponData;
    public PlayerController movement;
    public float interactionDistance = 2f;
    private IInteractable currentInteractable;
    public ActiveSkillCard activeSkill;
    private IActiveSkill activeSkillEffect;
    private float skillCooldownTimer;

    [Header("Stats")]
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentMana;
    [HideInInspector] public float currentRecovery;
    [HideInInspector] public float currentMight;
    [HideInInspector] public float currentProjectileSpeed;
    [HideInInspector] public float currentMagnet;
    [HideInInspector] public float currentCooldownReduction;

    PlayerCollector collector;

    [System.Serializable]
    public class LevelRange
    {
        public int startLevel;
        public int endLevel;
        public int expCapIncrease;
    }

    [Header("UI")]
    public Slider healthSlider;
    public Slider manaSlider;

    [Header("I-Frames")]
    public float invincibilityDuration;
    private float invincibilityTimer;
    private bool isInvincible;

    [Header("Level Ranges")]
    public List<LevelRange> levelRanges;

    public int weaponId;
    public int itemId;

    private WeaponBase currentWeapon;

    public void Awake()
    {
        characterData = CharacterSelector.LoadData();

        collector = GetComponentInChildren<PlayerCollector>();
        movement = GetComponent<PlayerController>();
        movement.currentMoveSpeed = characterData.MoveSpeed;

        currentHealth = characterData.MaxHealth;
        currentMana = characterData.MaxMana;
        currentRecovery = characterData.Recovery;
        currentMight = characterData.Might;
        currentProjectileSpeed = characterData.ProjectileSpeed;
        currentMagnet = characterData.Magnet;
        currentCooldownReduction = characterData.CooldownReduction;

        collector.SetRadius(currentMagnet);
        movement.SetAnimator(characterData.AnimationController);

        EquipWeapon(weaponData); 
    }

    void Start()
    {
        Canvas playerCanvas = GetComponentInChildren<Canvas>(true);
        if (playerCanvas != null)
        {
            playerCanvas.worldCamera = Camera.main;
            playerCanvas.sortingLayerName = "Ui";
        }

        HealthBarUI ui = FindObjectOfType<HealthBarUI>();
        if (ui != null)
        {
            ui.character = this;
        }
        if (activeSkill != null)
        {
            GameObject skillObj = Instantiate(activeSkill.skillEffectPrefab, transform);
            activeSkillEffect = skillObj.GetComponent<IActiveSkill>();
            skillCooldownTimer = 0f;
        }

        UpdateHealthBar();
    }

    void Update()
    {
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
        else if (isInvincible)
            isInvincible = false;

        Recover();

        if (Input.GetMouseButtonDown(0) && currentWeapon != null)
        {
            currentWeapon.Attack(this);
        }
        if (Input.GetKeyDown(KeyCode.E) )
        {
            TryInteract();
        }
        if (Input.GetKeyDown(KeyCode.Q) && skillCooldownTimer <= 0f && activeSkillEffect != null)
        {
            activeSkillEffect.Activate(this);
            skillCooldownTimer = activeSkill.cooldown;
        }
        if (skillCooldownTimer > 0f)
        {
            skillCooldownTimer -= Time.deltaTime;
        }

    }

    public void TakeDamage(float dmg)
    {
        if (isInvincible) return;

        currentHealth -= dmg;
        invincibilityTimer = invincibilityDuration;
        isInvincible = true;

        if (currentHealth <= 0)
            Die();

        UpdateHealthBar();
        Debug.Log($"[TEST DAMAGE] HP: {currentHealth}");
    }

    [ContextMenu("testdie")]
    public virtual void Die() { }

    void Recover()
    {
        if (currentRecovery == 0 || currentHealth >= characterData.MaxHealth) return;

        currentHealth += currentRecovery * Time.deltaTime;
        if (currentHealth > characterData.MaxHealth)
            currentHealth = characterData.MaxHealth;

        UpdateHealthBar();
    }

    public void EquipWeapon(WeaponData newData)
    {
        DropWeapon();

        GameObject wp = Instantiate(newData.weaponPrefab, transform.position, Quaternion.identity, transform);
        currentWeapon = wp.GetComponent<WeaponBase>();

        currentWeapon.weaponData = newData;
        currentWeapon.damage = newData.damage;
        currentWeapon.cooldown = newData.cooldown;
        currentWeapon.isEquipped = true;

        Animator anim = wp.GetComponent<Animator>();
        if (anim != null && newData.animatorController != null)
            anim.runtimeAnimatorController = newData.animatorController;

        SpriteRenderer sr = wp.GetComponent<SpriteRenderer>();
        if (sr != null && newData.weaponSprite != null)
            sr.sprite = newData.weaponSprite;
    }

    private void DropWeapon()
    {
        if (currentWeapon == null || currentWeapon.weaponData == null)
            return;

        WeaponData oldData = currentWeapon.weaponData;

        GameObject dropped = Instantiate(oldData.weaponPrefab, transform.position, Quaternion.identity);

        PickupWeapon pickup = dropped.GetComponent<PickupWeapon>();
        if (pickup != null)
            pickup.weaponData = oldData;

        Destroy(currentWeapon.gameObject);
        currentWeapon = null;
    }




    public void AcquirePassiveItem(GameObject item)
    {
        GameObject spawnedItem = Instantiate(item, transform.position, Quaternion.identity);
        spawnedItem.transform.SetParent(this.transform);
        itemId += 1;
    }
    public void SetActiveSkill(ActiveSkillCard skill)
    {
        activeSkill = skill;
        if (activeSkillEffect != null)
            Destroy(((MonoBehaviour)activeSkillEffect).gameObject);

        GameObject go = Instantiate(skill.skillEffectPrefab, transform);
        activeSkillEffect = go.GetComponent<IActiveSkill>();
    }
    public void SetInvincible(bool value)
    {
        isInvincible = value;
        invincibilityTimer = value ? Mathf.Infinity : 0f;
    }

    public void UpdateHealthBar()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out var interactable))
        {
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractable>(out var interactable) && interactable == currentInteractable)
        {
            currentInteractable = null;
        }
    }
    void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f); // Bán kính detect

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable) && interactable.CanInteract())
            {
                interactable.Interact();
                break;
            }
        }
    }

}
