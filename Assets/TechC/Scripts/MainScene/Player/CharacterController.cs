using UnityEngine;
using TechC.Manager;
using UnityEngine.InputSystem;

namespace TechC.Main.Player
{
    /// <summary>
    /// プレイヤーを管理するクラス
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterController : MonoBehaviour
    {
        [Header("レーン情報")]
        [SerializeField] private Transform[] lanes;
        [SerializeField] private int initLeftTofuLane = 1;
        [SerializeField] private int initRightTofuLane = 2;

        [Header("移動速度")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("豆腐のオブジェクト")]
        [SerializeField] private GameObject leftTofu;
        [SerializeField] private GameObject rightTofu;

        private Rigidbody rb;
        private int leftTofuCurrentLane = 1;
        private int rightTofuCurrentLane = 2;

        private bool isClickingLeft;
        private bool isClickingRight;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            leftTofuCurrentLane = initLeftTofuLane;
            rightTofuCurrentLane = initRightTofuLane;
        }

        void Update()
        {
            if (isClickingLeft)
            {
                MoveLeftLane();
            }
            if (isClickingRight)
            {
                MoveRightLane();
            }
        }

        void FixedUpdate()
        {
            float deltaTime = GameManager.I.DeltaTime;
            rb.MovePosition(transform.position + Vector3.forward * moveSpeed * deltaTime);
        }

        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isClickingLeft = true;
            }
            else if (context.canceled)
            {
                isClickingLeft = false;
                leftTofuCurrentLane = initLeftTofuLane;
                MoveToLane(leftTofu.transform, leftTofuCurrentLane);
            }
        }

        public void OnMoveRight(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                isClickingRight = true;
            }
            else if (context.canceled)
            {
                isClickingRight = false;
                rightTofuCurrentLane = initRightTofuLane;
                MoveToLane(rightTofu.transform, rightTofuCurrentLane);
            }
        }

        
        public void MoveLeftLane()
        {
            leftTofuCurrentLane = Mathf.Max(0, leftTofuCurrentLane - 1);
            MoveToLane(leftTofu.transform, leftTofuCurrentLane);
        }

        public void MoveRightLane()
        {
            rightTofuCurrentLane = Mathf.Min(lanes.Length - 1, rightTofuCurrentLane + 1);
            MoveToLane(rightTofu.transform, rightTofuCurrentLane);
        }

        void MoveToLane(Transform target, int laneIndex)
        {
            Vector3 targetPos = new Vector3(lanes[laneIndex].position.x, target.position.y, target.position.z);
            target.position = targetPos;
        }

    }
}