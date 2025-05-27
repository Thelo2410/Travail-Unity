using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Statistiques")]
    public int damage = 4;
    public float speed = 10f;
    public float lifetime = 3f;
    public float maxDistance = 25f;

    [Header("Effets visuels et audio")]
    public GameObject impactEffectPrefab;
    public AudioClip shootSound;
    public AudioClip impactSound;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Vector2 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    public void Fire(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = direction * speed;

        Collider2D playerCol = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Collider2D>();
        Collider2D arrowCol = GetComponent<Collider2D>();
        if (playerCol != null && arrowCol != null)
        {
            Physics2D.IgnoreCollision(arrowCol, playerCol);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Destroy(gameObject, lifetime);
    }



    void Update()
    {
        float distance = Vector2.Distance(startPosition, transform.position);
        if (distance > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        if (impactSound != null)
            AudioSource.PlayClipAtPoint(impactSound, transform.position);

        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
