using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DungeonSystem;
using UnityEngine.SceneManagement;

public class CharacterHandler : MonoBehaviourPun
{
    public PlayerProfile profile; // Account-level persistent data
    public DungeonPlayerState state;     // In-room transient combat state

    public CharacterData characterData;
    public IMovementController movement;
    public float interactionDistance = 2f;
    public SkillCardBase activeSkill;
    
    private float skillCooldownTimer;
    public Transform weaponHolder;
    public delegate float DamageModifier(float dmg);
    public event DamageModifier OnBeforeTakeDamage;

    public bool isDashing = false;
    private List<SkillCardBase> passiveSkills = new List<SkillCardBase>();

    [Header("Dash Settings")]
    public float dashSpeed = 30f;
    [HideInInspector] public Vector2 dashDirection;
    [HideInInspector] public Vector2 lastMoveDirection = Vector2.right;
   
    [Header("Shield Settings")]
    [HideInInspector] public bool isBlocking = false;

    [Header("Interaction")]
    public LayerMask interactableLayer;
    public IInteractable currentInteract;
    private float throttleInteractUpdateInterval = 0f;

    [Header("Stats")]
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentMana;
    [HideInInspector] public float currentRecovery;
    [HideInInspector] public float currentMight;
    [HideInInspector] public float baseMight;
    [HideInInspector] public float currentProjectileSpeed;
    [HideInInspector] public float currentMagnet;
    [HideInInspector] public float currentCooldownReduction;
    [HideInInspector] public float baseCritRate;
    [HideInInspector] public float currentCritRate;

    [HideInInspector] public float baseCritDamage;
    [HideInInspector] public float currentCritDamage;
    [Header("im losing my mind wth")]
    public RectTransform hp_cover, mana_cover;
    public TextMeshProUGUI hp_text;
    PlayerCollector collector;
    public Camera mainCamera;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

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

    public WeaponBase currentWeapon;

    public void Awake()
    {
        if (GetComponent<Rigidbody2D>() == null)
            Debug.LogError("CharacterHandler: Missing Rigidbody2D");
    }

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
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

        // Initialize player state from profile
        //if (profile != null)
        //{
        //    state = new DungeonPlayerState
        //    {
        //        currentClass = profile.@class,
        //        currentCard = activeSkill?.name ?? "None",
        //        currentWeapon = weaponData?.weaponName ?? "None",
        //        hp = currentHealth,
        //        mana = currentMana,
        //        stageLevel = PlayerPrefs.GetInt("CurrentStage", 1)
        //    };
        //}

