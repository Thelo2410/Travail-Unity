using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
    public void OpenDoor()
    {
        Debug.Log(" Porte désactivée !");
        gameObject.SetActive(false); // désactive  la porte
    }
}
