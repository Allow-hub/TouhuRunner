using UnityEngine;
using TechC.Manager;
using System;

namespace TechC.Main.Player
{
    /// <summary>
    /// プレイヤーの移動を管理するクラス
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

        [Header("壁判定設定")]
        [SerializeField] private LayerMask wallLayer = -1;
        // キャッシュ用フィールド
        private Rigidbody rb;
        private PlayerInputManager inputManager;
        private Transform leftTofuTransform;
        private Transform rightTofuTransform;

        // 現在のレーン位置
        private int leftTofuCurrentLane;
        private int rightTofuCurrentLane;

        // 移動状態フラグ
        private bool isMovingLeft;
        private bool isMovingRight;

        // プロパティで最大レーンインデックスを取得
        private int MaxLaneIndex => lanes.Length - 1;

        // 定数定義
        private const int LANE_STEP = 1;
        private const int MIN_LANE_INDEX = 0;
        private Vector3 lastPos;

        private void Start()
        {
            InitializeComponents();
            InitializeLanes();
            SubscribeToInputEvents();
            lastPos = transform.position;
        }

        private void OnDestroy()
        {
            UnsubscribeFromInputEvents();
        }

        private void Update()
        {
            HandleMovement();
            var dis = Vector3.Distance(transform.position, lastPos);
            GameManager.I.AddDisrance(dis);
            GameManager.I.AddScore(dis);
            lastPos = transform.position;
        }

        private void FixedUpdate()
        {
            MoveForward();
        }


        #region 初期化
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody>();
            inputManager = GetComponent<PlayerInputManager>();

            leftTofuTransform = leftTofu.transform;
            rightTofuTransform = rightTofu.transform;
        }

        private void InitializeLanes()
        {
            leftTofuCurrentLane = initLeftTofuLane;
            rightTofuCurrentLane = initRightTofuLane;
        }
        #endregion

        #region イベント購読管理
        private void SubscribeToInputEvents()
        {
            if (inputManager == null) return;

            inputManager.OnLeftInputStarted += StartLeftMovement;
            inputManager.OnLeftInputCanceled += StopLeftMovement;
            inputManager.OnRightInputStarted += StartRightMovement;
            inputManager.OnRightInputCanceled += StopRightMovement;
        }

        private void UnsubscribeFromInputEvents()
        {
            if (inputManager == null) return;

            inputManager.OnLeftInputStarted -= StartLeftMovement;
            inputManager.OnLeftInputCanceled -= StopLeftMovement;
            inputManager.OnRightInputStarted -= StartRightMovement;
            inputManager.OnRightInputCanceled -= StopRightMovement;
        }
        #endregion

        #region 移動処理
        private void HandleMovement()
        {
            if (isMovingLeft)
            {
                MoveLeftLane();
            }
            if (isMovingRight)
            {
                MoveRightLane();
            }
        }

        private void MoveForward()
        {
            float deltaTime = GameManager.I.DeltaTime;
            Vector3 movement = Vector3.forward * (moveSpeed * deltaTime);
            rb.MovePosition(transform.position + movement);
        }

        private void MoveLeftLane()
        {
            leftTofuCurrentLane = Mathf.Max(MIN_LANE_INDEX, leftTofuCurrentLane - LANE_STEP);
            MoveToLane(leftTofuTransform, leftTofuCurrentLane);
        }

        private void MoveRightLane()
        {
            rightTofuCurrentLane = Mathf.Min(MaxLaneIndex, rightTofuCurrentLane + LANE_STEP);
            MoveToLane(rightTofuTransform, rightTofuCurrentLane);
        }

        private void MoveToLane(Transform target, int laneIndex)
        {
            Vector3 currentPosition = target.position;
            Vector3 targetPosition = new Vector3(
                lanes[laneIndex].position.x,
                currentPosition.y,
                currentPosition.z
            );
            target.position = targetPosition;
        }
        #endregion

        #region 入力処理コールバック
        private void StartLeftMovement()
        {
            isMovingLeft = true;
        }

        private void StopLeftMovement()
        {
            isMovingLeft = false;
            leftTofuCurrentLane = initLeftTofuLane;
            MoveToLane(leftTofuTransform, leftTofuCurrentLane);
        }

        private void StartRightMovement()
        {
            isMovingRight = true;
        }

        private void StopRightMovement()
        {
            isMovingRight = false;
            rightTofuCurrentLane = initRightTofuLane;
            MoveToLane(rightTofuTransform, rightTofuCurrentLane);
        }
        #endregion

        #region 壁衝突処理
        /// <summary>
        /// 壁との衝突処理（ゲームオーバー）
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if ((wallLayer.value & (1 << collision.gameObject.layer)) > 0)
            {
                HandleWallCollision(collision.gameObject);
            }
        }

        private void HandleWallCollision(GameObject wall)
        {
            // プレイヤーの移動を停止
            rb.velocity = Vector3.zero;

            isMovingLeft = false;
            isMovingRight = false;

            GameManager.I.ChangeResultState();
            ResultManager.I.ShowResult();
        }
        #endregion
    }
}