using System;
using UnityEngine;

namespace ChaosFantasy.Models
{
    public class PlayerStats : MonoBehaviour
    {
        public float maxHP = 100f;
        public float currentHP;

        public float maxArmor = 30f;
        public float currentArmor;

        public float maxMana = 50f;
        public float currentMana;

        void Start()
        {
            currentHP = maxHP;
            currentArmor = maxArmor;
            currentMana = maxMana;
        }

        void Update()
        {
            // Test giảm máu, giáp, mana khi nhấn phím T
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestDamage();
            }
        }

        public void TestDamage(float hpLoss = 5f, float armorLoss = 3f, float manaLoss = 10f)
        {
            currentArmor -= armorLoss;
            if (currentArmor < 0) currentArmor = 0;

            currentHP -= hpLoss;
            if (currentHP < 0) currentHP = 0;

            currentMana -= manaLoss;
            if (currentMana < 0) currentMana = 0;

            Debug.Log($"[TEST DAMAGE] HP: {currentHP}, Armor: {currentArmor}, Mana: {currentMana}");
        }

        internal void UseMana(float manaCost)
        {
            throw new NotImplementedException();
        }
    }
}