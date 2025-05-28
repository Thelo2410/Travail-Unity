using UnityEngine;
using TMPro; 

[RequireComponent(typeof(BoxCollider2D))]
public class PuzzleButton : MonoBehaviour
{
    [Tooltip("Autres leviers à activer/désactiver en même temps")]
    public PuzzleButton[] othersToToggle;

    [Header("UI")]
    [Tooltip("Texte à afficher quand le joueur est proche")]
    [SerializeField] private TextMeshProUGUI interactionText;

    private float initialAngle;       // angle de départ du levier
    private bool playerInRange;       // vrai si le joueur est dans la zone/triger
    private bool alreadyActivated;    // vrai si le levier est activé

    public bool isOn => alreadyActivated; 

    void Awake()
    {
        //  garde l’angle de départ pour le réutiliser après
        initialAngle = transform.localEulerAngles.z;

        //  rend le collider déclencheur (trigger)
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        // Cache le texte dès le début
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    void Update()
    {
        // si le player est dans la zone et appuie sur V, on active/désactive
        if (playerInRange && Input.GetKeyDown(KeyCode.V))
        {
            ToggleFromPlayer(); // on inverse l'état du levier
            PuzzleManager.instance?.CheckPuzzleState(); // vérifie le puzzle 
        }
    }

    private void ToggleFromPlayer()
    {
        if (alreadyActivated)
            Deactivate(); // remet l’angle de base
        else
            Activate();   // tourne le levier

        // Active ou désactive les autres leviers liés
        if (othersToToggle != null)
        {
            foreach (var lev in othersToToggle)
                lev?.ToggleExternally();
        }
    }

    private void Activate()
    {
        // Change l'angle 
        transform.localEulerAngles = new Vector3(0f, 0f, -initialAngle);
        alreadyActivated = true;
    }

    private void Deactivate()
    {
        // Remet l’angle d’origine pour redresser le levier
        transform.localEulerAngles = new Vector3(0f, 0f, initialAngle);
        alreadyActivated = false;
    }

    public void ToggleExternally()
    {
        // Utilisé quand un autre levier veut activer celui-ci
        if (alreadyActivated) Deactivate();
        else Activate();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // Affiche le message
        if (interactionText != null)
        {
            interactionText.text = "Appuyez sur V pour interagir";
            interactionText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // hide le message 
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }
}
