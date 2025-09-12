using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;

// 1. エネミーの状態インターフェース
public interface IEnemyState
{
    void Enter(EnemyController enemy);
    void Update(EnemyController enemy);
    void Exit(EnemyController enemy);

    // それぞれのステートで視界にモノが入った時の挙動が変わるため、インターフェースにいれてる
    void OnVisionTrigger(Collider other, EnemyController enemy);

    // こっちは敵本体に当たった時の処理を書く
    void OnEnemyCollision(Collider other, EnemyController enemy);

    // カメラから外れた時の処理
    void OnBecameInvisible(EnemyController enemy);
}


public class EnemyController : MonoBehaviour
{
    [Header("移動速度設定")]
    public float _PlayerMoveSpeed = 6f; // プレイヤーの基準速度

    // Start関数でそれぞれのステートのスピードを初期化する
    public float _WanderSpeedRatio; // うろうろ状態: 1/2
    public float _FleeSpeedRatio;   // 逃げ状態: 3/4
    public float _EscapeSpeedRatio; // 脱出状態: 3/4

    // 視界用の当たり判定はトリガーで作り、敵そのものの当たり判定は別のコライダーで作ること！
    [Header("視界コライダー設定")]
    [SerializeField] Collider _VisionCollider;

    [Header("本体コライダー設定")]
    [SerializeField] Collider _EnemyCollider;

    [Header("掘削設定")]
    public float _PlayerDigSpeed = 2f; // プレイヤーの基準掘削速度
    public float _DigSpeedRatio = 0.5f; // ほりほり状態: 1/2

    [Header("アイテム設定")]
    public int _WordCount = 0; // 取得したワード数
    public int _PartCount = 0; // 取得したパーツ数
    public int _EscapeThreshold = 10; // 脱出状態に移行する閾値

    [Header("所持インベントリ(敵)")]
    public ItemManager _Inventory;   // ← 受け取り先


    [Header("参照")]
    public Transform _Player;
    public Transform[] _EscapePoints; // 脱出ポイントの位置リスト
    [HideInInspector] public ItemManager _TargetCrystalManager; // ターゲットのクリスタルのアイテムマネージャー

    // 内部変数
    private int _MovingDirection = 1; // 1: 右, -1: 左
    private bool _IsInPlayerView = true;
    private Transform _CurrentTarget; // 現在のターゲット（クリスタルや脱出ポイント）
    private float _DigTimer = 0f;
    private float _CrystalHealthRequired = 0f; // クリスタルを破壊するのに必要な時間

    // 現在の状態
    private IEnemyState _CurrentState;

    // 各ステートのインスタンス
    public WanderState _WanderState = new WanderState();
    public FleeState _FleeState = new FleeState();
    public DigMovement _DigMovementState = new DigMovement();
    public DigState _DigState = new DigState();
    public EscapeState _EscapeState = new EscapeState();

    // プロパティ
    public float CurrentMoveSpeed { get; private set; }
    public int TotalItemCount => _WordCount + _PartCount;
    public bool HasItems => TotalItemCount > 0;
    public bool ShouldEscape => TotalItemCount >= _EscapeThreshold;
    public Transform NearestEscapePoint => GetNearestEscapePoint();

    private Rigidbody rb;
    private bool isInExit = false;

    void Start()
    {
        // プレイヤーとカメラの参照を設定
        if (_Player == null)
            _Player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // それぞれのステートの移動スピードをプレイヤーの速度を基準に初期化
        _WanderSpeedRatio = _PlayerMoveSpeed * 0.5f;
        _FleeSpeedRatio = _PlayerMoveSpeed * 0.75f;
        _EscapeSpeedRatio = _PlayerMoveSpeed * 0.75f;

        // 初期状態をうろうろ状態に設定
        TransitionToState(_WanderState);

        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (_Inventory == null) TryGetComponent(out _Inventory); // 同じGOに付いていれば拾う
    }

    void Update()
    {
        // 現在の状態のUpdateを実行
        _CurrentState?.Update(this);

        Debug.Log($"<color=#00FF00>Enemy State: {_CurrentState.GetType().Name}</color>");
    }

    // 状態遷移メソッド
    public void TransitionToState(IEnemyState newState)
    {
        _CurrentState?.Exit(this);
        _CurrentState = newState;
        _CurrentState.Enter(this);

        Debug.Log($"エネミー状態変更: {newState.GetType().Name}");
    }

