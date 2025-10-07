using UnityEngine;
using UnityEditor;

namespace TechC.Editor
{
    /// <summary>
    /// リングの距離の目安を出力するツール
    /// </summary>
    public class SpeedRingPlacementTool : EditorWindow
    {
        [Header("距離指定")]
        private float inputDistance = 100f;

        // 線形計算の係数（8つのデータポイントから最小二乗法で算出）
        // データ: 20:-118, 50:-88, 80:-58, 120:-18, 150:33, 250:112, 450:312, 1000:862
        private const float LINEAR_SLOPE = 1.0127f;      // 傾き (高精度計算)
        private const float LINEAR_INTERCEPT = -138.24f;  // 切片 (高精度計算)

        [MenuItem("Tools/Speed Ring座標計算")]
        public static void ShowWindow()
        {
            SpeedRingPlacementTool window = GetWindow<SpeedRingPlacementTool>("Speed Ring座標計算");
            window.minSize = new Vector2(300, 200);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Speed Ring座標計算ツール", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawCalculationTool();
        }

        void DrawCalculationTool()
        {
            EditorGUILayout.LabelField("座標計算", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("実測データ基準 (8ポイント):");
            EditorGUILayout.LabelField("  20m→Z=-118  50m→Z=-88  80m→Z=-58  120m→Z=-18");
            EditorGUILayout.LabelField("  150m→Z=33  250m→Z=112  450m→Z=312  1000m→Z=862");
            EditorGUILayout.LabelField($"計算式: Z = {LINEAR_SLOPE:F4} × 距離 + ({LINEAR_INTERCEPT:F2})", EditorStyles.helpBox);

            EditorGUILayout.Space();

            inputDistance = EditorGUILayout.FloatField("距離 (m)", inputDistance);
            float calculatedZ = CalculateZFromDistance(inputDistance);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("計算結果:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Z座標 = {calculatedZ:F2}", EditorStyles.helpBox);

            // コピーしやすいように詳細な情報も表示
            EditorGUILayout.LabelField("Unity座標 (X, Y, Z):");
            EditorGUILayout.SelectableLabel($"(0, 5, {calculatedZ:F2})", EditorStyles.textField);
        }

        float CalculateZFromDistance(float distance)
        {
            // 線形計算: Z = LINEAR_SLOPE × distance + LINEAR_INTERCEPT
            // 実測データ（8ポイント: 20:-118, 50:-88, 80:-58, 120:-18, 150:33, 250:112, 450:312, 1000:862）から最小二乗法で算出
            return LINEAR_SLOPE * distance + LINEAR_INTERCEPT;
        }
    }
}