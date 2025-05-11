using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A1_24_25
{
    public class PlayerController : MonoBehaviour
    {
        [field: SerializeField] public Stats Stat { get; private set; }

        //[Header("MOVE")]
        //[SerializeField] private float _speedMove = 5;

        [Header("JUMP")]
        //[SerializeField] private float _forceJump = 5;
        [SerializeField] private int _limiteJump = 2;
        private int _currentJump;

        [Header("WALL")]
        public LayerMask detectWall;
        [SerializeField] private float _distanceDW = 1;


        private Rigidbody2D _rgbd2D;
        private Collider2D _collider2D;
        private SpriteRenderer _spriteRend;

        void Start()
        {
            Stat = new(5);

            TryGetComponent(out _collider2D);
            TryGetComponent(out _rgbd2D);
            TryGetComponent(out _spriteRend);
        }

        void Update()
        {
            if (Input.GetButton("Horizontal"))
                Move();

            if (Input.GetKeyDown(KeyCode.Space) && _currentJump < _limiteJump)
                Jump();
        }

        void Move()
        {
            Vector3 direction = Input.GetAxisRaw("Horizontal") * Vector2.right;

            var hit = Physics2D.BoxCast(transform.position, Vector2.one, 0, direction, _distanceDW, detectWall);

            if (hit.collider != null)
                return;

            Debug.DrawRay(transform.position, direction * _distanceDW);

            if (!Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetAxisRaw("Horizontal") < 0)
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                else
                    transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            //if (!Input.GetKey(KeyCode.LeftControl))
            //    _spriteRend.flipX = Input.GetAxisRaw("Horizontal") < 0;

            transform.position += direction * Stat.speedMove * Time.deltaTime;
        }

        void Jump() 
        {
            _rgbd2D.linearVelocity = Vector2.up * Stat.forceJump;
            _currentJump++;
        }

        public void SetDamage(int degat)
        {
            Stat.life = Mathf.Abs(degat);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
                _currentJump = 0;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
                _currentJump = 0;

            if (collision.gameObject.layer == LayerMask.NameToLayer("Item"))
                SetDamage(-1);

            if (collision.gameObject.layer == LayerMask.NameToLayer("Item") &&
                collision.CompareTag("Finish"))
            {
                GameManager.Instance.ToggleStopBloc();
            }
        }
    }
}