    // 移動処理
    public void Move()
    {
        Vector3 movement = Vector3.right * _MovingDirection * CurrentMoveSpeed * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, rb.linearVelocity.z);
        //transform.position += movement;
    }

    private void OnTriggerStay(Collider other)
    {
        // もし視界になにか入っていたら
        // 現在のステートに処理を投げる
        _CurrentState?.OnVisionTrigger(other, this);

        if (other.gameObject.CompareTag("Exit")) {
            isInExit = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 現在のステートに処理を投げる
        _CurrentState?.OnEnemyCollision(collision.collider, this);

        if(collision.gameObject.CompareTag("Exit")) {
            isInExit = false;
        }
    }

    private void OnBecameInvisible()
    {
        // 現在のステートに処理を投げる
        // カメラから外れた時の処理は「逃げ状態」の時しか必要じゃないが、一応それぞれのステートで使えるようにしておく
        _CurrentState?.OnBecameInvisible(this);
    }

    // Uターン
    public void UTurn()
    {
        _MovingDirection *= -1;

        // y軸を中心に反転させる
        Vector3 rot = transform.rotation.eulerAngles;
        rot.y = (_MovingDirection == 1) ? 0f : 180f;
        transform.rotation = Quaternion.Euler(rot);
    }


    // プレイヤーから逃げる方向の計算
    public void FleeFromPlayer()
    {
        if (_Player == null) return;

        // x方向のどちらに逃げるべきかを計算
        Vector3 directionFromPlayer = transform.position - _Player.position;
        int targetDirection = directionFromPlayer.x > 0 ? -1 : 1;

        // 進んでいる方向にプレイヤーがいたらUターンさせる
        if (_MovingDirection != targetDirection)
        {
            UTurn();
        }
    }

    // 最寄りの脱出ポイントを取得
    public Transform GetNearestEscapePoint()
    {
        // 脱出ポイントが空だったり要素がなかった場合は返す
        if (_EscapePoints == null || _EscapePoints.Length == 0) return null;

        Transform nearest = null;
        float nearestDistance = float.MaxValue; // 初期値として最大値を入れておく

        // 一番近い脱出ポイントを計算する
        foreach (Transform escapePoint in _EscapePoints)
        {
            if (escapePoint == null) continue;

            float distance = Vector3.Distance(transform.position, escapePoint.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = escapePoint;
            }
        }

        return nearest;
    }

    // 脱出ポイントに向かって移動
    public void MoveToEscapePoint()
    {
        Transform escapePoint = NearestEscapePoint; // 一番近い脱出ポイントを格納
        if (escapePoint == null) return;

        Vector3 direction = (escapePoint.position - transform.position).normalized;
        int targetDirection = direction.x > 0 ? 1 : -1;

        if (_MovingDirection != targetDirection)
        {
            _MovingDirection = targetDirection;
            transform.rotation = Quaternion.LookRotation(Vector3.right * _MovingDirection);
        }

        Move();

        // 脱出ポイントに到達したかチェック
        //if (Vector3.Distance(transform.position, escapePoint.position) < 0.5f)
        if (isInExit) {
            EscapeSuccessfully();
        }
    }

    // 脱出成功
    private void EscapeSuccessfully()
    {
        Debug.Log($"エネミーが脱出成功！ 失ったアイテム: ワード{_WordCount}, パーツ{_PartCount}");
        Destroy(gameObject);
    }

    // プレイヤーの攻撃を受けた時
    public void TakeAttackFromPlayer()
    {
        // アイテムをドロップ
        if (HasItems)
        {
            DropItems();
        }

        Debug.Log("エネミーが倒された！");
        Destroy(gameObject);
    }

    // アイテムドロップ
    private void DropItems()
    {
        // プレイヤーにアイテムを渡す処理（未実装）

        Debug.Log($"ドロップ: ワード{_WordCount}, パーツ{_PartCount}");
    }

    // 移動速度設定
    public void SetMoveSpeed(float speedRatio)
    {
        CurrentMoveSpeed = _PlayerMoveSpeed * speedRatio;
    }

    // プロパティでプライベート変数にアクセス
    public bool IsInPlayerView => _IsInPlayerView;
}

// 3. うろうろ状態
public class WanderState : IEnemyState
{
    public void Enter(EnemyController enemy)
    {
        enemy.SetMoveSpeed(enemy._WanderSpeedRatio);
        Debug.Log("うろうろ状態開始");
    }

