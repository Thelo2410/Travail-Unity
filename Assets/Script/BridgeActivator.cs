using UnityEngine;

public class BridgeActivator : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isActivated = false;
    private bool hasLanded = false;

    public LayerMask solLayer;
    public Transform checkPosition;
    public float checkRadius = 0.1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void ActiverPont()
    {
        if (isActivated) return;

        isActivated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        Debug.Log("Pont activé !");
    }

    void Update()
    {
        if (isActivated && !hasLanded)
        {
            if (Physics2D.OverlapCircle(checkPosition.position, checkRadius, solLayer))
            {
                Debug.Log("Pont stabilisé !");
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                hasLanded = true;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (checkPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(checkPosition.position, checkRadius);
        }
    }
}

