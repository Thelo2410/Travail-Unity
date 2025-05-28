using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A1_24_25
{
    public class BlocController : MonoBehaviour
    {
        public float speed;
        public bool IsMoved { get; set; } = true;

        void Start()
        {
        
        }

        void Update()
        {
            if (!IsMoved)
                return;

            transform.position += transform.up * speed * Time.deltaTime;
        }
    }
}
