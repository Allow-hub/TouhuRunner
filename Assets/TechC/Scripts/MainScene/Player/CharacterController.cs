using UnityEngine;
using TechC.Manager;
using System.Collections.Generic;

namespace TechC.Main.Player
{
    /// <summary>
    /// 速度段階の設定情報
    /// </summary>
    [System.Serializable]
    public class SpeedLevel
    {
        [SerializeField] private float distance;
        [SerializeField] private float speed;
        
        public float Distance => distance;
        public float Speed => speed;
        
        public SpeedLevel(float distance, float speed)
        {
            this.distance = distance;
            this.speed = speed;
        }
    }

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
        [SerializeField] private float maxMoveSpeed = 20f;
        
        [Header("段階的速度上昇設定")]
        [SerializeField] private List<SpeedLevel> speedStages = new List<SpeedLevel>
        {
            new SpeedLevel(0f, 0f),
        };

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

        [Header("ドッジ設定")]
        [SerializeField] private float dodgeDistance = 2.0f;         // ドッジ判定距離
        [SerializeField] private int dodgeBaseScore = 10;            // ベーススコア
        [SerializeField] private float dodgeCheckInterval = 0.1f;    // 判定間隔
        [SerializeField] private float raycastDistance = 3.0f;       // Raycast距離

        private float dodgeTimer; // 判定タイマー
        private HashSet<Collider> processedWalls = new HashSet<Collider>(); // 処理済み壁を記録

        // 現在のレーン位置
        private int leftTofuCurrentLane;
        private int rightTofuCurrentLane;

        // 移動状態フラグ
        private bool isMovingLeft;
        private bool isMovingRight;

        // 速度管理
        private float currentMoveSpeed;
        private float lastSpeedIncreaseDistance; // 最後に速度を上げた距離
        private int currentSpeedStage = 0; // 現在の速度段階

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
            InitializeSpeed();
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
            UpdateSpeed();
            CheckDodge();
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

        private void InitializeSpeed()
        {
            currentMoveSpeed = moveSpeed;
            lastSpeedIncreaseDistance = 0f;
            currentSpeedStage = 0;
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
        private void UpdateSpeed()
        {
            // GameManagerから現在の移動距離を取得
            float currentDistance = GameManager.I.MoveDistance;
            
            // 現在の段階をチェック
            if (currentSpeedStage < speedStages.Count && 
                currentDistance >= speedStages[currentSpeedStage].Distance)
            {
                // 現在の段階に対応する速度設定を適用
                SetSpeedToTarget(currentDistance, currentSpeedStage);
            }
        }
        
        private void SetSpeedToTarget(float currentDistance, int stageIndex)
        {
            // 段階が範囲内かチェック
            if (stageIndex < speedStages.Count)
            {
                // 指定された目標速度に設定、または最大速度との最小値
                float targetSpeed = speedStages[stageIndex].Speed;
                currentMoveSpeed = Mathf.Min(targetSpeed, maxMoveSpeed);
                
                lastSpeedIncreaseDistance = speedStages[stageIndex].Distance;
                
                Debug.Log($"段階{stageIndex + 1} 速度変更! 距離: {currentDistance:F1}m, 速度: {currentMoveSpeed:F1} (目標: {targetSpeed:F1})");
            }
            else
            {
                // 配列範囲外の場合は最大速度に設定
                currentMoveSpeed = maxMoveSpeed;
                Debug.Log($"段階{stageIndex + 1} 最大速度到達! 距離: {currentDistance:F1}m, 速度: {currentMoveSpeed:F1}");
            }
            
            currentSpeedStage++;
        }

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
            Vector3 movement = Vector3.forward * (currentMoveSpeed * deltaTime);
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

        #region ドッジ判定
        /// <summary>
        /// 壁との距離を測定してドッジスコアを計算
        /// </summary>
        private void CheckDodge()
        {
            dodgeTimer += GameManager.I.DeltaTime;
            
            if (dodgeTimer >= dodgeCheckInterval)
            {
                DetectDodge();
                dodgeTimer = 0f;
            }
        }

        private void DetectDodge()
        {
            CheckDodgeFromTofu(leftTofuTransform, leftTofu);
            CheckDodgeFromTofu(rightTofuTransform, rightTofu);
        }

        private void CheckDodgeFromTofu(Transform tofuTransform, GameObject tofuObject)
        {
            Vector3 rayOrigin = tofuTransform.position;
            Vector3 rayDirection = Vector3.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, raycastDistance, wallLayer))
            {
                float distance = hit.distance;
                
                // ドッジ距離内で、まだ処理していない壁の場合
                if (distance <= dodgeDistance && !processedWalls.Contains(hit.collider))
                {
                    int bonusScore = CalculateDodgeScore(distance);
                    GameManager.I.AddScore(bonusScore);
                    
                    // この壁を処理済みとしてマーク
                    processedWalls.Add(hit.collider);
                    
                    // Debug.Log($"ドッジ成功! ({tofuObject.name}) 距離: {distance:F2}, ボーナス: {bonusScore}");
                    
                    // 一定時間後に処理済みリストから削除（同じ壁で再度ボーナスを得られるように）
                    StartCoroutine(RemoveProcessedWallAfterDelay(hit.collider, 1.0f));
                }
                
                // デバッグ用：Rayを可視化
                Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.red, dodgeCheckInterval);
            }
            else
            {
                // 壁が見つからない場合のデバッグライン
                Debug.DrawRay(rayOrigin, rayDirection * raycastDistance, Color.green, dodgeCheckInterval);
            }
        }

        private System.Collections.IEnumerator RemoveProcessedWallAfterDelay(Collider wall, float delay)
        {
            yield return new WaitForSeconds(delay);
            processedWalls.Remove(wall);
        }

        private int CalculateDodgeScore(float distance)
        {
            // 距離が近いほど高得点（距離が0に近いほどスコアが高い）
            float scoreMultiplier = (dodgeDistance - distance) / dodgeDistance;
            int finalScore = Mathf.RoundToInt(dodgeBaseScore * scoreMultiplier * currentMoveSpeed);
            return Mathf.Max(1, finalScore); // 最低1ポイント
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