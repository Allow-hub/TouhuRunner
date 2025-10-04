using System.Collections;
using System.Collections.Generic;
using TechC.Manager;
using TMPro;
using UnityEngine;

namespace TechC.Main
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI distanceText;
        private float lastScore;
        private float lastDistance;
        private string scoreStr = "スコア:";
        private string distanceStr = "進んだ距離:";

        private void Start()
        {
            scoreText.text = scoreStr + GameManager.I.Score.ToString("F0");
            distanceText.text = distanceStr + GameManager.I.MoveDistance.ToString("F0") + "m";
            lastDistance = GameManager.I.MoveDistance;
            lastScore = GameManager.I.Score;
        }

        private void Update()
        {
            if (lastScore != GameManager.I.Score)
            {
                scoreText.text = scoreStr + GameManager.I.Score.ToString("F0");
                lastScore = GameManager.I.Score;
            }

            if (lastDistance != GameManager.I.MoveDistance)
            {
                distanceText.text = distanceStr + GameManager.I.MoveDistance.ToString("F0") + "m";
                lastDistance = GameManager.I.Score;
            }
        }

        [ContextMenu("Add")]
        public void Test()
        {
            GameManager.I.AddDisrance(10);
        }
    }
}
