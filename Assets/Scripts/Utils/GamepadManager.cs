using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadManager : MonoBehaviour
{
    public static GamepadManager instance { get; private set; }

    private float lowFrequencyVibration = 0.0f; // ����g�U���̋���
    private float highFrequencyVibration = 0.0f; // �����g�U���̋���

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // �V�[�����ׂ��ŃI�u�W�F�N�g��ێ�
        } else {
            Destroy(gameObject); // ���ɑ��݂���ꍇ�͐V�����C���X�^���X��j��
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Debug.Log($"GamepadManager LateUpdate called, lowFrequencyVibration: {lowFrequencyVibration}, highFrequencyVibration: {highFrequencyVibration}");
        Gamepad.current?.SetMotorSpeeds(lowFrequencyVibration, highFrequencyVibration);

        lowFrequencyVibration = 0.0f; // ���t���[�����Z�b�g
        highFrequencyVibration = 0.0f; // ���t���[�����Z�b�g
    }


    /// <summary>
    /// �Q�[���p�b�h�̐U����ǉ����܂��B
    /// </summary>
    /// <param name="lowFrequency">����g</param>
    /// <param name="highFrequency">�����g</param>
    public void AddVibration(float lowFrequency, float highFrequency)
    {
        lowFrequencyVibration += lowFrequency;
        highFrequencyVibration += highFrequency;

        lowFrequencyVibration = Mathf.Clamp(lowFrequencyVibration, 0.0f, 1.0f);
        highFrequencyVibration = Mathf.Clamp(highFrequencyVibration, 0.0f, 1.0f);
    }
}
