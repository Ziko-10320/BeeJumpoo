using UnityEngine;
using TMPro;

public class BossTimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public BossRoomManager bossManager;

    void Update()
    {
        if (!bossManager.bossActive)
        {
            timerText.text = "";
            return;
        }

        float remaining = bossManager.totalTime - bossManager.timer;
        remaining = Mathf.Max(0f, remaining);

        int seconds = Mathf.CeilToInt(remaining);
        timerText.text = seconds.ToString();
    }
}