    public void Update(EnemyController enemy)
    {
        // 崖チェック（ガキの場合はfalse）
        if (!HasGroundAhead(enemy))
        {
            // なにも無かったらUターン
            enemy.UTurn();
        }

        // 移動処理
        //enemy.CheckAndHandleCollision();
        enemy.Move();
    }

    public void Exit(EnemyController enemy)
    {
        // クリーンアップ処理
    }

    // 敵の視線に触れた時の処理
    public void OnVisionTrigger(Collider other, EnemyController enemy)
    {
        // 視界にプレイヤーが見えたら逃げ状態に移行
        if (other.gameObject.CompareTag("Player"))
        {
            enemy.TransitionToState(enemy._FleeState);
        }

        // 視界に鉱石が見えたらほりほり状態に移行
        // 注意！！既にその鉱石が他の敵にターゲットされていたら無視する処理をいれること！
        if (other.gameObject.CompareTag("Crystal"))
        {
            enemy.TransitionToState(enemy._DigMovementState);
            enemy._DigMovementState.SetTargetCrystal(other.gameObject.transform); // クリスタルの座標を格納
        }
    }

    // 敵本体に当たった時の処理
    public void OnEnemyCollision(Collider other, EnemyController enemy)
    {
        // ダメージ処理
        if (other.gameObject.CompareTag("Player"))
        {
            enemy.TransitionToState(enemy._FleeState);
        }

        // Uターン処理
        if (other.gameObject.CompareTag("Wall"))
        {
            enemy.UTurn();
        }
    }

    public void OnBecameInvisible(EnemyController enemy)
    {

    }

    // 足元前方の地面有無をレイキャストで確認
    private bool HasGroundAhead(EnemyController enemy)
    {
        // 前方オフセットとレイ長さはローカル定数で十分
        const float forwardOffset = 0.5f;   // 体の少し前
        const float rayDownLength = 1.2f;   // 足元まで十分届く長さ

        Vector3 origin = enemy.transform.position
                       + enemy.transform.right.normalized * forwardOffset
                       + Vector3.up * 0.1f; // 地面めり込み対策で少し上から
        Vector3 dir = Vector3.down;

        bool hitSomething = Physics.Raycast(origin, dir, out RaycastHit hit, rayDownLength);

        // unity editorで描画
#if UNITY_EDITOR
        // ヒット状況に応じて色分け
        if (hitSomething)
        {
            Color c = hit.collider.CompareTag("Ground") ? Color.green : new Color(1f, 0.8f, 0f); // Ground=緑 / それ以外=黄
            // 実際のヒット距離まで描画
            Debug.DrawRay(origin, dir * hit.distance, c, 0f, false);
        }
        else
        {
            // 何も無ければ崖：赤で最大長さまで描画
            Debug.DrawRay(origin, dir * rayDownLength, Color.red, 0f, false);
        }
#endif

        if (hitSomething)
        {
            // 地面タグが Ground であることを確認
            return hit.collider.CompareTag("Ground");
        }

        // 何にも当たらなければ崖
        return false;
    }
}

// 4. 逃げ状態
public class FleeState : IEnemyState
{
    public void Enter(EnemyController enemy)
    {
        enemy.SetMoveSpeed(enemy._FleeSpeedRatio); // 移動スピードを更新
        enemy.FleeFromPlayer();　// 左右どちらに逃げるべきか計算
        Debug.Log("逃げ状態開始");
    }

    public void Update(EnemyController enemy)
    {
        // プレイヤーカメラから外れた場合
        if (!enemy.IsInPlayerView)
        {
            // 取得数に応じて状態遷移
            if (enemy.ShouldEscape)
            {
                enemy.TransitionToState(enemy._EscapeState);
            }
            else
            {
                enemy.TransitionToState(enemy._WanderState);
            }
            return;
        }

        // 崖チェック（ガキの場合はfalse）
        if (!HasGroundAhead(enemy))
        {
            // なにも無かったらUターン
            enemy.UTurn();
        }

        // 移動処理
        enemy.Move();
    }

    public void Exit(EnemyController enemy)
    {
        // クリーンアップ処理
    }

    public void OnVisionTrigger(Collider other, EnemyController enemy)
    {
        // プレイヤーが視界に入ったらUターン
        if (other.gameObject.CompareTag("Player"))
        {
            enemy.UTurn();
        }
    }

    public void OnEnemyCollision(Collider other, EnemyController enemy)
    {
        // Uターン処理
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("GroundEdge"))
        {
            enemy.UTurn();
        }

