using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A1_24_25
{
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Database/Enemy", order = 1)]
    public class EnemyDatabase : ScriptableObject
    {
        [SerializeField] private List<EnemyData> datas = new();

        public EnemyData GetData(int id, bool random = false)
        {
            if(random && (id < 0 || id >= datas.Count)) 
                id = Random.Range(0, datas.Count);
            else
                id = Mathf.Clamp(id, 0, datas.Count - 1);

            return datas[id];
        }
    }
}
