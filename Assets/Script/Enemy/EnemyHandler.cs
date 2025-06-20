using Photon.Pun;
using System.Collections;
using UnityEngine;

public class EnemyHandler : MonoBehaviourPun, IPunInstantiateMagicCallback, IDamageable
{
    public EnemyData enemyData;

    private EnemyMovement movement;
    private DropRateManager drop;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    public float currentDamage, currentHealth;
    private GameObject player;

    void Awake()
    {
        drop = GetComponent<DropRateManager>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }
    public void TakeDamage(float damage)
    {
        // Gửi yêu cầu về master để xử lý sát thương
        photonView.RPC("RPC_TakeDamage", RpcTarget.MasterClient, damage);
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }

        DamagePopUp.Create(transform.position, Mathf.RoundToInt(damage));
    }

    [PunRPC]
    public void RPC_FireProjectile(string projectileName, Vector3 position, Vector2 direction, float speed, float lifespan, float damage)
    {
        GameObject prefab = Resources.Load<GameObject>($"Enemies/{projectileName}");
        if (prefab == null)
        {
            Debug.LogWarning($"Missing projectile prefab: {projectileName}");
            return;
        }

        GameObject proj = Instantiate(prefab, position, Quaternion.identity);
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
            Debug.Log("Projectile hit player");
            handler.TakeDamage(damage);
        };
        hitbox.OnDestroy = () => Destroy(proj); // Không cần ReturnToPool khi RPC

        // Nếu muốn pooling đồng bộ, cần thiết kế lại → giữ Destroy để đơn giản
    }

    public void Init(EnemyData data)
    {
        if (data == null)
        {
            Debug.LogError("[EnemyHandler] EnemyData is null!");
            return;
        }

        this.enemyData = data;
        player = GameObject.FindGameObjectWithTag("Player");
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