        // プレイヤーへのダメージ処理
        if (other.gameObject.CompareTag("Player"))
        {

        }
    }

    public void OnBecameInvisible(EnemyController enemy)
    {
        // パーツの取得数によってステートを変化させる
        if (enemy._PartCount < 5)
        {
            enemy.TransitionToState(enemy._WanderState);
        }
        else
        {
            enemy.TransitionToState(enemy._EscapeState);
        }
    }

    // 足元前方の地面有無をレイキャストで確認
    private bool HasGroundAhead(EnemyController enemy)
    {
        // 前方オフセットとレイ長さはローカル定数で十分
        const float forwardOffset = 0.5f;   // 体の少し前
        const float rayDownLength = 1.2f;   // 足元まで十分届く長さ

        Vector3 origin = enemy.transform.position
                       + enemy.transform.right.normalized * forwardOffset
                       + Vector3.up * 0.1f; // 地面めり込み対策で少し上から
        Vector3 dir = Vector3.down;

        bool hitSomething = Physics.Raycast(origin, dir, out RaycastHit hit, rayDownLength);

        // unity editorで描画
#if UNITY_EDITOR
        // ヒット状況に応じて色分け
        if (hitSomething)
        {
            Color c = hit.collider.CompareTag("Ground") ? Color.green : new Color(1f, 0.8f, 0f); // Ground=緑 / それ以外=黄
            // 実際のヒット距離まで描画
            Debug.DrawRay(origin, dir * hit.distance, c, 0f, false);
        }
        else
        {
            // 何も無ければ崖：赤で最大長さまで描画
            Debug.DrawRay(origin, dir * rayDownLength, Color.red, 0f, false);
        }
#endif

        if (hitSomething)
        {
            // 地面タグが Ground であることを確認
            return hit.collider.CompareTag("Ground");
        }

        // 何にも当たらなければ崖
        return false;
    }
}

// 5. ほりほり移行状態
public class DigMovement : IEnemyState
{
    private Transform _TargetCrystal;

    public void Enter(EnemyController enemy)
    {
        enemy.SetMoveSpeed(enemy._WanderSpeedRatio); // 掘っている間は移動しない
        Debug.Log("ほりほり移行状態開始");
    }

    public void Update(EnemyController enemy)
    {
        // ターゲットのクリスタルが存在するかチェック
        if (_TargetCrystal == null)
        {
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        // クリスタルまで行く処理（敵の前にクリスタルは確実にあるのでMove関数で良い）
        enemy.Move();

        // エネミーがクリスタルに到着したらステートを変更
        if (HasReachedCrystal(enemy))
        {
            enemy._DigState.SetTargetCrystal(_TargetCrystal); // ターゲットのクリスタルを次のほりほりステートにも渡す
            enemy.TransitionToState(enemy._DigState);

        }
    }

    public void Exit(EnemyController enemy)
    {
        _TargetCrystal = null;
    }

    public void SetTargetCrystal(Transform crystalTransform)
    {
        _TargetCrystal = crystalTransform;
        Debug.Log("クリスタルの座標を格納！！");
    }

    private bool HasReachedCrystal(EnemyController enemy)
    {
        // 鉱石との距離を測り、一定の値以下だったら着いた判定にする
        float distance = Vector3.Distance(enemy.transform.position, _TargetCrystal.position);
        //Debug.Log(distance);
        return distance < 2.0f;
    }


    public void OnVisionTrigger(Collider other, EnemyController enemy)
    {

    }

    public void OnEnemyCollision(Collider other, EnemyController enemy)
    {

    }

    public void OnBecameInvisible(EnemyController enemy)
    {

    }
}

// 6. ほりほり状態
public class DigState : IEnemyState
{
    private const string LOG = "[掘削]";

    private Transform _TargetCrystal;
    private Minerals _TargetCrystalMinerals;

    private float _DigAccumulator; // 掘削DPSの累積（整数化のため）

