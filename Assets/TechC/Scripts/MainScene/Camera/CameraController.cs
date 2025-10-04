using UnityEngine;

namespace TechC.Main.Camera
{
    /// <summary>
    /// プレイヤーに追従するカメラ制御クラス
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Z軸追従設定")]
        [SerializeField] private bool followZAxis = true;
        private Vector3 initialLocalPosition;
        private Transform parentTransform;

        void Start()
        {
            initialLocalPosition = transform.localPosition;
            parentTransform = transform.parent;

            if (parentTransform == null)
            {
                Debug.LogWarning("CameraController: カメラがプレイヤーの子オブジェクトではありません");
            }
        }

        void LateUpdate()
        {
            if (parentTransform == null) return;

            AdjustCameraPosition();
        }

        private void AdjustCameraPosition()
        {
            if (followZAxis)
            {
                Vector3 targetLocalPosition = new Vector3(
                    initialLocalPosition.x,
                    initialLocalPosition.y,
                    initialLocalPosition.z
                );

                transform.localPosition = targetLocalPosition;
            }
        }
    }
}
