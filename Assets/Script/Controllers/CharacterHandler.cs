using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DungeonSystem;
using UnityEngine.SceneManagement;
using System.Linq;

public class CharacterHandler : MonoBehaviourPun
{
    private StatUI statUI;
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
    public float currentHealth;
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

    public Camera mainCamera;
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    [Header("Revive System")]
    [HideInInspector] public bool isDowned = false;
    public float reviveRange = 2.5f;
    public float reviveHoldTime = 3f;
    public float reviveHealthPercent = 0.5f;
    public int deadCount = 0;

    [Header("I-Frames")]
    public float invincibilityDuration;
    private float invincibilityTimer;
    private bool isInvincible;

    public WeaponBase currentWeapon;

    private SpriteRenderer _spriteRenderer;
    private ReviveSystem _reviveSystem;
    private Rigidbody2D _rb;
    public void Awake()
    {
        if (GetComponent<Rigidbody2D>() == null)
            Debug.LogError("CharacterHandler: Missing Rigidbody2D");
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _reviveSystem = GetComponentInChildren<ReviveSystem>();
        _reviveSystem.gameObject.SetActive(false);
        _rb = GetComponent<Rigidbody2D>();
        statUI = GetComponent<StatUI>();
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
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = mainCamera.orthographicSize - scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
        else if (isInvincible)
            isInvincible = false;
        if (photonView != null && !photonView.IsMine) return;

        if (!isDowned)
        {
            if (Input.GetMouseButton(0) && currentWeapon != null)
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
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            TakeDamage(40);
        }

        if (throttleInteractUpdateInterval > 0)
        {
            throttleInteractUpdateInterval -= Time.deltaTime;
        }
        else
        {
            GetClosestInteractable();
            throttleInteractUpdateInterval = 0.2f;
            if (currentRecovery > 0)
                Heal(currentRecovery);
        }

    }
    private IEnumerator FlashCoroutine()
    {
        //flashMaterial.SetColor(FlashColor, flashColor);
        _spriteRenderer.material.SetFloat("_FlashAmount", 1f);

        yield return new WaitForSeconds(0.15f);

        _spriteRenderer.material.SetFloat("_FlashAmount", 0f);
    }
    public void TakeDamage(float dmg)
    {
        if (isDowned) return;
        if (isInvincible) return;
        if (isBlocking) dmg *= 0.5f;
        if (OnBeforeTakeDamage != null)
        {
            foreach (DamageModifier modifier in OnBeforeTakeDamage.GetInvocationList())
                dmg = modifier.Invoke(dmg);
        }
        if (photonView != null && photonView.IsMine)
        {
            currentHealth -= dmg;
            currentHealth = Mathf.Clamp(currentHealth, 0f, characterData.MaxHealth);
            photonView.RPC("RPC_UpdateStatTeammateHP", RpcTarget.Others, currentHealth, characterData.MaxHealth);
            statUI.UpdateHp(currentHealth, characterData.MaxHealth);
        }

        if (dmg > 0)
        {
            invincibilityTimer = invincibilityDuration;
            isInvincible = true;
            DamagePopUp.Create(transform.position, Mathf.RoundToInt(dmg));
            StartCoroutine(FlashCoroutine());
        }

        if (currentHealth <= 0 && !isDowned && photonView.IsMine)
        {
            Knockdown();
            return;
        }

    }
    public void Heal(float amount)
    {
        if (photonView != null && photonView.IsMine)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, characterData.MaxHealth);
            photonView.RPC("RPC_UpdateStatTeammateHP", RpcTarget.Others, currentHealth, characterData.MaxHealth);
            statUI.UpdateHp(currentHealth, characterData.MaxHealth);
        }

        if (amount >= 10)
        {
            StartCoroutine(FlashCoroutine());
        }
    }
    public bool UseMana(float amount)
    {
        if (currentMana < amount)
            return false;

        if (photonView != null && photonView.IsMine)
        {
            currentMana -= amount;
            currentMana = Mathf.Clamp(currentMana, 0, characterData.MaxMana);
            photonView.RPC("RPC_UpdateStatTeammateMana", RpcTarget.Others, currentMana, characterData.MaxMana);
            statUI.UpdateMana(currentMana, characterData.MaxMana);
        }

        return true;
    }

    public void Knockdown()
    {
        CanMove(false);

        if (photonView.IsMine)
            PlayerStatTracker.Instance?.AddDeath();

        photonView.RPC("RPC_PlayKnockdown", RpcTarget.All);

        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                GameManager.Instance.ShowSummaryPanel();
            }
        }
    }



    [PunRPC]
    void RPC_PlayKnockdown()
    {
        if (isDowned) return;
        isDowned = true;
        currentHealth = 0;
        if (movement != null)
        {
            movement.PlayDieAnimation();
        }
        if (RoomSessionManager.Instance.IsRoomOwner())
        {
            PlayerManager.Instance.CheckDie(photonView.ViewID, true);
        }
        currentWeapon.gameObject.SetActive(false);
        _reviveSystem.gameObject.SetActive(true);
        _rb.isKinematic = true;
    }

    [PunRPC]
    public void RPC_Revive()
    {
        currentHealth = characterData.MaxHealth * reviveHealthPercent;
        CanMove(true);
        photonView.RPC("RPC_OnRevive", RpcTarget.All);
        TakeDamage(0);
    }

    [PunRPC]
    void RPC_OnRevive()
    {
        if (movement != null)
            movement.PlayDashAnimation(); // animation đứng dậy
        currentWeapon.gameObject.SetActive(true);
        if (photonView.IsMine)
        {
            _rb.isKinematic = false;
        }
        _reviveSystem.gameObject.SetActive(false);
        isDowned = false;
        if (RoomSessionManager.Instance.IsRoomOwner())
        {
            PlayerManager.Instance.CheckDie(photonView.ViewID, false);
        }
    }




    public void CanMove(bool value)
    {
        if (movement is PlayerController pc)
            pc.CanMove = value;

    }

    public void EquipWeapon(WeaponBase newWeapon)
    {
        DropWeapon();
        currentWeapon = newWeapon;
        currentWeapon.transform.SetParent(weaponHolder);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
        currentWeapon.isEquipped = true;
    }

    private void DropWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.transform.SetParent(null);
            Destroy(currentWeapon.gameObject);
            currentWeapon = null;
        }
    }

    public void SetActiveSkill(SkillCardBase skill)
    {
        activeSkill = skill;
        skill.gameObject.transform.SetParent(transform, false);
        skill.transform.localPosition = Vector3.zero;
        activeSkill.GetComponent<SpriteRenderer>().enabled = false;
        statUI.UpdateActive(skill);
    }
    public void SetPassiveSkill(SkillCardBase skill)
    {
        passiveSkills.Add(skill);
        skill.gameObject.transform.SetParent(transform, false);
        skill.transform.localPosition = Vector3.zero;
        SpriteRenderer spriteRenderer = skill.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        statUI.UpdatePassive(skill, passiveSkills.Count - 1);
    }
    public List<SkillCardBase> GetPassiveSkills()
    {
        return passiveSkills;
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
    }
    void TryInteract()
    {
        if (currentInteract != null)
            currentInteract.Interact(this);
    }



    public void Init(CharacterData data)
    {
        characterData = data;

        if (characterData == null)
        {
            Debug.LogError("CharacterHandler.Init(): characterData is null");
            return;
        }

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
        {
            GameObject dropped = Instantiate(data.StartingWeapon);
            var weaponBase = dropped.GetComponent<WeaponBase>();
            string newId = System.Guid.NewGuid().ToString();
            weaponBase.Initialize(newId);
            EquipWeapon(weaponBase);
        }

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
        currentMana = playerloadinfo.currentMana;
        TakeDamage(0);
        UseMana(0);
    }


    [PunRPC]
    public void RPC_FireProjectile(string projectileName, Vector3 position, Vector2 direction, float speed, float lifespan, float damage, int rotationMode)
    {
        direction.Normalize();
        GameObject prefab = Resources.Load<GameObject>($"Weapon/{projectileName}");
        if (prefab == null)
        {
            Debug.LogWarning($"Missing projectile prefab: {projectileName}");
            return;
        }

        GameObject proj = Instantiate(prefab, position, Quaternion.identity);
        proj.GetComponent<SpriteRenderer>().flipX = rotationMode == 1;
        if (rotationMode == 2)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            proj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        var hitbox = proj.GetComponent<Projectile>();
        if (hitbox == null)
        {
            return;
        }

        hitbox.lifetime = lifespan;
        hitbox.direction = direction;
        hitbox.speed = speed;
        hitbox.damage = damage;

    }
    [PunRPC]
    public void RPC_FireProjectile(string projectileName, Vector3 position, Vector2 direction, float speed, float lifespan, float damage)
    {
        RPC_FireProjectile(projectileName, position, direction, speed, lifespan, damage, 0);
    }
    public void AssignTeammateView(RectTransform rt)
    {
        statUI.AssignTeammateView(rt, characterData.PlayerSprite);
    }
    [PunRPC]
    public void RPC_UpdateStatTeammateHP(float current, float max)
    {
        statUI.UpdateTeammateHp(current, max);
    }
    [PunRPC]
    public void RPC_UpdateStatTeammateMana(float current, float max)
    {
        statUI.UpdateTeammateMana(current, max);
    }
}