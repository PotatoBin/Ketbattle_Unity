using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class AIResponse : MonoBehaviour
{
    public string apiUrl = "https://api.openai.com/v1/completions";
    public string apiKey = "your-api-key-here";
    public GameManager gameManager;

    private string chatLogFilePath;

    private void Start()
    {
        chatLogFilePath = Path.Combine(Application.persistentDataPath, "chatLog.txt");
    }

    public void Attack(string chatLog)
    {
        if (string.IsNullOrEmpty(chatLog)) return;

        StartCoroutine(GenerateAIResponse(chatLog));
    }

    private IEnumerator GenerateAIResponse(string chatLog)
    {
        var payload = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "You are an AI in a chat-based fighting game. Respond aggressively to the player's messages based on the context of the chat log." },
                new { role = "user", content = chatLog }
            },
            max_tokens = 50,
            temperature = 0.7
        };

        string jsonPayload = JsonUtility.ToJson(payload);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            string aiResponse = ParseResponse(responseJson);

            if (!string.IsNullOrEmpty(aiResponse))
            {
                AppendChatLog($"AI: {aiResponse}");
                gameManager.OnAIAttack(aiResponse); // GameManager에 AI의 채팅 전달
            }
        }
        else
        {
            Debug.LogError($"GPT API error: {request.error}");
        }
    }

    private string ParseResponse(string jsonResponse)
    {
        try
        {
            var responseObject = JsonUtility.FromJson<GPTResponse>(jsonResponse);
            return responseObject.choices[0].message.content.Trim();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse GPT response: {e.Message}");
            return null;
        }
    }

    private void AppendChatLog(string logEntry)
    {
        File.AppendAllText(chatLogFilePath, logEntry + "\n");
    }

    [System.Serializable]
    private class GPTResponse
    {
        public Choice[] choices;

        [System.Serializable]
        public class Choice
        {
            public Message message;

            [System.Serializable]
            public class Message
            {
                public string content;
            }
        }
    }
}
