using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TechC.Manager
{
    public class GameManager : Singleton<GameManager>
    {
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
            //switch (state)
            //{
            //}
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
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
            asyncOperation.allowSceneActivation = false;

            // シーンのロードが終わるまで待機
            while (!asyncOperation.isDone)
            {
                // ロードが進んだら進行状況を表示
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                Debug.Log("Loading progress: " + (progress * 100) + "%");

                // ロードが完了したらシーンをアクティブ化
                if (asyncOperation.progress >= 0.9f)
                {
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }

}
