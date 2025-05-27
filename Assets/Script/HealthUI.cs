using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image healthBarFill;
    [SerializeField] private float m_duration = 1f;    
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = new Color(1f, 0.64f, 0f); // orange
    [SerializeField] private Color lowHealthColor = Color.red;
    private float m_time = 0;
    private float targetValue = 1f;

    


    public void UpdateHealthDisplay(int healthValue, int maxHealthValue)
    {
        targetValue = healthValue / (float)maxHealthValue;
    }

    void Update()
    {
        if (healthBarFill != null && healthBarFill.enabled)
        {
            float currentFill = healthBarFill.fillAmount;
            float newFill = Mathf.Lerp(currentFill, targetValue, Time.deltaTime * 3f);
            healthBarFill.fillAmount = newFill;
            Color targetColor;

            if (targetValue > 0.5f)
            {
                float t = Mathf.InverseLerp(1f, 0.5f, targetValue);
                targetColor = Color.Lerp(fullHealthColor, midHealthColor, t);
            }
            else
            {
                float t = Mathf.InverseLerp(0.5f, 0f, targetValue);
                targetColor = Color.Lerp(midHealthColor, lowHealthColor, t);
            }

            healthBarFill.color = targetColor;

            m_time += Time.unscaledDeltaTime;
        }
    }
}
