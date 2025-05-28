using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    private Coroutine currentLerpRoutine;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        if (currentLerpRoutine != null)
        {
            StopCoroutine(currentLerpRoutine);
        }
        currentLerpRoutine = StartCoroutine(SmoothHealthChange(health));
    }

    private IEnumerator SmoothHealthChange(int targetHealth)
    {
        float duration = 0.5f; // durée du lissage
        float elapsed = 0f;
        float startValue = slider.value;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            slider.value = Mathf.Lerp(startValue, targetHealth, t);
            fill.color = gradient.Evaluate(slider.normalizedValue);
            yield return null;
        }

        // Assure la valeur finale exacte
        slider.value = targetHealth;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}

