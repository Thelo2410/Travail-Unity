using UnityEngine;

public class DestructibleWall : MonoBehaviour
{
    public GameObject breakFX;


    public void Break()
    {
        if (breakFX != null)
        {
            Instantiate(breakFX, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}