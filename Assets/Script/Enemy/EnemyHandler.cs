using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHandler : MonoBehaviour, IDamageable

{
    public EnemyData enemyData;

    private EnemyMovement movement;
    private DropRateManager drop;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public float currentDamage, currentHealth;

    private GameObject player;

    void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        drop = GetComponent<DropRateManager>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        
    }

    void Start()
    {
        
    }

    void Update()
    {
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) {
            EnemySpawner.Instance.ReturnToPool(gameObject);
        }
    }
    public void Init(EnemyData data)
    {
        this.enemyData = data;
        player = GameObject.FindGameObjectWithTag("Player");
        currentDamage = enemyData.Damage;
        currentHealth = enemyData.MaxHealth;
        if (GetComponent<EnemyMovement>() != null)
        {
            Destroy(GetComponent<EnemyMovement>());
        }
        StartCoroutine(addMovementScript());
    }
    private IEnumerator addMovementScript()
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

}




