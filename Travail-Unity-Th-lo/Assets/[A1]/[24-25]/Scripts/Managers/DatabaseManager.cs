using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A1_24_25
{
    public class DatabaseManager : MonoBehaviour
    {
        private static DatabaseManager _instance;
        public static DatabaseManager Instance => _instance;

        [SerializeField] private EnemyDatabase enemyDatabase;

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        public EnemyData GetData(int id, bool random = false) 
            => enemyDatabase.GetData(id, random);
    }
}
