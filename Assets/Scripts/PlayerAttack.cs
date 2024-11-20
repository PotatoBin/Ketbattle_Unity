using System.IO;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameManager gameManager;
    private string chatLogFilePath;

    private void Start()
    {
        chatLogFilePath = Path.Combine(Application.persistentDataPath, "chatLog.txt");
        InitializeChatLog();
    }

    public void Attack(string playerChat)
    {
        if (string.IsNullOrEmpty(playerChat)) return;

        AppendChatLog($"Player: {playerChat}");
        gameManager.OnPlayerAttack(playerChat); // GameManager에 플레이어 공격 전달
    }

    private void InitializeChatLog()
    {
        if (!File.Exists(chatLogFilePath))
        {
            File.WriteAllText(chatLogFilePath, ""); // 빈 파일 생성
        }
    }

    private void AppendChatLog(string logEntry)
    {
        File.AppendAllText(chatLogFilePath, logEntry + "\n");
    }
}
