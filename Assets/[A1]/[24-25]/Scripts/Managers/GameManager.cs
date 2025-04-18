using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A1_24_25
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        private List<BlocController> blocs = new();

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            blocs.AddRange(FindObjectsOfType<BlocController>());
        }

        public void ToggleStopBloc()
        {
            foreach (var bc in blocs)
            {
                bc.IsMoved = !bc.IsMoved;
            }
        }
    }
}