        TryAttachStatBar();
        SceneManager.activeSceneChanged += (s, a) => TryAttachStatBar();
    }

    void TryAttachStatBar()
    {
        var statPanel = GameObject.FindGameObjectWithTag("Hpbar");
        hp_cover = null;
        mana_cover = null;
        hp_text = null;
        if (statPanel != null)
        {
            hp_cover = statPanel.transform.Find("hp_bar/cover").GetComponent<RectTransform>();
            mana_cover = statPanel.transform.Find("mana_bar/cover").GetComponent<RectTransform>();
            hp_text = statPanel.transform.Find("hp_text").GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = mainCamera.orthographicSize - scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }

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

        if (Input.GetKeyDown(KeyCode.Q) && skillCooldownTimer <= 0f && activeSkill != null)
        {
            activeSkill.Activate(this);
        }

        if (skillCooldownTimer > 0f)
        {
            skillCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(10);
        }

        if (throttleInteractUpdateInterval > 0)
        {
            throttleInteractUpdateInterval -= Time.deltaTime;
        }
        else
        {
            GetClosestInteractable();
            throttleInteractUpdateInterval = 0.1f;
        }
    }
    
    public void TakeDamage(float dmg)
    {
        if (isInvincible) dmg=0;

        // Shield Block
        if (isBlocking)
            dmg *= 0.5f;

        // Passive damage modifiers (e.g., Mana Shield)
        if (OnBeforeTakeDamage != null)
            foreach (DamageModifier modifier in OnBeforeTakeDamage.GetInvocationList())
                dmg = modifier.Invoke(dmg);

        currentHealth -= dmg;
        invincibilityTimer = invincibilityDuration;
        isInvincible = true;

        currentHealth = Mathf.Clamp(currentHealth, 0f, characterData.MaxHealth);
        if (hp_cover != null  && photonView.IsMine) {
            hp_cover.localScale = new Vector3(GetCurrentHealthPercent(), 1, 1);
            hp_text.text = $"{currentHealth}/{characterData.MaxHealth}";
        }
        if (currentHealth == 0)
            Die();

    }
    public bool UseMana(float amount)
    {
        if (currentMana < amount)
            return false;

        currentMana -= amount;
        currentMana = Mathf.Clamp(currentMana, 0, characterData.MaxMana);
        if (mana_cover != null)
        mana_cover.localScale = new Vector3(currentMana/characterData.MaxMana, 1, 1);

        return true;
    }

    [ContextMenu("testdie")]
    public virtual void Die()
    {
        if (movement is PlayerController pc)
            pc.PlayDieAnimation();
        else if (movement is PlayerOffController poc)
            poc.PlayDieAnimation();
    }
    //public void RecalculateStats()
    //{
    //    currentMight = baseMight;
    //    currentCritRate = baseCritRate;
    //    currentCritDamage = baseCritDamage;

    //    foreach (var passive in passiveSkills)
    //    {
    //        currentCritRate += passive.bonusCritRate;
    //        currentCritDamage += passive.bonusCritDamage; 
    //    }

    //    foreach (var passive in passiveSkills)
    //    {
    //        currentMight += passive.bonusDamage;
    //    }
    //}

    void Recover()
    {
        if (currentRecovery == 0 || currentHealth >= characterData.MaxHealth) return;

        TakeDamage(-currentRecovery);
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

        if (dropped.TryGetComponent(out WeaponBase weaponBase))
        {
            weaponBase.weaponData = oldData;
            weaponBase.SetFromData(oldData);
        }

        Destroy(currentWeapon.gameObject);
        currentWeapon = null;
    }

    public void SetActiveSkill(SkillCardBase skill)
    {
        activeSkill = skill;
        skill.gameObject.transform.SetParent(transform, false);
    }
    public void SetPassiveSkill(SkillCardBase skill)
    {
        passiveSkills.Add(skill);
        skill.gameObject.transform.SetParent(transform, false);
        SpriteRenderer spriteRenderer = skill.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
        invincibilityTimer = value ? Mathf.Infinity : 0f;
    }

    void GetClosestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionDistance, interactableLayer);

        float closestDistance = float.MaxValue;
        IInteractable closestInteractable = null;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IInteractable>(out var interactable) && interactable.CanInteract())
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        if (currentInteract != closestInteractable)
        {
            //print(currentInteract + ", " + closestInteractable);
            currentInteract?.CancelInRangeAction(this);
            currentInteract = closestInteractable;
            currentInteract?.InRangeAction(this);
        }
        //print(currentInteract);
    }
    void TryInteract()
    {
        if (currentInteract != null)
            currentInteract.Interact(this);
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
        baseMight = characterData.Might;
        currentMight = baseMight;
        currentProjectileSpeed = characterData.ProjectileSpeed;
        currentMagnet = characterData.Magnet;
        currentCooldownReduction = characterData.CooldownReduction;
        baseCritRate = characterData.BaseCritRate;
        baseCritDamage = characterData.BaseCritDamage;

        currentCritRate = baseCritRate;
        currentCritDamage = baseCritDamage;
        TakeDamage(0);
        UseMana(0);
        if (characterData.StartingWeapon != null)
            EquipWeapon(characterData.StartingWeapon);
    }
    public void ApplyLoadSave(DungeonApiClient.PlayerProgressDTO playerloadinfo)
    {
        string className = playerloadinfo.currentClass;
        Debug.Log(className);
        CharacterData characterData = Resources.Load<CharacterData>("Characters/" + className);
        if (characterData == null)
        {
            Debug.LogError("❌ Không tìm thấy CharacterData: " + className);
            return;
        }
        Init(characterData);
        currentHealth = playerloadinfo.currentHp;
        Debug.LogWarning(currentHealth);
        currentMana = playerloadinfo.currentMana;
        TakeDamage(0);
        UseMana(0);
    }
    public float GetCurrentHealthPercent()
    {
        return currentHealth / characterData.MaxHealth;
    }


}
