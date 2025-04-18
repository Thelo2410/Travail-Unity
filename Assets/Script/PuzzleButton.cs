using UnityEngine;

/// Gère un levier du puzzle. Lorsqu'on interagit avec, il peut changer son état
///  Maj la couleur et la rotation du levier.

public class PuzzleButton : MonoBehaviour
{
    [Tooltip("État initial du levier. true = allumé (droite, vert), false = éteint (gauche, rouge)")]
    public bool isOn = false;

    [Tooltip("Autres leviers à toggle lorsqu'on active celui-ci")]
    public PuzzleButton[] othersToToggle;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Si la porte est déjà ouverte, on ne peut plus toucher aux leviers
        if (PuzzleManager.instance.doorAlreadyOpened) return;

        ToggleSelf();

        // Active/désactive les autres leviers liés
        foreach (var levier in othersToToggle)
        {
            levier.ToggleSelf();
        }

        // Vérifie si le puzzle est résolu
        PuzzleManager.instance.CheckPuzzleState();
    }

    
    /// toggle = change l'etat du levier
   
    public void ToggleSelf()
    {
        isOn = !isOn;
        UpdateVisual();
    }

/// force le changement d'etat

    public void ForceToggle()
    {
        isOn = !isOn;
        UpdateVisual();
    }

    
    /// Définit l’état du levier

    public void SetState(bool state)
    {
        isOn = state;
        UpdateVisual();
    }

    /// Maj la rotation et la couleur du levier en fonction de son état

    void UpdateVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        transform.localRotation = Quaternion.Euler(0f, 0f, isOn ? -45f : 45f);

        // 🟢 = bon / 🔴 = pas bon
        sr.color = isOn ? Color.green : Color.red;
    }
}
