using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UIElements;

// 1. �G�l�~�[�̏�ԃC���^�[�t�F�[�X
public interface IEnemyState
{
    void Enter(EnemyController enemy);
    void Update(EnemyController enemy);
    void Exit(EnemyController enemy);

    // ���ꂼ��̃X�e�[�g�Ŏ��E�Ƀ��m�����������̋������ς�邽�߁A�C���^�[�t�F�[�X�ɂ���Ă�
    void OnVisionTrigger(Collider other, EnemyController enemy);

    // �������͓G�{�̂ɓ����������̏���������
    void OnEnemyCollision(Collider other, EnemyController enemy);

    // �J��������O�ꂽ���̏���
    void OnBecameInvisible(EnemyController enemy);
}


public class EnemyController : MonoBehaviour
{
    [Header("�ړ����x�ݒ�")]
    public float _PlayerMoveSpeed = 6f; // �v���C���[�̊���x

    // Start�֐��ł��ꂼ��̃X�e�[�g�̃X�s�[�h������������
    public float _WanderSpeedRatio; // ���낤����: 1/2
    public float _FleeSpeedRatio;   // �������: 3/4
    public float _EscapeSpeedRatio; // �E�o���: 3/4

    // ���E�p�̓����蔻��̓g���K�[�ō��A�G���̂��̂̓����蔻��͕ʂ̃R���C�_�[�ō�邱�ƁI
    [Header("���E�R���C�_�[�ݒ�")]
    [SerializeField] Collider _VisionCollider;

    [Header("�{�̃R���C�_�[�ݒ�")]
    [SerializeField] Collider _EnemyCollider;

    [Header("�@��ݒ�")]
    public float _PlayerDigSpeed = 2f; // �v���C���[�̊�@�푬�x
    public float _DigSpeedRatio = 0.5f; // �ق�ق���: 1/2

    [Header("�A�C�e���ݒ�")]
    public int _WordCount = 0; // �擾�������[�h��
    public int _PartCount = 0; // �擾�����p�[�c��
    public int _EscapeThreshold = 10; // �E�o��ԂɈڍs����臒l

    [Header("�����C���x���g��(�G)")]
    public ItemManager _Inventory;   // �� �󂯎���


    [Header("�Q��")]
    public Transform _Player;
    public Transform[] _EscapePoints; // �E�o�|�C���g�̈ʒu���X�g
    [HideInInspector] public ItemManager _TargetCrystalManager; // �^�[�Q�b�g�̃N���X�^���̃A�C�e���}�l�[�W���[

    // �����ϐ�
    private int _MovingDirection = 1; // 1: �E, -1: ��
    private bool _IsInPlayerView = true;
    private Transform _CurrentTarget; // ���݂̃^�[�Q�b�g�i�N���X�^����E�o�|�C���g�j
    private float _DigTimer = 0f;
    private float _CrystalHealthRequired = 0f; // �N���X�^����j�󂷂�̂ɕK�v�Ȏ���

    // ���݂̏��
    private IEnemyState _CurrentState;

    // �e�X�e�[�g�̃C���X�^���X
    public WanderState _WanderState = new WanderState();
    public FleeState _FleeState = new FleeState();
    public DigMovement _DigMovementState = new DigMovement();
    public DigState _DigState = new DigState();
    public EscapeState _EscapeState = new EscapeState();

    // �v���p�e�B
    public float CurrentMoveSpeed { get; private set; }
    public int TotalItemCount => _WordCount + _PartCount;
    public bool HasItems => TotalItemCount > 0;
    public bool ShouldEscape => TotalItemCount >= _EscapeThreshold;
    public Transform NearestEscapePoint => GetNearestEscapePoint();

    private Rigidbody rb;
    private bool isInExit = false;

    void Start()
    {
        // �v���C���[�ƃJ�����̎Q�Ƃ�ݒ�
        if (_Player == null)
            _Player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // ���ꂼ��̃X�e�[�g�̈ړ��X�s�[�h���v���C���[�̑��x����ɏ�����
        _WanderSpeedRatio = _PlayerMoveSpeed * 0.5f;
        _FleeSpeedRatio = _PlayerMoveSpeed * 0.75f;
        _EscapeSpeedRatio = _PlayerMoveSpeed * 0.75f;

        // ������Ԃ����낤���Ԃɐݒ�
        TransitionToState(_WanderState);

        rb = GetComponent<Rigidbody>();
    }

    void Awake()
    {
        if (_Inventory == null) TryGetComponent(out _Inventory); // ����GO�ɕt���Ă���ΏE��
    }

