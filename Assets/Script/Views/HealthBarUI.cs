
using UnityEngine;
using UnityEngine.UI;
using ChaosFantasy.Models;

namespace ChaosFantasy.Views
{
    public class HealthBarUI : MonoBehaviour
    {
        public Slider healthSlider;
        public PlayerStats stats;

        void Update()
        {
            if (stats != null && healthSlider != null)
            {
                healthSlider.value = stats.currentHP / stats.maxHP;
            }
        }
    }
}
