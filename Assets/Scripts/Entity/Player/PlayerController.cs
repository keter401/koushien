using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions input;

    private Vector2 moveInput;                          // �ړ�����
    private Vector2 lookInput = Vector2.right;          // ���_����
    private Vector3 dir = Vector3.right;                // ����
    private Vector3 normalScale;                     // ���b�V���̌��̃X�P�[��

    [SerializeField] private Transform drill = null;          // �h������Transform
    [SerializeField] private Transform drillMesh = null;      // �h�����̃��b�V����Transform
    [SerializeField] private Transform center = null;        // ���S��Transform
    [SerializeField] private Transform mesh = null;        // ���b�V����Transform
    [SerializeField] private Collider drillCollider = null; // �h�����̃R���C�_�[
    [SerializeField] private float moveSpeed = 20.0f;           // �ړ����x
    [SerializeField] private float moveAcceleration = 10.0f;    // �����x
    [SerializeField] private float linearDrag = 0.1f;          // ���j�A�h���b�O
    [SerializeField] private float jumpPower = 10.0f;           // �W�����v��
    [SerializeField] private float maxJumpTime = 0.3f;             // �W�����v�̎�������
    [SerializeField] private float groundRayLength = 0.1f;        // �n�ʔ���p�̃��C�̒���
    [SerializeField] private float rotateSpeed = 180.0f;          // �h�����̉�]���x
    [SerializeField] private float lowFrequencyVibration = 0.1f; // ����g�U���̋���
    [SerializeField] private float highFrequencyVibration = 0.1f; // �����g�U���̋���

    private Rigidbody rb;
    private PerlinShake drillShake;

    private float jumpTime = 0.0f; // �W�����v�̎�������
    private float drillRadius; // �h�����̒��S����̋���

    private bool isOnGround = false; // �n�ʂɂ��邩�ǂ���
    private bool isJumping = false; // �W�����v�����ǂ���

    private void Awake()
    {
        input = new InputSystem_Actions();


        // ���̓C�x���g�̓o�^
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

        // ���̓A�N�V������L����
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
        // �R���|�[�l���g�擾
        rb = GetComponent<Rigidbody>();
        drillShake = drillMesh.GetComponent<PerlinShake>();

        drillRadius = (drill.position - center.position).magnitude;
        normalScale = mesh.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        // �h�����̉�]����
        if (lookInput.sqrMagnitude > 0.001f) {
            // ���͕����̊p�x���擾
            float targetAngle = Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg;

            // center�𒆐S�Ƀh�������ǂ��ɗ���ׂ����v�Z
            float radius = drillRadius;
            Vector3 targetPos = center.position + (Quaternion.Euler(0, 0, targetAngle) * Vector3.right) * radius;

            // ���݂̃h�����ʒu����A��]���ׂ��p�x�����߂�
            Vector3 currentDir = drill.position - center.position;
            Vector3 targetDir = targetPos - center.position;

            float signedAngle = Vector3.SignedAngle(currentDir, targetDir, Vector3.forward);
            float stepAngle = Mathf.Clamp(signedAngle, -rotateSpeed * Time.deltaTime, rotateSpeed * Time.deltaTime);

            // ���ۂɒ��S�����ɉ�]
            drill.RotateAround(center.position, Vector3.forward, stepAngle);

            // ��]��Ɍ������C���icenter -> drill�̕����Ɍ�����j
            Vector3 dir = drill.position - center.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            drill.rotation = Quaternion.Euler(0, 0, angle);
        }


        // �R���g���[���[�̐U������
        if (drillCollider.enabled) {
            GamepadManager.instance.AddVibration(lowFrequencyVibration, highFrequencyVibration);
        }
    }

    private void FixedUpdate()
    {
        // �n�ʔ���
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Physics.Raycast(origin, Vector3.down, out hit, groundRayLength);

        if(hit.collider != null) {
            isOnGround = hit.collider.tag == "Ground";
        } else {
            isOnGround = false;
        }


        Vector3 jumpAcce = Vector3.zero;

        // �W�����v�̎������Ԃ��X�V
        if (isJumping) {
            jumpTime += Time.fixedDeltaTime;

            jumpAcce += Vector3.up * jumpPower; // �W�����v���͏�ɏ�����ɗ͂�������

            // �W�����v�̎������Ԃ��ő�l�𒴂�����W�����v���I��
            if (jumpTime > maxJumpTime) {
                isJumping = false;
            }
        }


        // �����̍X�V
        if (moveInput.x != 0.0f) {
            dir = new Vector3(moveInput.x, 0.0f, 0.0f);
            dir.Normalize();

            Vector3 scale = normalScale;
            scale.x *= Mathf.Sign(dir.x);

            //mesh.localScale = scale; // �����ɉ����Ĕ��]
            //transform.localScale = new Vector3(dir.x, 1.0f, 1.0f); // �����ɉ����Ĕ��]
        }

        Vector3 moveAcce = Vector3.zero;

        float maxSpeed = moveSpeed;
        float velocityX = rb.linearVelocity.x;
        if (Mathf.Abs(velocityX) < maxSpeed) {
            moveAcce.x += moveInput.x * moveAcceleration;
        }

        moveAcce.x -= velocityX * linearDrag; // ���j�A�h���b�O�̓K�p

        // �����x�̓K��
        rb.AddForce(moveAcce + jumpAcce);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        // �R���g���[���[����̓��͂̏ꍇ�A���_���͂��擾
        if (context.control.device is Gamepad) {
            lookInput = context.ReadValue<Vector2>();
            lookInput.Normalize();
        } else if(context.control.device is Mouse) {
            // �}�E�X����̓��͂̏ꍇ�A�}�E�X�̈ʒu���擾
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 screenPos = Camera.main.WorldToScreenPoint(center.position);
            lookInput = (mousePosition - screenPos).normalized;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // �n�ʂɂ��ă{�^���������ꂽ�u�ԂɎ��s
        if (isOnGround && context.started) {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

            isJumping = true;
            jumpTime = 0.0f; // �W�����v�̎������Ԃ����Z�b�g
        };

        // �{�^���������ꂽ
        if (context.canceled) {
            isJumping = false;
        }
    }


    private void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed) {
            drillCollider.enabled = true; // �h�����̃R���C�_�[��L����
            drillShake.ShakeStart();
        }

        if (context.canceled) {
            drillCollider.enabled = false; // �h�����̃R���C�_�[�𖳌���
            drillShake.Stop();
        }
    }
}
