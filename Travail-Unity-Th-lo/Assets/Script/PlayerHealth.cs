using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Santé")]
    public int maxHealth = 100;
    private int currentHealth;

    public int CurrentHealth
    {
        get => currentHealth;
        private set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            if (healthBar != null)
                healthBar.SetHealth(currentHealth);
        }
    }

    [Header("Son")]
    public AudioClip hitSound;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer graphics;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitColorDuration = 0.8f;
    private Color originalColor;

    [Header("Invincibilité")]
    [SerializeField] private float invincibilityTimeAfterHit = 2f;
    [SerializeField] private float invincibilityFlashDelay    = 0.2f;
    private bool isInvincible = false;

    [Header("UI")]
    [SerializeField] private HealthBar healthBar;

    public static PlayerHealth instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Plus d'une instance de PlayerHealth détectée !");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        CurrentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        if (graphics != null)
            originalColor = graphics.color;
    }

    private void Update()
    {
        // test damage
        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(60);
    }

    public void HealPlayer(int amount)
    {
        CurrentHealth += amount;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        // son + dégâts
        if (hitSound != null)
            AudioManager.instance.PlayClipAt(hitSound, transform.position);

        CurrentHealth -= damage;

        // anim + flash couleur dans PlayerController
        PlayerController.instance?.OnHit();

        if (CurrentHealth <= 0) 
            Die();
        else
        {
            isInvincible = true;
            StartCoroutine(HandleInvincibilitySequence());
        }
    }

    private IEnumerator HandleInvincibilitySequence()
    {
        // Flash rouge rapide
        if (graphics != null)
        {
            graphics.color = hitColor;
            yield return new WaitForSeconds(hitColorDuration);
            graphics.color = originalColor;
        }

        // Clignotement invisible / visible
        float timer = 0f;
        while (timer < invincibilityTimeAfterHit)
        {
            if (graphics != null) graphics.enabled = false;
            yield return new WaitForSeconds(invincibilityFlashDelay);

            if (graphics != null) graphics.enabled = true;
            yield return new WaitForSeconds(invincibilityFlashDelay);

            timer += invincibilityFlashDelay * 2f;
        }

        // remet la visibilité et la couleur d'origine
        if (graphics != null)
        {
            graphics.enabled = true;
            graphics.color   = originalColor;
        }

        isInvincible = false;
    }

    public void Die()
    {
        // Anim de mort + désactivation du collider et du Rigidbody
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnDeath();
            var rb = PlayerController.instance.rb;
            rb.bodyType = RigidbodyType2D.Kinematic;
            PlayerController.instance.playerCollider.enabled = false;
        }

        // Écran Game Over
        GameOverManager.instance?.OnPlayerDeath();
        Debug.Log("Le joueur est mort.");
    }

    public void Respawn()
    {
        // Réactivation player
        if (PlayerController.instance != null)
        {
            var pc = PlayerController.instance;
            pc.enabled       = true;
            pc.rb.bodyType   = RigidbodyType2D.Dynamic;
            pc.playerCollider.enabled = true;
        }

        CurrentHealth = maxHealth;
    }
}
