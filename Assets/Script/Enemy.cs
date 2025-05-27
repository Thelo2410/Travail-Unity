using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    private int currentHealth;
    private bool isDead = false;

    [Header("Animations")]
    public Animator animator;
    private static readonly int IsHitHash  = Animator.StringToHash("isHit");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [Header("Effets")]
    public GameObject deathEffect;

    [Header("Hit Flash")]
    [SerializeField] private Color hitColor          = Color.red;
    [SerializeField] private float hitFlashDuration  = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth   = maxHealth;
        spriteRenderer  = GetComponent<SpriteRenderer>();
        originalColor   = spriteRenderer.color;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"{name} prend {amount} dégâts ! Vie restante : {currentHealth}");

        // anim hit
        animator?.SetTrigger(IsHitHash);

        // hit rouge
        if (spriteRenderer != null)
            StartCoroutine(FlashHitColor());

        if (currentHealth <= 0)
            StartCoroutine(Die());
    }

    public void Attack()
    {
        if (isDead) return;
        animator?.SetTrigger(AttackHash);
    }

    private IEnumerator FlashHitColor()
    {
        // passe en rouge si toucher
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);
        // restaure la couleur
        spriteRenderer.color = originalColor;
    }

    private IEnumerator Die()
    {
        isDead = true;

        // récup d’abord le collider dans une variable
        Collider2D col2D = GetComponent<Collider2D>();
        if (col2D != null)
            col2D.enabled = false;

        yield return new WaitForSeconds(0.1f);

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        GetComponent<LootDropper>()?.DropLoot();
        Destroy(gameObject);
    }
}