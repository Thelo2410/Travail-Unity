using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public Rigidbody2D rb;
    public Collider2D playerCollider;

    [Header("Mouvement & Saut")]
    [SerializeField] public float moveSpeed = 16f; 
    [SerializeField] private float jumpForce = 15f; 
    [SerializeField] private float wallJumpForce = 16f; //verticale 
    [SerializeField] private float wallJumpHorizontalForce = 8f; // Force horizontale du wall jump

    [Header("Visual")]
    [SerializeField] private float wallSlideSpeed = 2f; // Vitesse de slide  le long d'un mur
    [SerializeField] private Color hitColor = Color.red; // hit color
    [SerializeField] private float hitColorDuration = 0.8f; 
    [SerializeField] private float hitAnimDuration = 0.5f; 

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float normalizeRotationSpeed = 3f;
    [SerializeField] private float maxTiltAngle = 20f;

    [Header("Fuel / Gliding")] // on l'utilise plus
    [SerializeField] private float fuel = 100f; 
    [SerializeField] private float fuelBurnRate = 30f; 
    [SerializeField] private float fuelRefillRate = 20f; 
    [SerializeField] private Slider fuelSlider; 

    [Header("Sol & Murs")]
    [SerializeField] private float boxLength = 1f;
    [SerializeField] private float boxHeight = 0.2f;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private Transform groundPosition; // détecter le sol
    [SerializeField] private Transform wallCheckPosition; // détecter les murs
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Slash Water VFX")]
    public GameObject slashWaterVfxPrefab; // vfx déclenché quand on attaque les ennemis
    public float slashWaterVfxDuration = 0.6f; // durée  de l'effet visuel

    // paramètres Animator
    private static readonly int IsHit = Animator.StringToHash("IsHit"); // Transforme IsHit en int pour l'Animator pour évite les fautes. 'readonly' = valeur fixe.
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int IsDeadHash = Animator.StringToHash("isDead");

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;

    // mvmts & sauts
    private float moveInput;
    private bool jumpPressed;
    private bool jumpBuffered;
    private float jumpBufferTimer;
    private float jumpBufferTime = 0.15f;
    private float coyoteTimer;
    private float coyoteTime = 0.15f;
    private bool canDoubleJump;
    private bool grounded;
    private bool touchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;

    // Gliding(le planage)
    private float currentFuel;
    private bool isFacingRight = true;

    // Armes
    public Transform firePoint;
    public GameObject weaponPickupPrefab;
    private Weapon currentWeapon;
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
        rb.freezeRotation = true; // empêche la rotation  du joueur
        rb.gravityScale = 0f;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        currentFuel = fuel; // carburant
    }

    void Update()
    {
        // inputs
        moveInput = Input.GetAxisRaw("Horizontal");
        jumpPressed = Input.GetButtonDown("Jump");
        fuelSlider.value = currentFuel / fuel;

        // Animation en fonction de que fait le player
        grounded = Physics2D.OverlapBoxAll(
                       groundPosition.position,
                       new Vector2(boxLength, boxHeight),
                       0f,
                       groundLayer
                   ).Length > 1;
        // Gestion des animations selon ce qui se passe au niveau du saut
        animator.SetBool(IsJumping, !grounded && rb.linearVelocity.y > 0.1f);
        animator.SetBool(IsFalling, !grounded && rb.linearVelocity.y < -0.1f);
        animator.SetBool("isRunning", grounded && Mathf.Abs(moveInput) > 0.1f);

        // Gestion du buffer de saut
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

        WallSlide(); // Détecte et applique un wallslide 
        WallJump(); // le saut depuis les murs

        if (!isWallJumping) Flip(); // retourne le sprite quannd on fait le walljump

        // Ramasser et déposer les armes(+thelo)
        if (Input.GetKeyDown(KeyCode.T) && nearbyWeapon != null && nearbyWeapon.CanBePickedUp())
        {
            EquipWeapon(nearbyWeapon.weaponData);
            Destroy(nearbyWeapon.gameObject);
            nearbyWeapon = null;
        }
        if (Input.GetKeyDown(KeyCode.F) && currentWeapon != null)
            DropWeapon(currentWeapon);
        if (Input.GetButtonDown("Fire1") && currentWeapon != null && canAttack)
            Attack(); // Attaque avec l'arme équipée
    }

    void FixedUpdate()
    {
        // mvmt horizontal (désactivé  pendant  wall jump)
        if (!isWallJumping)
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Saut classique ou double saut 
        if (jumpBuffered && (grounded || (canDoubleJump && !grounded && !touchingWall)))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            canDoubleJump = grounded ? true : false;
            isWallJumping = false;
            jumpBuffered = false;
        }

        // planer : si le joueur maintient saut en tombant
        bool isGliding = Input.GetButton("Jump") && rb.linearVelocity.y < 0f && currentFuel > 0f;
        rb.gravityScale = isGliding ? 1f : 3f;
        if (isGliding) currentFuel -= fuelBurnRate * Time.fixedDeltaTime;
        if (grounded && currentFuel < fuel) currentFuel += fuelRefillRate * Time.fixedDeltaTime;
        currentFuel = Mathf.Clamp(currentFuel, 0f, fuel);
    }

    private void WallSlide()
    {
        touchingWall = Physics2D.Raycast(transform.position, isFacingRight ? Vector2.right : Vector2.left, wallCheckDistance, wallLayer);
        isWallSliding = touchingWall && !grounded && Mathf.Abs(moveInput) > 0f;
        if (isWallSliding)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
    }

    private void WallJump()
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
            canDoubleJump = true;
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                var s = transform.localScale;
                s.x *= -1f;
                transform.localScale = s;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping() => isWallJumping = false;

    private void EquipWeapon(Weapon newWeapon)
    {
        if (currentWeapon != null) DropWeapon(currentWeapon);
        currentWeapon = newWeapon;

        if (equippedWeaponGO != null) Destroy(equippedWeaponGO);
        if (newWeapon.weaponVisualPrefab != null)
            equippedWeaponGO = Instantiate(newWeapon.weaponVisualPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);

        canAttack = true;
    }

    private void DropWeapon(Weapon weaponToDrop)
    {
        var dropped = Instantiate(weaponPickupPrefab, transform.position + Vector3.right, Quaternion.identity);
        var pickup = dropped.GetComponent<WeaponPickup>();
        if (pickup != null)
        {
            pickup.weaponData = weaponToDrop;
            if (weaponToDrop.weaponVisualPrefab != null)
            {
                var vis = Instantiate(weaponToDrop.weaponVisualPrefab, dropped.transform);
                vis.transform.localPosition = Vector3.zero;
            }
        }

        if (equippedWeaponGO != null) Destroy(equippedWeaponGO);
        currentWeapon = null;
        canAttack = false;
    }

    public void OnHit()
    {
        spriteRenderer.color = hitColor; // change la couleur du sprite quand se fait toucher par un ennemi
        animator.SetBool(IsHit, true); // lance l'animation hit
        Invoke(nameof(ResetHitColor), hitColorDuration); // Reset couleur (quand on ne se fait plus touché)
        Invoke(nameof(ResetHitBool), hitAnimDuration); // Reset animation
    }

    private void ResetHitColor()
    {
        spriteRenderer.color = originalColor;
    }

    private void ResetHitBool()
    {
        animator.SetBool(IsHit, false);
    }

    public void OnDeath()
    {
        isDead = true;
        animator.SetBool(IsDeadHash, true);
        enabled = false;
    }

    private void Flip()
    {
        if ((isFacingRight && moveInput < 0f) || (!isFacingRight && moveInput > 0f))
        {
            isFacingRight = !isFacingRight;
            var s = transform.localScale;
            s.x *= -1f;
            transform.localScale = s;
        }
    }

    private void Attack()
    {
        switch (currentWeapon.weaponType)
        {
            case WeaponType.Sword: SwordAttack(); break;
            case WeaponType.Axe: AxeAttack(); break;
            case WeaponType.Bow: BowAttack(); break;
        }
    }

    private void SwordAttack()
    {
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
        float range = 3.5f, width = 0.5f;
        Vector2 center = (Vector2)firePoint.position + dir * (range / 2f);
        Vector2 size = new Vector2(range, width);

        // zone devant le joueur avec VFX quand on attaque les ennemis
        var hits = Physics2D.OverlapBoxAll(center, size, 0f, LayerMask.GetMask("Enemy"));
        foreach (var h in hits)
        {
            h.GetComponent<Enemy>()?.TakeDamage(5);
            if (slashWaterVfxPrefab != null)
            {
                var spawn = h.ClosestPoint(firePoint.position);
                var vfx = Instantiate(slashWaterVfxPrefab, spawn, Quaternion.identity);
                vfx.transform.localScale = Vector3.one;
                var sr = vfx.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) { sr.sortingLayerName = "Foreground"; sr.sortingOrder = 20; }
                Destroy(vfx, slashWaterVfxDuration);
            }
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), 0.5f);
    }

    private void AxeAttack()
    {
        float radius = 3.5f;
        Vector3 pos = transform.position;

        // zone  autour du joueur avec VFX
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, radius, LayerMask.GetMask("Enemy"));

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;

            enemy.TakeDamage(8);

            if (slashWaterVfxPrefab != null)
            {
                Vector3 spawnPos = hit.ClosestPoint(firePoint.position);
                var vfx = Instantiate(slashWaterVfxPrefab, spawnPos, Quaternion.identity);
                vfx.transform.localScale = Vector3.one;

                var sr = vfx.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = "Foreground";
                    sr.sortingOrder = 20;
                }

                Destroy(vfx, slashWaterVfxDuration);
            }
        }

        canAttack = false;
        Invoke(nameof(ResetAttack), 1f);
    }

    private void BowAttack()
    {
        if (currentWeapon.attackEffectPrefab != null)
        {
            var arrow = Instantiate(currentWeapon.attackEffectPrefab, firePoint.position, firePoint.rotation);
            var arb = arrow.GetComponent<Rigidbody2D>();
            if (arb != null) arb.linearVelocity = (isFacingRight ? Vector2.right : Vector2.left) * 10f;
        }
        canAttack = false;
        Invoke(nameof(ResetAttack), 0.75f);
    }

    private void ResetAttack() => canAttack = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("WeaponPickup")) nearbyWeapon = other.GetComponent<WeaponPickup>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("WeaponPickup")) nearbyWeapon = null;
    }
}
