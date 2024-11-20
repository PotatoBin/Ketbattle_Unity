using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public SimilarityCalculator similarityCalculator;
    public MotionLoader motionLoader;

    public TextAsset motionDatabaseFile;

    private void Start()
    {
        motionLoader.LoadMotionDatabase(motionDatabaseFile);
    }

    public void OnPlayerAttack(string playerChat)
    {
        List<string> prompts = new List<string>(motionLoader.GetPrompts());

        StartCoroutine(similarityCalculator.GetMostSimilarMotion(playerChat, prompts, mostSimilarPrompt =>
        {
            motionLoader.PlayMotion(mostSimilarPrompt, true); // 플레이어 모션 실행
        }));
    }

    public void OnAIAttack(string aiChat)
    {
        List<string> prompts = new List<string>(motionLoader.GetPrompts());

        StartCoroutine(similarityCalculator.GetMostSimilarMotion(aiChat, prompts, mostSimilarPrompt =>
        {
            motionLoader.PlayMotion(mostSimilarPrompt, false); // AI 모션 실행
        }));
    }
}
