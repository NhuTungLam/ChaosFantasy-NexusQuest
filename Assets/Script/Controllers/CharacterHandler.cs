using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHandler : MonoBehaviour
{
    public CharacterData characterData;
    public WeaponData weaponData;
    public PlayerController movement;

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

    public void EquipWeapon(WeaponData data)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);

        GameObject wp = Instantiate(data.weaponPrefab, transform.position, Quaternion.identity, transform);
        currentWeapon = wp.GetComponent<WeaponBase>();

        // Set damage và cooldown từ data
        currentWeapon.damage = data.damage;
        currentWeapon.cooldown = data.cooldown;

        // Gán animator nếu prefab có Animator
        Animator animator = wp.GetComponent<Animator>();
        if (animator != null && data.animatorController != null)
        {
            animator.runtimeAnimatorController = data.animatorController;
        }

        // Gán sprite nếu có
        SpriteRenderer sr = wp.GetComponent<SpriteRenderer>();
        if (sr != null && data.weaponSprite != null)
        {
            sr.sprite = data.weaponSprite;
        }
    }


    public void AcquirePassiveItem(GameObject item)
    {
        GameObject spawnedItem = Instantiate(item, transform.position, Quaternion.identity);
        spawnedItem.transform.SetParent(this.transform);
        itemId += 1;
    }

    public void UpdateHealthBar()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }
}
