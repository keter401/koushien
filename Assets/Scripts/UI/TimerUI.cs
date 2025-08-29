using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private string timerFormat = "{0:00}";

    private TextMeshProUGUI timerText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        timerText.text = string.Format(timerFormat, GManager.instance.GetTimer());
    }
}
