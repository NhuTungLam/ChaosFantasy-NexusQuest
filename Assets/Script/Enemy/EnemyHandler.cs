using Photon.Pun;
using System.Collections;
using UnityEngine;

public class EnemyHandler : MonoBehaviourPun, IPunInstantiateMagicCallback, IDamageable
{
    public EnemyData enemyData;

    protected EnemyMovement movement;
    protected DropRateManager drop;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;

    public float currentDamage, currentHealth;
    //private GameObject player;

    void Awake()
    {
        drop = GetComponent<DropRateManager>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }
    public void TakeDamage(float damage)
    {
        if (!RoomSessionManager.Instance.IsRoomOwner()) { return; }
        photonView.RPC("RPC_UpdateVisual", RpcTarget.All, damage);
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        // Gửi yêu cầu về master để xử lý sát thương
        //photonView.RPC("RPC_TakeDamage", RpcTarget.MasterClient, damage);
    }
    private IEnumerator FlashCoroutine()
    {
        //flashMaterial.SetColor(FlashColor, flashColor);
        spriteRenderer.material.SetFloat("_FlashAmount", 1f);

        yield return new WaitForSeconds(0.15f);

        spriteRenderer.material.SetFloat("_FlashAmount", 0f);
    }
    [PunRPC]
    public void RPC_TakeDamage(float damage)
    {
        if(!RoomSessionManager.Instance.IsRoomOwner()) { return; }
        photonView.RPC("RPC_UpdateVisual",RpcTarget.All, damage);
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }

    }
    [PunRPC]
    public void RPC_UpdateVisual(float damage)
    {
        if (damage > 0f)
        {
            DamagePopUp.Create(transform.position, Mathf.RoundToInt(damage));
            if (this != null)
                StartCoroutine(FlashCoroutine());
        }
    }
    /// <summary>
    /// Rotation mode: 0 - null, 1 - flipx, 2 - based on direction
    /// </summary>
    /// <param name="projectileName"></param>
    /// <param name="position"></param>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    /// <param name="lifespan"></param>
    /// <param name="damage"></param>
    /// <param name="rotationMode"></param>
    [PunRPC]
    public void RPC_FireProjectile(string projectileName, Vector3 position, Vector2 direction, float speed, float lifespan, float damage, int rotationMode)
    {
        direction.Normalize();
        GameObject prefab = Resources.Load<GameObject>($"Enemies/{projectileName}");
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

        var hitbox = proj.GetComponent<EnemyHitBox>();
        if (hitbox == null)
        {
            Debug.LogWarning("Projectile prefab missing EnemyHitBox");
            return;
        }

        hitbox.lifespan = lifespan;
        hitbox.UpdateFunc = () => proj.transform.position += (Vector3)(direction * speed * Time.deltaTime);
        hitbox.HitEffect = (handler) =>
        {
            //Debug.Log("Projectile hit player");
            handler.TakeDamage(damage);
        };
        hitbox.OnDestroy = () => Destroy(proj); // Không cần ReturnToPool khi RPC

        // Nếu muốn pooling đồng bộ, cần thiết kế lại → giữ Destroy để đơn giản
    }
    [PunRPC]
    public void RPC_FireProjectile(string projectileName, Vector3 position, Vector2 direction, float speed, float lifespan, float damage)
    {
        RPC_FireProjectile(projectileName, position, direction, speed, lifespan, damage, 0);
    }
    
    public void Init(EnemyData data)
    {
        if (data == null)
        {
            Debug.LogError("[EnemyHandler] EnemyData is null!");
            return;
        }

        this.enemyData = data;
        currentDamage = enemyData.Damage;
        currentHealth = enemyData.MaxHealth;

        if (animator != null && enemyData.animatorController != null)
        {
            animator.runtimeAnimatorController = enemyData.animatorController;
        }

        SetupCollider();
        rb.isKinematic = enemyData.IsStationary;

        if (GetComponent<EnemyMovement>() != null)
        {
            Destroy(GetComponent<EnemyMovement>());
        }

        StartCoroutine(AddMovementScript());
    }

    private void SetupCollider()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.offset = enemyData.ColliderOffset;
            col.size = enemyData.ColliderSize;
        }
        else
        {
            Debug.LogWarning("[EnemyHandler] No BoxCollider2D found.");
        }
    }

    private IEnumerator AddMovementScript()
    {
        yield return null;
        System.Type type = System.Type.GetType(enemyData.movementScriptName);
        if (type != null && type.IsSubclassOf(typeof(EnemyMovement)))
        {
            movement = gameObject.AddComponent(type) as EnemyMovement;
            movement.detectionRange = enemyData.DetectionRange;
        }
        else
        {
            Debug.LogError($"[EnemyHandler] Cannot find type '{enemyData.movementScriptName}' or it's not EnemyMovement.");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        rb.velocity = Vector2.zero;
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<CharacterHandler>();
            if (player != null)
            {
                player.TakeDamage(currentDamage);
                Debug.Log($"[Enemy] Hit player for {currentDamage} damage");
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = photonView.InstantiationData;
        if (data != null && data.Length > 0)
        {
            string enemyName = (string)data[0];
            EnemyData edata = Resources.Load<EnemyData>($"Enemies/{enemyName}");
            Init(edata);
        }
        else
        {
            Debug.LogError("[EnemyHandler] Missing InstantiationData!");
        }
    }
}