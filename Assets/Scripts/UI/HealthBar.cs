using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    public Image image;
    public void SetHealthBar(int health,int maxHealth)
    {
        healthBar.value = (health * 1f)/maxHealth;
    }
    public void SetBarColor(Color color)
    {
        image.color = color;
    }
}
