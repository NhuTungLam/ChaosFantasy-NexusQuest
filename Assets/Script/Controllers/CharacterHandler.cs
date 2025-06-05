using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DungeonSystem;

public class CharacterHandler : MonoBehaviourPun
{
    public PlayerProfile profile; // Account-level persistent data
    public DungeonPlayerState state;     // In-room transient combat state

    public CharacterData characterData;
    public WeaponData weaponData;
    public IMovementController movement;
    public float interactionDistance = 2f;
    private IInteractable currentInteractable;
    public ActiveSkillCard activeSkill;
    private IActiveSkill activeSkillEffect;
    private float skillCooldownTimer;
    public Transform weaponHolder;

    public bool isDashing = false;
    private List<PassiveSkillCard> passiveSkills = new List<PassiveSkillCard>();

    [Header("Dash Settings")]
    public float dashSpeed = 30f;
    [HideInInspector] public Vector2 dashDirection;
    [HideInInspector] public Vector2 lastMoveDirection = Vector2.right;

    [Header("Interaction")]
    public LayerMask interactableLayer;

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
        if (GetComponent<Rigidbody2D>() == null)
            Debug.LogError("CharacterHandler: Missing Rigidbody2D");
    }

    void Start()
    {
        Canvas playerCanvas = GetComponentInChildren<Canvas>(true);
        if (playerCanvas != null)
        {
            playerCanvas.worldCamera = Camera.main;
            playerCanvas.sortingLayerName = "Ui";
        }

        if (photonView == null || photonView.IsMine)
        {
            HealthBarUI ui = FindObjectOfType<HealthBarUI>();
            if (ui != null)
            {
                ui.character = this;
            }
        }

        if (activeSkill != null)
        {
            GameObject skillObj = Instantiate(activeSkill.skillEffectPrefab, transform);
            activeSkillEffect = skillObj.GetComponent<IActiveSkill>();
            skillCooldownTimer = 0f;
        }

        // Initialize player state from profile
        if (profile != null)
        {
            Position playerPos = new Position
            {
                x = transform.position.x,
                y = transform.position.y
            };

            state = new DungeonPlayerState
            {
                userId = profile.userId,
                @class = profile.@class,
                hp = currentHealth,
                mana = currentMana,
                position = playerPos
            };

        }
    }


    void Update()
    {
        if (photonView != null && !photonView.IsMine) return;

        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
        else if (isInvincible)
            isInvincible = false;

        Recover();

        if (Input.GetMouseButtonDown(0) && currentWeapon != null)
        {
            currentWeapon.Attack(this);
        }

        if (Input.GetKeyDown(KeyCode.E))
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

        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(10);
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

        Debug.Log($"[TEST DAMAGE] HP: {currentHealth}");
    }

    [ContextMenu("testdie")]
    public virtual void Die()
    {
        if (movement is PlayerController pc)
            pc.PlayDieAnimation();
        else if (movement is PlayerOffController poc)
            poc.PlayDieAnimation();
    }

    void Recover()
    {
        if (currentRecovery == 0 || currentHealth >= characterData.MaxHealth) return;

        currentHealth += currentRecovery * Time.deltaTime;
        if (currentHealth > characterData.MaxHealth)
            currentHealth = characterData.MaxHealth;
    }

    public void EquipWeapon(WeaponData newData)
    {
        DropWeapon();

        GameObject wp = Instantiate(newData.weaponPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);

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

        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
    }

    private void DropWeapon()
    {
        if (currentWeapon == null || currentWeapon.weaponData == null) return;

        WeaponData oldData = currentWeapon.weaponData;
        GameObject dropped = Instantiate(oldData.weaponPrefab, transform.position, Quaternion.identity);

        if (dropped.TryGetComponent(out PickupWeapon pickup))
        {
            pickup.weaponData = oldData;
        }

        if (dropped.TryGetComponent(out WeaponBase weaponBase))
        {
            weaponBase.weaponData = oldData;
            weaponBase.SetFromData(oldData);
        }

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

        GameObject go = Instantiate(skill.skillEffectPrefab);
        activeSkillEffect = go.GetComponent<IActiveSkill>();
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
        invincibilityTimer = value ? Mathf.Infinity : 0f;
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionDistance, interactableLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable) && interactable.CanInteract())
            {
                interactable.Interact();
                break;
            }
        }
    }

    public void ApplyPassiveSkill(PassiveSkillCard skill)
    {
        if (skill == null) return;

        passiveSkills.Add(skill);
        currentMight += skill.bonusDamage;
        Debug.Log($"[Passive Skill] Acquired: {skill.skillName} (+{skill.bonusDamage} dmg, +{skill.bonusSpeed} speed)");
    }

    public IEnumerator FireProjectileDelayed(Transform origin, Vector2 direction, float delay, GameObject projectilePrefab, float damage)
    {
        yield return new WaitForSeconds(delay);
        GameObject proj = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();
        projectile.Initialize(direction, damage);
    }

    public void Init(CharacterData data)
    {
        characterData = data;

        if (characterData == null)
        {
            Debug.LogError("CharacterHandler.Init(): characterData is null");
            return;
        }

        collector = GetComponentInChildren<PlayerCollector>();
        if (collector != null)
            collector.SetRadius(characterData.Magnet);

        movement = GetComponent<IMovementController>();
        if (movement != null)
        {
            movement.currentMoveSpeed = characterData.MoveSpeed;
            movement.SetAnimator(characterData.AnimationController);
        }

        currentHealth = characterData.MaxHealth;
        currentMana = characterData.MaxMana;
        currentRecovery = characterData.Recovery;
        currentMight = characterData.Might;
        currentProjectileSpeed = characterData.ProjectileSpeed;
        currentMagnet = characterData.Magnet;
        currentCooldownReduction = characterData.CooldownReduction;

        weaponData = characterData.StartingWeapon;
        if (weaponData != null)
            EquipWeapon(weaponData);
    }
}