    public void Enter(EnemyController enemy)
    {
        if (_TargetCrystal == null)
        {
            Debug.LogWarning($"{LOG} Enter: ターゲット未設定のため、うろうろへ遷移");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        enemy.SetMoveSpeed(0f); // 掘削中は移動停止

        if (!_TargetCrystal.TryGetComponent(out _TargetCrystalMinerals) || _TargetCrystalMinerals == null)
        {
            Debug.LogWarning($"{LOG} Enter: Minerals が見つかりません。うろうろへ遷移");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        _DigAccumulator = 0f;
        Debug.Log($"{LOG} Enter: クリスタル='{_TargetCrystal.name}', maxHP={_TargetCrystalMinerals.maxHP}, wordCount={_TargetCrystalMinerals.wordCount}");
    }

    public void Update(EnemyController enemy)
    {
        if (enemy == null)
        {
            Debug.LogError($"{LOG} Update: enemy が null です");
            return;
        }
        if (_TargetCrystal == null)
        {
            Debug.LogWarning($"{LOG} Update: ターゲットを見失いました。うろうろへ遷移");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }
        if (_TargetCrystalMinerals == null &&
            (!_TargetCrystal.TryGetComponent(out _TargetCrystalMinerals) || _TargetCrystalMinerals == null))
        {
            Debug.LogWarning($"{LOG} Update: Minerals が見つかりません。うろうろへ遷移");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        // 掘削DPS → 整数ダメージに束ねる
        float dps = enemy._PlayerDigSpeed * enemy._DigSpeedRatio;
        _DigAccumulator += dps * Time.deltaTime;

        int damage = Mathf.FloorToInt(_DigAccumulator);
        if (damage > 0)
        {
            _DigAccumulator -= damage;

            ItemManager receiver = enemy._Inventory; // 受け取り先（敵インベントリ）
            int dropped = _TargetCrystalMinerals.Mine(damage, receiver);

            if (dropped > 0)
            {
                enemy._WordCount += dropped; // メタ用カウントを更新（UIや逃走判定）
                Debug.Log($"{LOG} 今回のドロップ数={dropped} / 敵の所持ワード合計={enemy._WordCount}");
            }
        }

        // 取得数しきい値で脱出へ
        if (enemy.ShouldEscape)
        {
            Debug.Log($"{LOG} しきい値到達（Word={enemy._WordCount}, Part={enemy._PartCount}）→ 脱出へ遷移");
            enemy.TransitionToState(enemy._EscapeState);
            return;
        }

        // 破壊済みなら終了
        if (_TargetCrystalMinerals.GetCurrentHP() <= 0)
        {
            Debug.Log($"{LOG} クリスタル破壊 → うろうろへ遷移");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        Debug.Log($"<color=#0000FF>Word={enemy._WordCount}, クリスタルHP={_TargetCrystalMinerals.GetCurrentHP()} </color>");
    }

    public void Exit(EnemyController enemy)
    {
        Debug.Log($"{LOG} Exit: 掘削を終了します");
        _TargetCrystal = null;
        _TargetCrystalMinerals = null;
        _DigAccumulator = 0f;
    }

    public void SetTargetCrystal(Transform crystal)
    {
        _TargetCrystal = crystal;
        Debug.Log($"{LOG} SetTargetCrystal: {(_TargetCrystal ? _TargetCrystal.name : "NULL")}");
    }

    // ---- IEnemyState 必須メソッド（必要なら実装を追加）----
    public void OnVisionTrigger(Collider other, EnemyController enemy)
    {
        //Debug.Log($"{LOG} OnVisionTrigger: tag={other.tag}");

    }

    public void OnEnemyCollision(Collider other, EnemyController enemy)
    {
        //Debug.Log($"{LOG} OnEnemyCollision: tag={other.tag}");
    }

    public void OnBecameInvisible(EnemyController enemy)
    {
        Debug.Log($"{LOG} OnBecameInvisible: カメラ外になりました");
    }
}
// 7. 脱出状態
public class EscapeState : IEnemyState
{
    public void Enter(EnemyController enemy)
    {
        enemy.SetMoveSpeed(enemy._EscapeSpeedRatio); // 移動スピードを更新
        Debug.Log("脱出状態開始");
    }

    public void Update(EnemyController enemy)
    {
        // 脱出ポイントに向かって移動、関数の中で到達処理や一番近い脱出ポイントの計算をしている
        enemy.MoveToEscapePoint();
    }

    public void Exit(EnemyController enemy)
    {
        // クリーンアップ処理
    }


    public void OnVisionTrigger(Collider other, EnemyController enemy)
    {
        // 視界にプレイヤーが見えたら逃げ状態に移行
        if (other.gameObject.CompareTag("Player"))
        {
            enemy.TransitionToState(enemy._FleeState);
        }
    }

    public void OnEnemyCollision(Collider other, EnemyController enemy)
    {

    }

    public void OnBecameInvisible(EnemyController enemy)
    {

    }
}