using System;
using Unity.VisualScripting;
using UnityEngine;

public class GManager : MonoBehaviour
{
    public static GManager instance { get; private set; } // シングルトンインスタンス
    public static event Action OnTimerStart;    // タイマー開始イベント
    public static event Action OnTimerEnd;      // タイマー終了イベント


    [SerializeField] private WordDataBase wordDataBase = null; // 単語データベース

    [Header("時間設定")]
    [SerializeField] private bool isTimerEnabled = false;   // タイマーを有効にするか
    [SerializeField] private bool isUnscaleTime = false;    // タイマーの時間をスケールしないか
    [SerializeField] private float timer = 60.0f;           // タイマーの値

    public float maxActionTime = 60.0f;                     // アクションパートの最大時間


    [SerializeField, Header("現在のゲーム状態")]
    private GameState currentGameState = new GameState();
    private GameState nextState = new ActionGameState(); 


    void Awake()
    {
        // シングルトンの初期化
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでオブジェクトを保持


            // タイマーのログ出力
            OnTimerStart += () => Debug.Log($"[GManager] タイマーが開始されました。 ({timer} 秒)");
            OnTimerEnd += () => Debug.Log("[GManager] タイマーが終了しました。");
        } else {
            Destroy(gameObject); // 既に存在する場合は新しいインスタンスを破棄
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 状態が変わった場合、次のフレームで状態を更新
        if (nextState != null) {
            currentGameState = nextState;
            nextState = null;

            currentGameState.Enter();
        }


        // タイマー処理
        if (isTimerEnabled) {
            timer -= isUnscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;

            // タイマーが0以下になった場合、イベントを発火
            if (timer <= 0.0f) {
                timer = 0.0f;
                OnTimerEnd?.Invoke();
            }
        }

        currentGameState.Update();
    }


    void LateUpdate()
    {
        currentGameState.LateUpdate();
    }


    public WordDataBase GetWordDataBase()
    {
        return wordDataBase;
    }


    /// <summary>
    /// 現在のゲームの状態を取得します。
    /// </summary>
    /// <returns>現在のゲームの状態</returns>
    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }


    /// <summary>
    /// ゲームの状態を設定します。
    /// </summary>
    /// <param name="newState">ゲームの状態</param>
    public void SetGameState(GameState newState)
    {
        if (currentGameState != newState) {
            nextState = newState;
        }
    }


    /// <summary>
    /// 現在のタイマーの値を取得します。
    /// </summary>
    /// <returns>現在のタイマー値</returns>
    public float GetTimer()
    {
        return timer;
    }


    /// <summary>
    /// タイマーを開始します。
    /// </summary>
    /// <param name="time">最大時間</param>
    /// <param name="unscaleTime">スケールタイムを有効にするか</param>
    public void StartTimer(float time, bool unscaleTime = false)
    {
        isTimerEnabled = true;
        isUnscaleTime = unscaleTime;
        timer = time;

        OnTimerStart?.Invoke();
    }


    /// <summary>
    ///  タイマーをリセットします。
    /// </summary>
    public void ResetTimer()
    {
        isTimerEnabled = false;
        isUnscaleTime = false;
        timer = 60.0f;          // デフォルトのタイマー値にリセット
        OnTimerEnd?.Invoke();   // タイマー終了イベントを発火
    }
}
