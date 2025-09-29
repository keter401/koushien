using UnityEngine;

public class PerlinShake : MonoBehaviour
{
    [Header("Shake Axes")]
    public bool shakeX = true;
    public bool shakeY = false;
    public bool shakeZ = false;

    [Header("Strength Settings")]
    public float strengthX = 0.5f;
    public float strengthY = 0.5f;
    public float strengthZ = 0.5f;

    [Header("Speed Settings")]
    public float frequency = 2.0f; // �m�C�Y�̐i�ޑ���

    private Vector3 originalPos;
    private float shakeTime = 0f;
    private float currentTime = 0f;
    private bool isShaking = false;
    private bool infiniteShake = false;

    private float offsetX, offsetY, offsetZ;
    private Vector3 customStrength;

    void Start()
    {
        originalPos = transform.localPosition;

        // �m�C�Y�̃I�t�Z�b�g�������_���ɏ�����
        offsetX = Random.Range(0f, 100f);
        offsetY = Random.Range(0f, 100f);
        offsetZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!isShaking) return;

        currentTime += Time.deltaTime;

        float t = Time.time * frequency;

        float x = shakeX ? (Mathf.PerlinNoise(t + offsetX, 0f) - 0.5f) * 2f * customStrength.x : 0f;
        float y = shakeY ? (Mathf.PerlinNoise(t + offsetY, 0f) - 0.5f) * 2f * customStrength.y : 0f;
        float z = shakeZ ? (Mathf.PerlinNoise(t + offsetZ, 0f) - 0.5f) * 2f * customStrength.z : 0f;

        transform.localPosition = originalPos + new Vector3(x, y, z);

        // ���Ԑ�������̏ꍇ
        if (!infiniteShake && currentTime >= shakeTime) {
            StopSmooth();
        }
    }

    // Inspector �ݒ�Ŏ��Ԏw�� Shake
    public void ShakeStart(float time)
    {
        ShakeStart(time, new Vector3(strengthX, strengthY, strengthZ));
    }

    // Inspector �ݒ�㏑���Ŏ��Ԏw�� Shake
    public void ShakeStart(float time, Vector3 strength)
    {
        shakeTime = time;
        currentTime = 0f;
        infiniteShake = false;
        customStrength = strength;
        isShaking = true;
    }

    // Inspector �ݒ�Ŗ���Shake
    public void ShakeStart()
    {
        ShakeStart(new Vector3(strengthX, strengthY, strengthZ));
    }

    // Inspector �ݒ�㏑���Ŗ���Shake
    public void ShakeStart(Vector3 strength)
    {
        infiniteShake = true;
        customStrength = strength;
        isShaking = true;
    }

    // �����ɒ�~���Č��ɖ߂�
    public void Stop()
    {
        isShaking = false;
        transform.localPosition = originalPos;
    }

    // ���X�Ɍ��̈ʒu�ɖ߂�
    private void StopSmooth()
    {
        isShaking = false;
        StartCoroutine(ReturnToOrigin());
    }

    private System.Collections.IEnumerator ReturnToOrigin()
    {
        Vector3 start = transform.localPosition;
        float elapsed = 0f;
        float duration = 0.5f; // �߂�����

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(start, originalPos, elapsed / duration);
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}