using System;
using Unity.VisualScripting;
using UnityEngine;

public class GManager : MonoBehaviour
{
    public static GManager instance { get; private set; } // �V���O���g���C���X�^���X
    public static event Action OnTimerStart;    // �^�C�}�[�J�n�C�x���g
    public static event Action OnTimerEnd;      // �^�C�}�[�I���C�x���g


    [SerializeField] private WordDataBase wordDataBase = null; // �P��f�[�^�x�[�X

    [Header("���Ԑݒ�")]
    [SerializeField] private bool isTimerEnabled = false;   // �^�C�}�[��L���ɂ��邩
    [SerializeField] private bool isUnscaleTime = false;    // �^�C�}�[�̎��Ԃ��X�P�[�����Ȃ���
    [SerializeField] private float timer = 60.0f;           // �^�C�}�[�̒l

    public float maxActionTime = 60.0f;                     // �A�N�V�����p�[�g�̍ő厞��


    [SerializeField, Header("���݂̃Q�[�����")]
    private GameState currentGameState = new GameState();
    private GameState nextState = new ActionGameState(); 


    void Awake()
    {
        // �V���O���g���̏�����
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����ׂ��ŃI�u�W�F�N�g��ێ�


            // �^�C�}�[�̃��O�o��
            OnTimerStart += () => Debug.Log($"[GManager] �^�C�}�[���J�n����܂����B ({timer} �b)");
            OnTimerEnd += () => Debug.Log("[GManager] �^�C�}�[���I�����܂����B");
        } else {
            Destroy(gameObject); // ���ɑ��݂���ꍇ�͐V�����C���X�^���X��j��
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ��Ԃ��ς�����ꍇ�A���̃t���[���ŏ�Ԃ��X�V
        if (nextState != null) {
            currentGameState = nextState;
            nextState = null;

            currentGameState.Enter();
        }


        // �^�C�}�[����
        if (isTimerEnabled) {
            timer -= isUnscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;

            // �^�C�}�[��0�ȉ��ɂȂ����ꍇ�A�C�x���g�𔭉�
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
    /// ���݂̃Q�[���̏�Ԃ��擾���܂��B
    /// </summary>
    /// <returns>���݂̃Q�[���̏��</returns>
    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }


    /// <summary>
    /// �Q�[���̏�Ԃ�ݒ肵�܂��B
    /// </summary>
    /// <param name="newState">�Q�[���̏��</param>
    public void SetGameState(GameState newState)
    {
        if (currentGameState != newState) {
            nextState = newState;
        }
    }


    /// <summary>
    /// ���݂̃^�C�}�[�̒l���擾���܂��B
    /// </summary>
    /// <returns>���݂̃^�C�}�[�l</returns>
    public float GetTimer()
    {
        return timer;
    }


    /// <summary>
    /// �^�C�}�[���J�n���܂��B
    /// </summary>
    /// <param name="time">�ő厞��</param>
    /// <param name="unscaleTime">�X�P�[���^�C����L���ɂ��邩</param>
    public void StartTimer(float time, bool unscaleTime = false)
    {
        isTimerEnabled = true;
        isUnscaleTime = unscaleTime;
        timer = time;

        OnTimerStart?.Invoke();
    }


    /// <summary>
    ///  �^�C�}�[�����Z�b�g���܂��B
    /// </summary>
    public void ResetTimer()
    {
        isTimerEnabled = false;
        isUnscaleTime = false;
        timer = 60.0f;          // �f�t�H���g�̃^�C�}�[�l�Ƀ��Z�b�g
        OnTimerEnd?.Invoke();   // �^�C�}�[�I���C�x���g�𔭉�
    }
}
