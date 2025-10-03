using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TechC.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        private float maxMoveDistance;//今までの最高移動距離
        private int maxScore;//今までの最高スコア
        private int currentMoney;//スコア/xをお金として加算、所持金

        private float moveDistance;//進んだ距離
        public float MoveDistance => moveDistance;

        private int score;//壁ぎりぎりでの避けで加算
        public int Score => score;
        [SerializeField] private int targetFrameRate = 144;
        [HideInInspector] public float DeltaTime { get; private set; }
        public bool IsPaused = false;
        public enum GameState
        {
            Title,
            Menu,
            Game,
            Result
        }
        public GameState currentState = GameState.Title;
        protected override void Init()
        {
            base.Init();

            // VSyncCount を Dont Sync に変更
            QualitySettings.vSyncCount = 0;
            // fps 144 を目標に設定
            Application.targetFrameRate = targetFrameRate;
        }


        private void Update()
        {
            DeltaTime = IsPaused ? 0f : Time.deltaTime;
            StateHandler();
        }

        private void SetState(GameState state)
        {
            currentState = state;
            switch (state)
            {
                case GameState.Title:
                    break;
                case GameState.Game:
                    moveDistance = 0;
                    score = 0;
                    break;
            }
        }
        private void StateHandler()
        {
            //switch (currentState)
            //{

            //}
        }

        private void ChangeCursorMode(bool visible, CursorLockMode cursorLockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = cursorLockMode;
        }


        // 非同期でシーンをロード
        public void LoadSceneAsync(int sceneIndex)
        {
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }

        // 非同期でシーンをロードするコルーチン
        private IEnumerator LoadSceneCoroutine(int sceneIndex)
        {
            // フェードアウト
            yield return StartCoroutine(FadeManager.I.FadeOut());

            // シーンロード
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
            while (!asyncOperation.isDone)
                yield return null;

            // フェードイン
            yield return StartCoroutine(FadeManager.I.FadeIn());
        }

        //小規模なのでGameManagerが管理
        public void AddScore(int value) => score += value;
        public void AddDisrance(float value) => moveDistance += value;

        public void ChangeTitleState() => SetState(GameState.Title);
        public void ChangeGameState() => SetState(GameState.Game);
    }

}
