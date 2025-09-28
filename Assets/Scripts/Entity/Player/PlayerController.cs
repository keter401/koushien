using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions input;

    private Vector2 moveInput;                          // 移動入力
    private Vector2 lookInput = Vector2.right;          // 視点入力
    private Vector3 dir = Vector3.right;                // 向き
    private Vector3 normalScale;                     // メッシュの元のスケール

    [SerializeField] private Transform drill = null;          // ドリルのTransform
    [SerializeField] private Transform drillMesh = null;      // ドリルのメッシュのTransform
    [SerializeField] private Transform center = null;        // 中心のTransform
    [SerializeField] private Transform mesh = null;        // メッシュのTransform
    [SerializeField] private Collider drillCollider = null; // ドリルのコライダー
    [SerializeField] private float moveSpeed = 20.0f;           // 移動速度
    [SerializeField] private float moveAcceleration = 10.0f;    // 加速度
    [SerializeField] private float linearDrag = 0.1f;          // リニアドラッグ
    [SerializeField] private float jumpPower = 10.0f;           // ジャンプ力
    [SerializeField] private float maxJumpTime = 0.3f;             // ジャンプの持続時間
    [SerializeField] private float groundRayLength = 0.1f;        // 地面判定用のレイの長さ
    [SerializeField] private float rotateSpeed = 180.0f;          // ドリルの回転速度
    [SerializeField] private float lowFrequencyVibration = 0.1f; // 低周波振動の強さ
    [SerializeField] private float highFrequencyVibration = 0.1f; // 高周波振動の強さ

    private Rigidbody rb;
    private PerlinShake drillShake;

    private float jumpTime = 0.0f; // ジャンプの持続時間
    private float drillRadius; // ドリルの中心からの距離

    private bool isOnGround = false; // 地面にいるかどうか
    private bool isJumping = false; // ジャンプ中かどうか

    private void Awake()
    {
        input = new InputSystem_Actions();


        // 入力イベントの登録
        input.Player.Move.started += OnMove;
        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += OnMove;

        input.Player.Jump.started += OnJump;
        input.Player.Jump.canceled += OnJump;

        input.Player.Look.started += OnLook;
        input.Player.Look.performed += OnLook;
        input.Player.Look.canceled += OnLook;

        input.Player.Attack.started += OnAttack;
        input.Player.Attack.performed += OnAttack;
        input.Player.Attack.canceled += OnAttack;

        // 入力アクションを有効化
        input.Enable();

        Debug.Log("PlayerController Awake called, InputSystem_Actions initialized and enabled.");
    }

    private void OnDestroy()
    {
        input?.Dispose();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // コンポーネント取得
        rb = GetComponent<Rigidbody>();
        drillShake = drillMesh.GetComponent<PerlinShake>();

        drillRadius = (drill.position - center.position).magnitude;
        normalScale = mesh.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        // ドリルの回転処理
        if (lookInput.sqrMagnitude > 0.001f) {
            // 入力方向の角度を取得
            float targetAngle = Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg;

            // centerを中心にドリルがどこに来るべきか計算
            float radius = drillRadius;
            Vector3 targetPos = center.position + (Quaternion.Euler(0, 0, targetAngle) * Vector3.right) * radius;

            // 現在のドリル位置から、回転すべき角度を求める
            Vector3 currentDir = drill.position - center.position;
            Vector3 targetDir = targetPos - center.position;

            float signedAngle = Vector3.SignedAngle(currentDir, targetDir, Vector3.forward);
            float stepAngle = Mathf.Clamp(signedAngle, -rotateSpeed * Time.deltaTime, rotateSpeed * Time.deltaTime);

            // 実際に中心を軸に回転
            drill.RotateAround(center.position, Vector3.forward, stepAngle);

            // 回転後に向きを修正（center -> drillの方向に向ける）
            Vector3 dir = drill.position - center.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            drill.rotation = Quaternion.Euler(0, 0, angle);
        }


        // コントローラーの振動処理
        if (drillCollider.enabled) {
            GamepadManager.instance.AddVibration(lowFrequencyVibration, highFrequencyVibration);
        }
    }

    private void FixedUpdate()
    {
        // 地面判定
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Physics.Raycast(origin, Vector3.down, out hit, groundRayLength);

        if(hit.collider != null) {
            isOnGround = hit.collider.tag == "Ground";
        } else {
            isOnGround = false;
        }


        Vector3 jumpAcce = Vector3.zero;

        // ジャンプの持続時間を更新
        if (isJumping) {
            jumpTime += Time.fixedDeltaTime;

            jumpAcce += Vector3.up * jumpPower; // ジャンプ中は常に上方向に力を加える

            // ジャンプの持続時間が最大値を超えたらジャンプを終了
            if (jumpTime > maxJumpTime) {
                isJumping = false;
            }
        }


        // 向きの更新
        if (moveInput.x != 0.0f) {
            dir = new Vector3(moveInput.x, 0.0f, 0.0f);
            dir.Normalize();

            Vector3 scale = normalScale;
            scale.x *= Mathf.Sign(dir.x);

            //mesh.localScale = scale; // 向きに応じて反転
            //transform.localScale = new Vector3(dir.x, 1.0f, 1.0f); // 向きに応じて反転
        }

        Vector3 moveAcce = Vector3.zero;

        float maxSpeed = moveSpeed;
        float velocityX = rb.linearVelocity.x;
        if (Mathf.Abs(velocityX) < maxSpeed) {
            moveAcce.x += moveInput.x * moveAcceleration;
        }

        moveAcce.x -= velocityX * linearDrag; // リニアドラッグの適用

        // 加速度の適応
        rb.AddForce(moveAcce + jumpAcce);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        // コントローラーからの入力の場合、視点入力を取得
        if (context.control.device is Gamepad) {
            lookInput = context.ReadValue<Vector2>();
            lookInput.Normalize();
        } else if(context.control.device is Mouse) {
            // マウスからの入力の場合、マウスの位置を取得
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 screenPos = Camera.main.WorldToScreenPoint(center.position);
            lookInput = (mousePosition - screenPos).normalized;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // 地面にいてボタンが押された瞬間に実行
        if (isOnGround && context.started) {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

            isJumping = true;
            jumpTime = 0.0f; // ジャンプの持続時間をリセット
        };

        // ボタンが離された
        if (context.canceled) {
            isJumping = false;
        }
    }


    private void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed) {
            drillCollider.enabled = true; // ドリルのコライダーを有効化
            drillShake.ShakeStart();
        }

        if (context.canceled) {
            drillCollider.enabled = false; // ドリルのコライダーを無効化
            drillShake.Stop();
        }
    }
}
