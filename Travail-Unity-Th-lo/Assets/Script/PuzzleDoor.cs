using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
    [Header("VFX d'ouverture")]
    [Tooltip("Prefab du VFX à jouer quand la porte s'ouvre")]
    [SerializeField] private GameObject openVfxPrefab;
    [Tooltip("Durée en secondes avant destruction du VFX")]
    [SerializeField] private float vfxDuration = 1f;

    public void OpenDoor()
    {
        //  VFX 
        if (openVfxPrefab != null)
        {
            GameObject vfx = Instantiate(
                openVfxPrefab,
                transform.position,
                Quaternion.identity
            );
            Destroy(vfx, vfxDuration);
        }

        // désactivation de la porte
        Debug.Log("Porte désactivée !");
        gameObject.SetActive(false);
    }
}
