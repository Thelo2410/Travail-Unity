using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverUI;

    public static GameOverManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de GameOverManager dans la scène");
            return;
        }

        instance = this;
    }

    /// <summary>
    /// Appelée quand le joueur meurt : lance le menu Game Over après 2 secondes.
    /// </summary>
    public void OnPlayerDeath()
    {
        StartCoroutine(ShowGameOverAfterDelay());
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        // Attendre 2 secondes
        yield return new WaitForSeconds(2f);
        // Afficher le menu Game Over
        gameOverUI.SetActive(true);
    }

    public void RetryButton()
    {
        // Recharge la scène actuelle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // Réinitialise le joueur si nécessaire
        PlayerHealth.instance.Respawn();
        gameOverUI.SetActive(false);
    }

    public void MainMenuButton()
    {
        SceneManager.LoadScene("main_menu");
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
