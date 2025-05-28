using UnityEngine;
using UnityEngine.EventSystems;

//  gère l'effet lorsque la souris survole le bouton "Play"
public class PlayButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator animator; 

    
    void Start()
    {
        // récup le composant Animator 
        animator = GetComponent<Animator>();
    }

    // la souris est sur le bouton
    public void OnPointerEnter(PointerEventData eventData)
    {
        //  active le bool "IsHovered" dans l'Animator
        // déclenche l'animation de survol
        animator.SetBool("IsHovered", true);
    }

    // quand la souris quitte le bouton
    public void OnPointerExit(PointerEventData eventData)
    {
        // désactive le bool "IsHovered"
        // arrête l'animation de survol
        animator.SetBool("IsHovered", false);
    }
}