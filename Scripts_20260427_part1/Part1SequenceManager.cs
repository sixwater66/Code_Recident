using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Part1SequenceManager : MonoBehaviour
{
    [Header("音频设置")]
    public AudioSource introAudioSource;

    [Header("门把手引用（重要！）")]
    [Tooltip("拖入场景中的门把手物体")]
    public XRSimpleInteractable doorHandle;

    [Header("字幕 UI")]
    [Tooltip("拖入用于显示字幕的 TextMeshProUGUI")]
    public TextMeshProUGUI subtitleText;

    [Header("字幕内容设置")]
    public string[] subtitleLines;
    public float[] subtitleTimings;

    private LevelOneDoorController _doorController;

    void Start()
    {
        // 清空字幕
        if (subtitleText != null)
            subtitleText.text = "";

        // 确保初始为锁定状态
        if (doorHandle != null)
        {
            _doorController = doorHandle.GetComponent<LevelOneDoorController>();
            if (_doorController != null)
            {
                _doorController.introSequenceFinished = false;
            }
        }

        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // 等待场景稍微稳定
        yield return new WaitForSeconds(1f);

        // 播放开场音频
        if (introAudioSource != null && introAudioSource.clip != null)
        {
            introAudioSource.Play();
            Debug.Log("Part1SequenceManager：开始播放开场音频");
        }
        else
        {
            Debug.LogWarning("未找到开场音频，将在2秒后直接解锁");
            yield return new WaitForSeconds(2f);
            UnlockIntroFinished();
            yield break;
        }

        int currentLine = 0;

        // 播放字幕逻辑
        while (currentLine < subtitleLines.Length)
        {
            if (introAudioSource.isPlaying)
            {
                if (introAudioSource.time >= subtitleTimings[currentLine])
                {
                    if (subtitleText != null)
                    {
                        subtitleText.text = subtitleLines[currentLine];
                    }
                    currentLine++;
                }
            }
            else
            {
                break; // 音频播完，跳出
            }
            yield return null;
        }

        // 等待音频完全结束
        yield return new WaitWhile(() => introAudioSource.isPlaying);

        // 额外等待，让玩家读完最后一句
        yield return new WaitForSeconds(1.5f);

        // 清空字幕
        if (subtitleText != null)
            subtitleText.text = "";

        // === 关键：解锁门把手第一条件 ===
        UnlockIntroFinished();
    }

    private void UnlockIntroFinished()
    {
        if (_doorController != null)
        {
            _doorController.SetIntroFinished();
            Debug.Log("Part1SequenceManager：开场音频 + 字幕流程完成，已解锁门把手第一条件！");
        }
        else
        {
            Debug.LogWarning("未找到 LevelOneDoorController，无法解锁门把手！");
        }
    }
}