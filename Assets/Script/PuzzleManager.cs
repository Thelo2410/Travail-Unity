using UnityEngine;
/// Gère l'état du puzzle (leviers + porte). Vérifie si la condition d'ouverture de porte est rempli
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance;

    public PuzzleButton[] leviers;
    public PuzzleDoor door;

    void Awake()
    {
        instance = this;
    }
public bool doorAlreadyOpened = false;
/// Si tous les leviers sont activés = ouvre la porte
public void CheckPuzzleState()
{
    foreach (var levier in leviers)
    {
        if (!levier.isOn) return;
    }

    Debug.Log(" Tous les leviers sont tournés à droite ");
    door.OpenDoor();
    doorAlreadyOpened = true; //  empêche toute future interaction(reste vert quoi)
}
}