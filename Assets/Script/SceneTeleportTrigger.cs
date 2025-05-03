using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleportTrigger : MonoBehaviour
{
    [Tooltip("Nom de la scène vers laquelle téléporter")]
    public string targetSceneName = "main_menu";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
