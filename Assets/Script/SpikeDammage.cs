using UnityEngine;
using System.Collections;

public class Spikes : MonoBehaviour
{
    public int damageAmount = 100; // dégâts suffisants pour tuer, ou ajustable

    private Animator fadeSystem;

    private void Awake()
    {
        fadeSystem = GameObject.FindGameObjectWithTag("FadeSystem").GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);

                // Si la vie tombe à 0, on lance la séquence de respawn
                if (playerHealth.CurrentHealth <= 0)
                {
                    StartCoroutine(RespawnAfterDeath(collision));
                }
            }
        }
    }

    private IEnumerator RespawnAfterDeath(Collider2D collision)
    {
        // Lancer le fondu
        fadeSystem.SetTrigger("FadeIn");
        yield return new WaitForSeconds(1f);

        // Réinitialiser la position et la vie du joueur
        collision.transform.position = CurrentSceneManager.instance.respawnPoint;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Respawn(); // gère la réactivation + restauration de la vie
        }
    }
}
