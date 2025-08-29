using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadManager : MonoBehaviour
{
    public static GamepadManager instance { get; private set; }

    private float lowFrequencyVibration = 0.0f; // 低周波振動の強さ
    private float highFrequencyVibration = 0.0f; // 高周波振動の強さ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンを跨いでオブジェクトを保持
        } else {
            Destroy(gameObject); // 既に存在する場合は新しいインスタンスを破棄
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Debug.Log($"GamepadManager LateUpdate called, lowFrequencyVibration: {lowFrequencyVibration}, highFrequencyVibration: {highFrequencyVibration}");
        Gamepad.current?.SetMotorSpeeds(lowFrequencyVibration, highFrequencyVibration);

        lowFrequencyVibration = 0.0f; // 毎フレームリセット
        highFrequencyVibration = 0.0f; // 毎フレームリセット
    }


    /// <summary>
    /// ゲームパッドの振動を追加します。
    /// </summary>
    /// <param name="lowFrequency">低周波</param>
    /// <param name="highFrequency">高周波</param>
    public void AddVibration(float lowFrequency, float highFrequency)
    {
        lowFrequencyVibration += lowFrequency;
        highFrequencyVibration += highFrequency;

        lowFrequencyVibration = Mathf.Clamp(lowFrequencyVibration, 0.0f, 1.0f);
        highFrequencyVibration = Mathf.Clamp(highFrequencyVibration, 0.0f, 1.0f);
    }
}
