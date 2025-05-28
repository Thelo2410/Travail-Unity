using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class Enemy : MonoBehaviour
{
    [Header("UI")]
    public GameObject healthBarObject;
    public Slider healthSlider;

    private float healthBarTimer;
    private float healthBarDisplayDuration = 4f;
    [Header("Stats")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Effets")]
    public GameObject deathEffect;

    private SpriteRenderer spriteRenderer;
    private Coroutine flashRoutine;


    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthBarObject.SetActive(false);
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    void Update()
    {
        if (healthBarObject.activeSelf)
        {
            healthBarTimer -= Time.deltaTime;
            if (healthBarTimer <= 0f)
            {
                healthBarObject.SetActive(false);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} a pris {amount} dégâts ! Vie restante : {currentHealth}");

        healthSlider.value = currentHealth;

        healthBarObject.SetActive(true);
        healthBarTimer = healthBarDisplayDuration;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRed());
        StartCoroutine(Shake());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        spriteRenderer.color = originalColor;
    }

    IEnumerator Shake(float duration = 0.15f, float magnitude = 0.05f)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }


    void Die()
    {
        Debug.Log($"{gameObject.name} est mort !");

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        GetComponent<LootDropper>()?.DropLoot();
        Destroy(gameObject);
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
    }
}