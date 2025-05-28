using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [System.Serializable]
    public class Loot
    {
        public GameObject prefabLootable;
        [Range(0f, 1f)] public float chanceDrop;
    }

    public Loot[] lootTable;

    public void DropLoot()
    {
        foreach (var loot in lootTable)
        {
            if (Random.value < loot.chanceDrop && loot.prefabLootable != null)
            {
                Instantiate(loot.prefabLootable, transform.position, Quaternion.identity);
                return;
            }
        }
    }
}
