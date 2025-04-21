
using UnityEngine;

namespace ChaosFantasy.Models
{
    public class PlayerStats : MonoBehaviour
    {
        public float maxHP = 100f;
        public float currentHP;

        public float maxMana = 50f;
        public float currentMana;

        public float moveSpeed = 5f;
        public float attackSpeed = 1f;

        void Start()
        {
            currentHP = maxHP;
            currentMana = maxMana;
        }

        public void TakeDamage(float amount)
        {
            currentHP -= amount;
            if (currentHP <= 0)
            {
                currentHP = 0;
                Die();
            }
        }

        public void UseMana(float amount)
        {
            currentMana -= amount;
            if (currentMana < 0)
                currentMana = 0;
        }

        public void RegenerateMana(float amount)
        {
            currentMana = Mathf.Min(currentMana + amount, maxMana);
        }

        void Die()
        {
            Debug.Log("Player Died!");
        }
    }
}
