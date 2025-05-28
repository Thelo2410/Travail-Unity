using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("VFX du checkpoint")]
    [Tooltip("Prefab du petit effet (particle system ou sprite animé)")]
    [SerializeField] private GameObject checkpointVfxPrefab;
    [Tooltip("Durée avant destruction du VFX")]
    [SerializeField] private float effectDuration = 1f;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // Maj le point de respawn
        CurrentSceneManager.instance.respawnPoint = transform.position;

        //Lance le VFX
        if (checkpointVfxPrefab != null)
        {
            var vfx = Instantiate(
                checkpointVfxPrefab,
                transform.position,
                Quaternion.identity
            );
            //le detruit après une courte durée
            Destroy(vfx, effectDuration);
        }

        
        boxCollider.enabled = false;
    }
}
