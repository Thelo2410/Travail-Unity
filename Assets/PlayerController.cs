using UnityEngine;
using UnityEngine.UI;


/// Gère les déplacements du joueur, les sauts, les wall jumps, et le système de planage (gliding).
/// + systeme de carburant type slider
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public Rigidbody2D rb;
    public Collider2D playerCollider;

    [Header("Stats")]
    [SerializeField] private PlayerStats _stat; // Stats de base du joueur 

    [Header("Mouvement & Saut")]
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private float jumpForce = 10f; 
    [SerializeField] private float wallJumpForce = 16f; 
    [SerializeField] private float wallJumpHorizontalForce = 8f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 100f; 
    [SerializeField] private float normalizeRotationSpeed = 3f; 
    [SerializeField] private float maxTiltAngle = 20f; 

    [Header("Fuel / Gliding")]
    [SerializeField] private float fuel = 100f;
    [SerializeField] private float fuelBurnRate = 30f; 
    [SerializeField] private float fuelRefillRate = 20f; 
    [SerializeField] private Slider fuelSlider; //slider

    [Header("Sol & Murs")]
    [SerializeField] private float boxLength = 1f; 
    [SerializeField] private float boxHeight = 0.2f; 
    [SerializeField] private float wallCheckDistance = 0.6f; 
    [SerializeField] private Transform groundPosition; // Position detection sol
    [SerializeField] private Transform wallCheckPosition; // Position  détection mur
    [SerializeField] private LayerMask groundLayer; 
    
    [SerializeField] private LayerMask wallLayer; 

    private float moveInput;
    private bool jumpPressed;
    private bool jumpBuffered;
    private float jumpBufferTimer;
    private float jumpBufferTime = 0.15f; 
    private float coyoteTimer;
    private float coyoteTime = 0.15f; // Délai après avoir quitté le sol pour permettre un saut

    private bool canDoubleJump;
    private bool grounded;
    private bool touchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;

    private float currentFuel;
    private bool isFacingRight = true;

    public Transform firePoint;  // Point où l'attaque se déclenche (au bout de la main par exemple)
    public GameObject weaponPickupPrefab;

    private Weapon currentWeapon;  // Arme actuelle que le joueur utilise
    private float attackCooldown = 0f;  // Pour contrôler le délai entre les attaques
    private bool canAttack = true;  // Pour contrôler quand attaquer

    private WeaponPickup nearbyWeapon;  // Arme à ramasser

    public Transform weaponHolder;  // assigné dans l’inspecteur
    private GameObject equippedWeaponGO;  // l’objet visuel actuellement équipé


    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f; // desactive la gravité

        currentFuel = fuel;

        //  stats du struct
        if (_stat.speed > 0) moveSpeed = _stat.speed;
        if (_stat.life > 0) jumpForce = _stat.life;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetButtonDown("Jump");
        fuelSlider.value = currentFuel / fuel;

        if (jumpPressed)
        {
            jumpBuffered = true;
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer <= 0f) jumpBuffered = false;
        }

        WallSlide();
        WallJump();

        if (!isWallJumping) Flip();

        // Ramasser l'arme avec T
        if (Input.GetKeyDown(KeyCode.T) && nearbyWeapon != null && nearbyWeapon.CanBePickedUp())
        {
           EquipWeapon(nearbyWeapon.weaponData);
            Destroy(nearbyWeapon.gameObject);
            nearbyWeapon = null;
        }

        // Reposer l'arme avec F
        if (Input.GetKeyDown(KeyCode.F) && currentWeapon != null)
        {
            DropWeapon(currentWeapon);
        }

        // Attaquer si une arme est équipée et si le cooldown est terminé
        if (Input.GetButtonDown("Fire1") && currentWeapon != null && canAttack)
        {
            Attack();
        }


    }

    void FixedUpdate()
    {
        // Détection sol
        Collider2D[] hits = Physics2D.OverlapBoxAll(groundPosition.position, new Vector2(boxLength, boxHeight), 0f);
        grounded = false;
        foreach (Collider2D hit in hits)
        {
            if (hit != playerCollider && hit.CompareTag("Ground"))
            {
                grounded = true;
                break;
            }
        }

        coyoteTimer = grounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;

        // Détection mur
        touchingWall = Physics2D.Raycast(transform.position, Vector2.right * (isFacingRight ? 1 : -1), wallCheckDistance, wallLayer);

        // Mouvement horizontal
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        HandleJump();

        // Glide (descente ralentie/planer)
        bool isGliding = Input.GetButton("Jump") && rb.linearVelocity.y < 0f && currentFuel > 0f;

        if (isGliding)
        {
            rb.gravityScale = 1f; // planage
            currentFuel -= fuelBurnRate * Time.fixedDeltaTime;
        }
        else
        {
            rb.gravityScale = 3f; // chute normale
        }

        if (grounded) RefillFuel();

        // Rotation visuelle(donne l'impression qu'il plane)
        float targetAngle = Mathf.Clamp(-moveInput * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
        float smoothAngle = Mathf.LerpAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothAngle);

        if (grounded && Mathf.Approximately(moveInput, 0f))
        {
            float uprightAngle = Mathf.LerpAngle(rb.rotation, 0f, normalizeRotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(uprightAngle);
        }

        currentFuel = Mathf.Clamp(currentFuel, 0f, fuel);
    }

    void HandleJump()
    {
        // Saut normal (avec coyote time)
        if (jumpBuffered && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            canDoubleJump = true;
            isWallJumping = false;
            jumpBuffered = false;
        }
        // Double saut (uniquement si en l'air et pas collé au mur)
        else if (jumpBuffered && canDoubleJump && !grounded && !touchingWall)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            canDoubleJump = false;
            jumpBuffered = false;
        }
    }

    void WallSlide()
    {
        isWallSliding = touchingWall && !grounded && moveInput != 0;
        if (isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
        }
    }

    void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else wallJumpingCounter -= Time.deltaTime;


        if (jumpPressed && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpHorizontalForce, wallJumpForce);
            wallJumpingCounter = 0f;
            canDoubleJump = true;

            // inverse la direction visuelle du joueur
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    void StopWallJumping() => isWallJumping = false;

    void Flip()
    {
        if ((isFacingRight && moveInput < 0f) || (!isFacingRight && moveInput > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    void RefillFuel()
    {
        if (currentFuel < fuel)
        {
            currentFuel += fuelRefillRate * Time.fixedDeltaTime;
        }
    }

    /// affiche la zone de détection du sol dans l'éditeur(car j'avais eu des bug jpp )
    void OnDrawGizmosSelected()
    {
        if (groundPosition == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundPosition.position, new Vector2(boxLength, boxHeight));
    }







    public void EquipWeapon(Weapon newWeapon)
    {
        if (currentWeapon != null)
        {
            DropWeapon(newWeapon);
        }

        currentWeapon = newWeapon;

        // Afficher l’arme dans la main
        if (equippedWeaponGO != null)
        {
            Destroy(equippedWeaponGO);
        }

        if (currentWeapon.weaponVisualPrefab != null)
        {
            equippedWeaponGO = Instantiate(currentWeapon.weaponVisualPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);
        }

        canAttack = true;
    }


    public void DropWeapon(Weapon weaponToDrop)
    {
        if (weaponToDrop == null) return;

        // Créer l'arme au sol
        GameObject droppedWeapon = Instantiate(weaponPickupPrefab, transform.position + Vector3.right * 1f, Quaternion.identity);
        WeaponPickup pickup = droppedWeapon.GetComponent<WeaponPickup>();

        if (pickup != null)
        {
            pickup.weaponData = weaponToDrop;
            if (weaponToDrop.weaponVisualPrefab != null)
            {  
                GameObject visual = Instantiate(weaponToDrop.weaponVisualPrefab, droppedWeapon.transform);
                visual.transform.localPosition = Vector3.zero;
            }
        }

        // Supprimer l’arme visuelle de la main
        if (equippedWeaponGO != null)
        {
            Destroy(equippedWeaponGO);
            equippedWeaponGO = null;
        }

        // Réinitialiser l’arme actuelle
        currentWeapon = null;
        canAttack = false;

        Debug.Log("L'arme lâchée est : " + weaponToDrop.name);
    }
    

    void Attack()
    {
        if (currentWeapon == null) return;

        // Appeler l'attaque en fonction du type d'arme
        switch (currentWeapon.weaponType)
        {
            case WeaponType.Sword:
                SwordAttack();
                break;
            case WeaponType.Axe:
                AxeAttack();
               break;
           case WeaponType.Bow:
                BowAttack();
                break;
        }
    }

    void SwordAttack()
    {
        // Affichage visuel de l'effet (optionnel)
        if (currentWeapon.attackEffectPrefab != null)
        {
            Instantiate(currentWeapon.attackEffectPrefab, firePoint.position, firePoint.rotation);
        }

        // Détection des ennemis dans une zone devant le joueur
        Vector2 attackOrigin = firePoint.position;
        Vector2 attackDirection = isFacingRight ? Vector2.right : Vector2.left;
        float range = 2f;
        float width = 0.5f;
        Vector2 boxSize = new Vector2(range, width);
        Vector2 boxCenter = (Vector2)firePoint.position + attackDirection * (range / 2f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, LayerMask.GetMask("Enemy"));
        foreach (Collider2D hit in hits)
        {
            // Tu peux adapter cette méthode selon ton système de dégâts
            hit.GetComponent<Enemy>()?.TakeDamage(5);
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), 0.5f); // délai entre attaques
    }

    void AxeAttack()
    {
        if (currentWeapon.attackEffectPrefab != null)
        {
            Instantiate(currentWeapon.attackEffectPrefab, firePoint.position, Quaternion.identity);
        }

        float radius = 1.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Enemy"));

        foreach (Collider2D enemy in hits)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(8);
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), 1f);
    }

    void BowAttack()
    {
        if (currentWeapon.attackEffectPrefab != null)
        {
            GameObject arrow = Instantiate(currentWeapon.attackEffectPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float speed = 10f;
                Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
                rb.linearVelocity = direction * speed;
            }
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), 0.75f);
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    // Détection d'une arme à ramasser
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("WeaponPickup"))
        {
            nearbyWeapon = other.GetComponent<WeaponPickup>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("WeaponPickup"))
        {
            nearbyWeapon = null;
        }
    }
}
