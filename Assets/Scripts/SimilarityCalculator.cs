using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Linq;

public class SimilarityCalculator : MonoBehaviour
{
    public string apiUrl = "https://api.openai.com/v1/embeddings";
    public string apiKey = "your-api-key-here";

    public IEnumerator GetMostSimilarMotion(string chatInput, List<string> prompts, System.Action<string> onComplete)
    {
        // 1. 채팅 입력 임베딩 가져오기
        Vector3 chatEmbedding = Vector3.zero;
        yield return StartCoroutine(GetEmbedding(chatInput, embedding => chatEmbedding = embedding));

        // 2. 데이터베이스 프롬프트들 임베딩 가져오기
        List<(string prompt, Vector3 embedding)> promptEmbeddings = new List<(string, Vector3)>();

        foreach (var prompt in prompts)
        {
            Vector3 promptEmbedding = Vector3.zero;
            yield return StartCoroutine(GetEmbedding(prompt, embedding => promptEmbedding = embedding));
            promptEmbeddings.Add((prompt, promptEmbedding));
        }

        // 3. 유사도 계산
        string mostSimilarPrompt = "";
        float highestSimilarity = -1f;

        foreach (var (prompt, embedding) in promptEmbeddings)
        {
            float similarity = CosineSimilarity(chatEmbedding, embedding);
            if (similarity > highestSimilarity)
            {
                highestSimilarity = similarity;
                mostSimilarPrompt = prompt;
            }
        }

        onComplete?.Invoke(mostSimilarPrompt);
    }

    private IEnumerator GetEmbedding(string text, System.Action<Vector3> onComplete)
    {
        var payload = new
        {
            input = text,
            model = "text-embedding-ada-002"
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
            var response = JsonUtility.FromJson<EmbeddingResponse>(request.downloadHandler.text);
            Vector3 embedding = new Vector3(
                response.data[0].embedding[0],
                response.data[0].embedding[1],
                response.data[0].embedding[2]
            );
            onComplete?.Invoke(embedding);
        }
        else
        {
            Debug.LogError($"Error in embedding request: {request.error}");
            onComplete?.Invoke(Vector3.zero);
        }
    }

    private float CosineSimilarity(Vector3 v1, Vector3 v2)
    {
        return Vector3.Dot(v1, v2) / (v1.magnitude * v2.magnitude);
    }

    [System.Serializable]
    private class EmbeddingResponse
    {
        public Data[] data;

        [System.Serializable]
        public class Data
        {
            public float[] embedding;
        }
    }
}
