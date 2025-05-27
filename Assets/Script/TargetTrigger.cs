using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    public GameObject pontASactiver;
    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Arrow"))
        {
            Debug.Log("Cible touchée !");
            triggered = true;

            if (pontASactiver != null)
            {
                BridgeActivator ba = pontASactiver.GetComponent<BridgeActivator>();
                if (ba != null)
                {
                    ba.ActiverPont();
                }
                else
                {
                    Debug.LogWarning("BridgeActivator manquant sur l'objet pont.");
                }
            }

            Destroy(gameObject);
        }
    }
}
