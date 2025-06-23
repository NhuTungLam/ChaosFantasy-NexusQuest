using UnityEngine;

[CreateAssetMenu(fileName = "EnemyScriptableObject", menuName = "ScriptableObjects/Enemy")]
public class EnemyData : ScriptableObject
{
    public string e_name;
    public string deathAnimName;

    [SerializeField]
    private float maxHealth;
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    [SerializeField]
    private float damage;
    public float Damage { get => damage; set => damage = value; }

    [SerializeField]
    private float atkSpeed;
    public float AtkSpeed { get => atkSpeed; set => atkSpeed = value; }

    public string movementScriptName;

    public RuntimeAnimatorController animatorController;
    [SerializeField]
    private Vector2 colliderOffset;
    public Vector2 ColliderOffset => colliderOffset;

    [SerializeField]
    private Vector2 colliderSize;
    public Vector2 ColliderSize => colliderSize;

    [SerializeField]
    private bool isStationary;
    public bool IsStationary => isStationary;
    [SerializeField]
    private float detectionRange;
    public float DetectionRange => detectionRange;

}
