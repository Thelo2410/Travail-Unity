using UnityEngine;
using UnityEngine.UI;


public class WeaponPickup : MonoBehaviour
{
    public Weapon weaponData;
    private Text interactUI;
    private bool isInRange;



    public float pickupDelay = 1f;
    private float spawnTime;
    
   void Start()
    {
       spawnTime = Time.time;
    }

        void Awake()
    {
        interactUI = GameObject.FindGameObjectWithTag("InteractUI").GetComponent<Text>();
    }

    public bool CanBePickedUp()
    {
        return Time.time - spawnTime > pickupDelay;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactUI.enabled = true;
            isInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interactUI.enabled = false;
            isInRange = false;
        }
    }
}