    void Update()
    {
        // ���݂̏�Ԃ�Update�����s
        _CurrentState?.Update(this);

        Debug.Log($"<color=#00FF00>Enemy State: {_CurrentState.GetType().Name}</color>");
    }

    // ��ԑJ�ڃ��\�b�h
    public void TransitionToState(IEnemyState newState)
    {
        _CurrentState?.Exit(this);
        _CurrentState = newState;
        _CurrentState.Enter(this);

        Debug.Log($"�G�l�~�[��ԕύX: {newState.GetType().Name}");
    }

    // �ړ�����
    public void Move()
    {
        Vector3 movement = Vector3.right * _MovingDirection * CurrentMoveSpeed * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, rb.linearVelocity.z);
        //transform.position += movement;
        
    }

    private void OnTriggerStay(Collider other)
    {
        // �������E�ɂȂɂ������Ă�����
        // ���݂̃X�e�[�g�ɏ����𓊂���
        _CurrentState?.OnVisionTrigger(other, this);

        if (other.gameObject.CompareTag("Exit")) {
            isInExit = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ���݂̃X�e�[�g�ɏ����𓊂���
        _CurrentState?.OnEnemyCollision(collision.collider, this);

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Exit"))
        {
            isInExit = false;
        }
    }

    private void OnBecameInvisible()
    {
        // ���݂̃X�e�[�g�ɏ����𓊂���
        // �J��������O�ꂽ���̏����́u������ԁv�̎������K�v����Ȃ����A�ꉞ���ꂼ��̃X�e�[�g�Ŏg����悤�ɂ��Ă���
        _CurrentState?.OnBecameInvisible(this);
    }

    // U�^�[��
    public void UTurn()
    {
        _MovingDirection *= -1;

        // y���𒆐S�ɔ��]������
        Vector3 rot = transform.rotation.eulerAngles;
        rot.y = (_MovingDirection == 1) ? 0f : 180f;
        transform.rotation = Quaternion.Euler(rot);
    }


    // �v���C���[���瓦��������̌v�Z
    public void FleeFromPlayer()
    {
        if (_Player == null) return;

        // x�����̂ǂ���ɓ�����ׂ������v�Z
        Vector3 directionFromPlayer = transform.position - _Player.position;
        int targetDirection = directionFromPlayer.x > 0 ? -1 : 1;

        // �i��ł�������Ƀv���C���[��������U�^�[��������
        if (_MovingDirection != targetDirection)
        {
            UTurn();
        }
    }

    // �Ŋ��̒E�o�|�C���g���擾
    public Transform GetNearestEscapePoint()
    {
        // �E�o�|�C���g���󂾂�����v�f���Ȃ������ꍇ�͕Ԃ�
        if (_EscapePoints == null || _EscapePoints.Length == 0) return null;

        Transform nearest = null;
        float nearestDistance = float.MaxValue; // �����l�Ƃ��čő�l�����Ă���

        // ��ԋ߂��E�o�|�C���g���v�Z����
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

    // �E�o�|�C���g�Ɍ������Ĉړ�
    public void MoveToEscapePoint()
    {
        Transform escapePoint = NearestEscapePoint; // ��ԋ߂��E�o�|�C���g���i�[
        if (escapePoint == null) return;

        Vector3 direction = (escapePoint.position - transform.position).normalized;
        int targetDirection = direction.x > 0 ? 1 : -1;

        if (_MovingDirection != targetDirection)
        {
            _MovingDirection = targetDirection;
            transform.rotation = Quaternion.LookRotation(Vector3.right * _MovingDirection);
        }

        Move();

        // �E�o�|�C���g�ɓ��B�������`�F�b�N
        //if (Vector3.Distance(transform.position, escapePoint.position) < 0.5f)
        if (isInExit) {
            EscapeSuccessfully();
        }
    }

    // �E�o����
    private void EscapeSuccessfully()
    {
        Debug.Log($"�G�l�~�[���E�o�����I �������A�C�e��: ���[�h{_WordCount}, �p�[�c{_PartCount}");
        Destroy(gameObject);
    }

    // �v���C���[�̍U�����󂯂���
    public void TakeAttackFromPlayer()
    {
        // �A�C�e�����h���b�v
        if (HasItems)
        {
            DropItems();
        }

        Debug.Log("�G�l�~�[���|���ꂽ�I");
        Destroy(gameObject);
    }

    // �A�C�e���h���b�v
    private void DropItems()
    {
        // �v���C���[�ɃA�C�e����n�������i�������j

        Debug.Log($"�h���b�v: ���[�h{_WordCount}, �p�[�c{_PartCount}");
    }

    // �ړ����x�ݒ�
    public void SetMoveSpeed(float speedRatio)
    {
        CurrentMoveSpeed = _PlayerMoveSpeed * speedRatio;
    }

    // �v���p�e�B�Ńv���C�x�[�g�ϐ��ɃA�N�Z�X
    public bool IsInPlayerView => _IsInPlayerView;
}

// 3. ���낤����
public class WanderState : IEnemyState
{
    public void Enter(EnemyController enemy)
    {
        enemy.SetMoveSpeed(enemy._WanderSpeedRatio);
        Debug.Log("���낤���ԊJ�n");
    }

    public void Update(EnemyController enemy)
    {
        // �R�`�F�b�N�i�K�L�̏ꍇ��false�j
        if (!HasGroundAhead(enemy))
        {
            // �Ȃɂ�����������U�^�[��
            enemy.UTurn();
        }

        // �ړ�����
        //enemy.CheckAndHandleCollision();
        enemy.Move();
    }

    public void Exit(EnemyController enemy)
    {
        // �N���[���A�b�v����
    }

    // �G�̎����ɐG�ꂽ���̏���
    public void OnVisionTrigger(Collider other, EnemyController enemy)
    {
        // ���E�Ƀv���C���[���������瓦����ԂɈڍs
        if (other.gameObject.CompareTag("Player"))
        {
            enemy.TransitionToState(enemy._FleeState);
        }

        // ���E�ɍz�΂���������ق�ق��ԂɈڍs
        // ���ӁI�I���ɂ��̍z�΂����̓G�Ƀ^�[�Q�b�g����Ă����疳�����鏈��������邱�ƁI
        if (other.gameObject.CompareTag("Crystal"))
        {
            enemy.TransitionToState(enemy._DigMovementState);
            enemy._DigMovementState.SetTargetCrystal(other.gameObject.transform); // �N���X�^���̍��W���i�[
        }
    }

    // �G�{�̂ɓ����������̏���
    public void OnEnemyCollision(Collider other, EnemyController enemy)
    {
        // �_���[�W����
        if (other.gameObject.CompareTag("Player"))
        {
            enemy.TransitionToState(enemy._FleeState);
        }

        // U�^�[������
        if (other.gameObject.CompareTag("Wall"))
        {
            enemy.UTurn();
        }
        // U�^�[������
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemy.UTurn();
        }
    }

    public void OnBecameInvisible(EnemyController enemy)
    {

    }

    // �����O���̒n�ʗL�������C�L���X�g�Ŋm�F
    private bool HasGroundAhead(EnemyController enemy)
    {
        // �O���I�t�Z�b�g�ƃ��C�����̓��[�J���萔�ŏ\��
        const float forwardOffset = 0.5f;   // �̂̏����O
        const float rayDownLength = 1.2f;   // �����܂ŏ\���͂�����

        Vector3 origin = enemy.transform.position
                       + enemy.transform.right.normalized * forwardOffset
                       + Vector3.up * 0.1f; // �n�ʂ߂荞�ݑ΍�ŏ����ォ��
        Vector3 dir = Vector3.down;

        bool hitSomething = Physics.Raycast(origin, dir, out RaycastHit hit, rayDownLength);

        // unity editor�ŕ`��
#if UNITY_EDITOR
        // �q�b�g�󋵂ɉ����ĐF����
        if (hitSomething)
        {
            Color c = hit.collider.CompareTag("Ground") ? Color.green : new Color(1f, 0.8f, 0f); // Ground=�� / ����ȊO=��
            // ���ۂ̃q�b�g�����܂ŕ`��
            Debug.DrawRay(origin, dir * hit.distance, c, 0f, false);
        }
        else
        {
            // ����������ΊR�F�Ԃōő咷���܂ŕ`��
            Debug.DrawRay(origin, dir * rayDownLength, Color.red, 0f, false);
        }
#endif

        if (hitSomething)
        {
            // �n�ʃ^�O�� Ground �ł��邱�Ƃ��m�F
            return hit.collider.CompareTag("Ground");
        }

        // ���ɂ�������Ȃ���ΊR
        return false;
    }
}

// 4. �������
public class FleeState : IEnemyState
{
    public void Enter(EnemyController enemy)
    {
        enemy.SetMoveSpeed(enemy._FleeSpeedRatio); // �ړ��X�s�[�h���X�V
        enemy.FleeFromPlayer();// ���E�ǂ���ɓ�����ׂ����v�Z
        Debug.Log("������ԊJ�n");
    }

    public void Update(EnemyController enemy)
    {
        // �v���C���[�J��������O�ꂽ�ꍇ
        if (!enemy.IsInPlayerView)
        {
            // �擾���ɉ����ď�ԑJ��
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

        // �R�`�F�b�N�i�K�L�̏ꍇ��false�j
        if (!HasGroundAhead(enemy))
        {
            // �Ȃɂ�����������U�^�[��
            enemy.UTurn();
        }

        // �ړ�����
        enemy.Move();
    }

    public void Exit(EnemyController enemy)
    {
        // �N���[���A�b�v����
    }

    public void OnVisionTrigger(Collider other, EnemyController enemy)
    {
        // �v���C���[�����E�ɓ�������U�^�[��
        if (other.gameObject.CompareTag("Player"))
        {
            enemy.UTurn();
        }
    }

    public void OnEnemyCollision(Collider other, EnemyController enemy)
    {
        // U�^�[������
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("GroundEdge"))
        {
            enemy.UTurn();
        }

        // �v���C���[�ւ̃_���[�W����
        if (other.gameObject.CompareTag("Player"))
        {

        }
    }

    public void OnBecameInvisible(EnemyController enemy)
    {
        // �p�[�c�̎擾���ɂ���ăX�e�[�g��ω�������
        if (enemy._PartCount < 5)
        {
            enemy.TransitionToState(enemy._WanderState);
        }
        else
        {
            enemy.TransitionToState(enemy._EscapeState);
        }
    }

    // �����O���̒n�ʗL�������C�L���X�g�Ŋm�F
    private bool HasGroundAhead(EnemyController enemy)
    {
        // �O���I�t�Z�b�g�ƃ��C�����̓��[�J���萔�ŏ\��
        const float forwardOffset = 0.5f;   // �̂̏����O
        const float rayDownLength = 1.2f;   // �����܂ŏ\���͂�����

        Vector3 origin = enemy.transform.position
                       + enemy.transform.right.normalized * forwardOffset
                       + Vector3.up * 0.1f; // �n�ʂ߂荞�ݑ΍�ŏ����ォ��
        Vector3 dir = Vector3.down;

        bool hitSomething = Physics.Raycast(origin, dir, out RaycastHit hit, rayDownLength);

        // unity editor�ŕ`��
#if UNITY_EDITOR
        // �q�b�g�󋵂ɉ����ĐF����
        if (hitSomething)
        {
            Color c = hit.collider.CompareTag("Ground") ? Color.green : new Color(1f, 0.8f, 0f); // Ground=�� / ����ȊO=��
            // ���ۂ̃q�b�g�����܂ŕ`��
            Debug.DrawRay(origin, dir * hit.distance, c, 0f, false);
        }
        else
        {
            // ����������ΊR�F�Ԃōő咷���܂ŕ`��
            Debug.DrawRay(origin, dir * rayDownLength, Color.red, 0f, false);
        }
#endif

        if (hitSomething)
        {
            // �n�ʃ^�O�� Ground �ł��邱�Ƃ��m�F
            return hit.collider.CompareTag("Ground");
        }

        // ���ɂ�������Ȃ���ΊR
        return false;
    }
}

// 5. �ق�ق�ڍs���
public class DigMovement : IEnemyState
{
    private Transform _TargetCrystal;

    public void Enter(EnemyController enemy)
    {
        enemy.SetMoveSpeed(enemy._WanderSpeedRatio); // �@���Ă���Ԃ͈ړ����Ȃ�
        Debug.Log("�ق�ق�ڍs��ԊJ�n");
    }

    public void Update(EnemyController enemy)
    {
        // �^�[�Q�b�g�̃N���X�^�������݂��邩�`�F�b�N
        if (_TargetCrystal == null)
        {
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        // �N���X�^���܂ōs�������i�G�̑O�ɃN���X�^���͊m���ɂ���̂�Move�֐��ŗǂ��j
        enemy.Move();

        // �G�l�~�[���N���X�^���ɓ���������X�e�[�g��ύX
        if (HasReachedCrystal(enemy))
        {
            enemy._DigState.SetTargetCrystal(_TargetCrystal); // �^�[�Q�b�g�̃N���X�^�������̂ق�ق�X�e�[�g�ɂ��n��
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
        Debug.Log("�N���X�^���̍��W���i�[�I�I");
    }

    private bool HasReachedCrystal(EnemyController enemy)
    {
        // �z�΂Ƃ̋����𑪂�A���̒l�ȉ��������璅��������ɂ���
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

// 6. �ق�ق���
public class DigState : IEnemyState
{
    private const string LOG = "[�@��]";

    private Transform _TargetCrystal;
    private Minerals _TargetCrystalMinerals;

    private float _DigAccumulator; // �@��DPS�̗ݐρi�������̂��߁j

    public void Enter(EnemyController enemy)
    {
        if (_TargetCrystal == null)
        {
            Debug.LogWarning($"{LOG} Enter: �^�[�Q�b�g���ݒ�̂��߁A���낤��֑J��");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        enemy.SetMoveSpeed(0f); // �@�풆�͈ړ���~

        if (!_TargetCrystal.TryGetComponent(out _TargetCrystalMinerals) || _TargetCrystalMinerals == null)
        {
            Debug.LogWarning($"{LOG} Enter: Minerals ��������܂���B���낤��֑J��");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        _DigAccumulator = 0f;
        Debug.Log($"{LOG} Enter: �N���X�^��='{_TargetCrystal.name}', maxHP={_TargetCrystalMinerals.maxHP}, wordCount={_TargetCrystalMinerals.wordCount}");
    }

    public void Update(EnemyController enemy)
    {
        if (enemy == null)
        {
            Debug.LogError($"{LOG} Update: enemy �� null �ł�");
            return;
        }
        if (_TargetCrystal == null)
        {
            Debug.LogWarning($"{LOG} Update: �^�[�Q�b�g���������܂����B���낤��֑J��");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }
        if (_TargetCrystalMinerals == null &&
            (!_TargetCrystal.TryGetComponent(out _TargetCrystalMinerals) || _TargetCrystalMinerals == null))
        {
            Debug.LogWarning($"{LOG} Update: Minerals ��������܂���B���낤��֑J��");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        // �@��DPS �� �����_���[�W�ɑ��˂�
        float dps = enemy._PlayerDigSpeed * enemy._DigSpeedRatio;
        _DigAccumulator += dps * Time.deltaTime;

        int damage = Mathf.FloorToInt(_DigAccumulator);
        if (damage > 0)
        {
            _DigAccumulator -= damage;

            ItemManager receiver = enemy._Inventory; // �󂯎���i�G�C���x���g���j
            int dropped = _TargetCrystalMinerals.Mine(damage, receiver);

            if (dropped > 0)
            {
                enemy._WordCount += dropped; // ���^�p�J�E���g���X�V�iUI�ⓦ������j
                Debug.Log($"{LOG} ����̃h���b�v��={dropped} / �G�̏������[�h���v={enemy._WordCount}");
            }
        }

        // �擾���������l�ŒE�o��
        if (enemy.ShouldEscape)
        {
            Debug.Log($"{LOG} �������l���B�iWord={enemy._WordCount}, Part={enemy._PartCount}�j�� �E�o�֑J��");
            enemy.TransitionToState(enemy._EscapeState);
            return;
        }

        // �j��ς݂Ȃ�I��
        if (_TargetCrystalMinerals.GetCurrentHP() <= 0)
        {
            Debug.Log($"{LOG} �N���X�^���j�� �� ���낤��֑J��");
            enemy.TransitionToState(enemy._WanderState);
            return;
        }

        Debug.Log($"<color=#0000FF>Word={enemy._WordCount}, �N���X�^��HP={_TargetCrystalMinerals.GetCurrentHP()} </color>");
    }

    public void Exit(EnemyController enemy)
    {
        Debug.Log($"{LOG} Exit: �@����I�����܂�");
        _TargetCrystal = null;
        _TargetCrystalMinerals = null;
        _DigAccumulator = 0f;
    }

    public void SetTargetCrystal(Transform crystal)
    {
        _TargetCrystal = crystal;
        Debug.Log($"{LOG} SetTargetCrystal: {(_TargetCrystal ? _TargetCrystal.name : "NULL")}");
    }

    // ---- IEnemyState �K�{���\�b�h�i�K�v�Ȃ������ǉ��j----
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
        Debug.Log($"{LOG} OnBecameInvisible: �J�����O�ɂȂ�܂���");
    }
}
// 7. �E�o���
public class EscapeState : IEnemyState
{
    public void Enter(EnemyController enemy)
    {
        enemy.SetMoveSpeed(enemy._EscapeSpeedRatio); // �ړ��X�s�[�h���X�V
        Debug.Log("�E�o��ԊJ�n");
    }

    public void Update(EnemyController enemy)
    {
        // �E�o�|�C���g�Ɍ������Ĉړ��A�֐��̒��œ��B�������ԋ߂��E�o�|�C���g�̌v�Z�����Ă���
        enemy.MoveToEscapePoint();
    }

    public void Exit(EnemyController enemy)
    {
        // �N���[���A�b�v����
    }


    public void OnVisionTrigger(Collider other, EnemyController enemy)
    {
        // ���E�Ƀv���C���[���������瓦����ԂɈڍs
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