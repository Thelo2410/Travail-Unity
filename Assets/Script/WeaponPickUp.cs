using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Weapon weaponData;

    public float pickupDelay = 1f;
    private float spawnTime;
    
   void Start()
    {
       spawnTime = Time.time;
    }

    public bool CanBePickedUp()
    {
        return Time.time - spawnTime > pickupDelay;
    }
}