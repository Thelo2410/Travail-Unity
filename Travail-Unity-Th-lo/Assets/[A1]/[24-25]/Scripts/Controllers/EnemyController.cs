using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static A1_24_25.EnemyData;

namespace A1_24_25
{
    public class EnemyController : MonoBehaviour
    {
        public enum STATE
        {
            NONE,
            INIT,
            IDLE,
            MOVE,
            FOLLOW,
            FIRE,
            DEATH
        }
        [SerializeField] private STATE _state = STATE.NONE;

        public int id;
        public bool randomAllow;

        private EnemyData data;

        private Rigidbody2D _rgbd2D;
        private BoxCollider2D _collider2D;
        private SpriteRenderer _spriteRend;

        private bool moveLeft;
        private float _countdown;
        private Vector3 _direction;

        [Header("POSITION")]
        public float limiteOffsetMin;
        public float limiteOffsetMax;
        [field: SerializeField] public MOVE_DIR DirectionMode { get; private set; }
        public Vector2 Direction => DirectionMode == MOVE_DIR.HORIZONTAL ? Vector2.right : Vector2.up;
        private Vector2 _relativeLimitePatrolMin;
        private Vector2 _relativeLimitePatrolMax;

        private void Awake()
        {
            TryGetComponent(out _collider2D);
            TryGetComponent(out _rgbd2D);
            TryGetComponent(out _spriteRend);
        }

        void Start()
        {
            data = DatabaseManager.Instance.GetData(id, randomAllow);
            Init();
        }

        private void Init()
        {
            _state = STATE.INIT;

            name = data.label;
            transform.localScale = Vector3.one * data.scaleCoef;

            _collider2D.size = data.sprite.bounds.size;
            _spriteRend.sprite = data.sprite;
            _spriteRend.color = data.color;

            var limiteMin = limiteOffsetMin * Direction;
            var limiteMax = limiteOffsetMax * Direction;

            _relativeLimitePatrolMin = transform.TransformPoint(limiteMin);
            _relativeLimitePatrolMax = transform.TransformPoint(limiteMax);

            _state = STATE.IDLE;
        }

        void Update()
        {
            if (_state < STATE.INIT)
                return;

            switch (_state)
            {
                case STATE.IDLE:
                    if (_countdown > data.durationIDLE)
                    {
                        _state = STATE.MOVE;
                        _countdown = 0;

                        ChangeDirection();
                    }

                    _countdown += Time.deltaTime;
                    break;

                case STATE.MOVE:
                    //if (_countdown > data.durationWALK)
                    //{
                    //    _state = STATE.IDLE;
                    //    _countdown = 0;
                    //}

                    bool change = false;
                    switch (DirectionMode)
                    {
                        case MOVE_DIR.HORIZONTAL:
                            change = transform.position.x < _relativeLimitePatrolMin.x || transform.position.x > _relativeLimitePatrolMax.x;
                            break;

                        case MOVE_DIR.VERTICAL:
                            change = transform.position.y < _relativeLimitePatrolMin.y || transform.position.y > _relativeLimitePatrolMax.y;
                            break;
                    }

                    if (change)
                        ChangeDirection();
                    
                    _rgbd2D.transform.position += _direction * Time.deltaTime * data.speed;
                    _countdown += Time.deltaTime;
                    break;

                case STATE.FOLLOW:

                    break;
                case STATE.FIRE:

                    break;
                case STATE.DEATH:

                    break;
            }
        }

        private void ChangeDirection()
        {
            moveLeft = !moveLeft;

            _spriteRend.flipX = moveLeft;
            switch (DirectionMode)
            {
                case MOVE_DIR.HORIZONTAL:

                    if (moveLeft)
                        _direction = Vector3.right;
                    else
                        _direction = Vector3.left;
                    break;

                case MOVE_DIR.VERTICAL:

                    if (moveLeft)
                        _direction = Vector3.up;
                    else
                        _direction = Vector3.down;
                    break;
            }

        }

        private void OnDrawGizmosSelected()
        {
            Vector2 limiteMin;
            Vector2 limiteMax;

            if (Application.isPlaying)
            {
                limiteMin = _relativeLimitePatrolMin;
                limiteMax = _relativeLimitePatrolMax;
            }
            else
            {
                limiteMin = transform.TransformPoint(limiteOffsetMin * Direction);
                limiteMax = transform.TransformPoint(limiteOffsetMax * Direction);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(limiteMin, .5f);
            Gizmos.DrawLine(transform.position, limiteMin);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, limiteMax);
            Gizmos.DrawWireSphere(limiteMax, .5f);  
        }
    }


}
