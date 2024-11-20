using System.Collections.Generic;
using UnityEngine;

public class MotionLoader : MonoBehaviour
{
    public Animator playerAnimator;
    public Animator aiAnimator;

    private Dictionary<string, string> motionDatabase = new Dictionary<string, string>();

    // CSV 데이터 로드
    public void LoadMotionDatabase(TextAsset csvFile)
    {
        string[] lines = csvFile.text.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            string prompt = parts[0].Trim();
            string fbxFileName = parts[1].Trim();

            motionDatabase[prompt] = fbxFileName;
        }

        Debug.Log("Motion database loaded.");
    }

    // 모션 실행
    public void PlayMotion(string prompt, bool isPlayer)
    {
        if (motionDatabase.TryGetValue(prompt, out string fbxFileName))
        {
            var animator = isPlayer ? playerAnimator : aiAnimator;

            AnimationClip clip = Resources.Load<AnimationClip>($"Animations/{fbxFileName}");
            if (clip != null)
            {
                animator.Play(clip.name);
            }
            else
            {
                Debug.LogWarning($"Animation clip {fbxFileName} not found.");
            }
        }
        else
        {
            Debug.LogWarning($"Prompt {prompt} not found in motion database.");
        }
    }

    public IEnumerable<string> GetPrompts()
    {
        return motionDatabase.Keys;
    }
}