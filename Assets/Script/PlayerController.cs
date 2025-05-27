using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public Rigidbody2D rb;
    public Collider2D playerCollider;

    [Header("Mouvement & Saut")]
    public float moveSpeed = 16f;
    [SerializeField] private float jumpForce = 15f; 
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
    [SerializeField] private Slider fuelSlider;

    [Header("Sol & Murs")]
    [SerializeField] private float boxLength = 1f; 
    [SerializeField] private float boxHeight = 0.2f; 
    [SerializeField] private float wallCheckDistance = 0.6f; 
    [SerializeField] private Transform groundPosition;
    [SerializeField] private Transform wallCheckPosition;
    [SerializeField] private LayerMask groundLayer; 
    
    [SerializeField] private LayerMask wallLayer; 

    private float moveInput;
    private bool jumpPressed;
    private bool jumpBuffered;
    private float jumpBufferTimer;
    private float jumpBufferTime = 0.15f; 
    private float coyoteTimer;
    private float coyoteTime = 0.15f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public GameObject jumpDustPrefab;
    public GameObject landDustPrefab;


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

    public Transform firePoint;
    public GameObject weaponPickupPrefab;

    private Weapon currentWeapon;
    private float attackCooldown = 0f;
    private bool canAttack = true;

    private WeaponPickup nearbyWeapon;

    public Transform weaponHolder;
    private GameObject equippedWeaponGO;


    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 0f;

        currentFuel = fuel;
        //initatilisation de l'animator
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");
        jumpPressed = Input.GetButtonDown("Jump");
        fuelSlider.value = currentFuel / fuel;
        bool isRunning = !Mathf.Approximately(moveInput, 0f);
        animator.SetBool("isRunning", isRunning);


        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (mouseWorldPos.x > transform.position.x && !isFacingRight)
            Flip();
        else if (mouseWorldPos.x < transform.position.x && isFacingRight)
            Flip();

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
        
        if (!isWallJumping)
            Flip();

        if (!isWallJumping) Flip();

        if (Input.GetKeyDown(KeyCode.T) && nearbyWeapon != null && nearbyWeapon.CanBePickedUp())
        {
           EquipWeapon(nearbyWeapon.weaponData);
            Destroy(nearbyWeapon.gameObject);
            nearbyWeapon = null;
        }

        if (Input.GetKeyDown(KeyCode.F) && currentWeapon != null)
        {
            DropWeapon(currentWeapon);
        }

        if (Input.GetButtonDown("Fire1") && currentWeapon != null && canAttack)
        {
            Attack();
        }


    }

    void FixedUpdate()
    {
        bool wasGrounded = grounded;
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

        if (!wasGrounded && grounded && rb.linearVelocity.y < -2f)
        {
            TriggerLandParticles();
        }


        coyoteTimer = grounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;

        touchingWall = Physics2D.Raycast(transform.position, Vector2.right * (isFacingRight ? 1 : -1), wallCheckDistance, wallLayer);

        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }

        HandleJump();

        bool isGliding = Input.GetButton("Jump") && rb.linearVelocity.y < 0f && currentFuel > 0f;

        if (isGliding)
        {
            rb.gravityScale = 1f;
            currentFuel -= fuelBurnRate * Time.fixedDeltaTime;
        }
        else
        {
            rb.gravityScale = 3f;
        }

        if (grounded) RefillFuel();
        currentFuel = Mathf.Clamp(currentFuel, 0f, fuel);
    }

    void HandleJump()
    {
        if (jumpBuffered && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            TriggerJumpParticles(); 
            canDoubleJump = true;
            isWallJumping = false;
            jumpBuffered = false;
        }
        else if (jumpBuffered && canDoubleJump && !grounded && !touchingWall)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            canDoubleJump = false;
            jumpBuffered = false;
        }
    }

    void TriggerJumpParticles()
    {
        if (jumpDustPrefab != null)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, groundPosition.position.y, 0);
            Instantiate(jumpDustPrefab, spawnPos, Quaternion.identity);
        }
    }
    void TriggerLandParticles()
    {
        if (landDustPrefab != null)
        {
            Vector3 spawnPos = new Vector3(transform.position.x, groundPosition.position.y, 0);
            Instantiate(landDustPrefab, spawnPos, Quaternion.identity);
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
            DropWeapon(currentWeapon);
        }

        currentWeapon = newWeapon;

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

        // 1. Instancier le prefab de pickup
        GameObject droppedWeapon = Instantiate(weaponPickupPrefab, transform.position + Vector3.right * 0.5f, Quaternion.identity);

        // 2. Remplir les données
        WeaponPickup pickup = droppedWeapon.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.weaponData = weaponToDrop;

            // 3. Ajouter un visuel si nécessaire
            if (weaponToDrop.weaponVisualPrefab != null)
            {
                GameObject visual = Instantiate(weaponToDrop.weaponVisualPrefab, droppedWeapon.transform);
                visual.transform.localPosition = Vector3.zero;
            }

            // 4. Appliquer une petite impulsion physique
            Rigidbody2D rb = droppedWeapon.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic; // Forcer le corps dynamique
                rb.gravityScale = 1f;
                rb.linearVelocity = Vector2.zero; // Reset au cas où
                rb.AddForce(new Vector2(Random.Range(-1f, 1f), 3f), ForceMode2D.Impulse);
            }
        }

        // 5. Supprimer l’arme visuelle dans la main
        if (equippedWeaponGO != null)
        {
            Destroy(equippedWeaponGO);
            equippedWeaponGO = null;
        }

        currentWeapon = null;
        canAttack = false;
    }

    

    void Attack()
    {
        if (currentWeapon == null) return;

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
        Vector2 attackDirection = isFacingRight ? Vector2.right : Vector2.left;
        float range = 3.5f;
        float width = 0.5f;
        Vector2 boxSize = new Vector2(range, width);
        Vector2 boxCenter = (Vector2)firePoint.position + attackDirection * (range / 2f);

        if (currentWeapon.attackEffectPrefab != null)
        {
            GameObject fx = Instantiate(currentWeapon.attackEffectPrefab, boxCenter, Quaternion.identity);
            fx.transform.localScale = new Vector3(range, width, 1f);
            Destroy(fx, 0.3f);
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, LayerMask.GetMask("Enemy"));
        foreach (Collider2D hit in hits)
        {
            hit.GetComponent<Enemy>()?.TakeDamage(5);
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), 0.5f);
    }



    void AxeAttack()
    {
        float radius = 3.5f;
        Vector3 center = transform.position;

        if (currentWeapon.attackEffectPrefab != null)
        {
            GameObject fx = Instantiate(currentWeapon.attackEffectPrefab, center, Quaternion.identity);
            fx.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
            Destroy(fx, 0.3f);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask("Enemy"));
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
            // Obtenir la position du curseur dans le monde
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            // Calculer la direction de tir
            Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

            // Instancier la flèche
            GameObject arrow = Instantiate(currentWeapon.attackEffectPrefab, firePoint.position, Quaternion.identity);

            ArrowProjectile proj = arrow.GetComponent<ArrowProjectile>();
            if (proj != null)
            {
                proj.Fire(direction);
            }
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), 0.75f);
    }



    void ResetAttack()
    {
        canAttack = true;
    }

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
