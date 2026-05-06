using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class IntroSequenceManager : MonoBehaviour
{
    [Header("音频设置")]
    public AudioSource introAudioSource;

    [Header("UI 引用")]
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI instructionText;

    [Header("交互控制")]
    [Tooltip("拖入房门把手物体")]
    public XRSimpleInteractable doorHandle;

    [Header("字幕内容设置")]
    public string[] subtitleLines;
    public float[] subtitleTimings;

    private DoorTransition _doorScript;

    void Start()
    {
        subtitleText.text = "";
        instructionText.gameObject.SetActive(false);

        if (doorHandle != null)
        {
            _doorScript = doorHandle.GetComponent<DoorTransition>();
            // 初始锁定门把手
            if (_doorScript != null) _doorScript.isLocked = true;
            doorHandle.enabled = true;
        }

        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        // 1. 等待 1 秒黑屏适应
        yield return new WaitForSeconds(1f);

        if (introAudioSource != null && introAudioSource.clip != null)
        {
            introAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("未检测到音频剪辑，流程将直接继续。");
        }

        int currentLine = 0;

        // 2. 字幕播放循环
        // 修改逻辑：只要字幕还没放完，就继续循环
        while (currentLine < subtitleLines.Length)
        {
            if (introAudioSource != null && introAudioSource.clip != null)
            {
                // 基于音频实际播放时间来判断
                if (introAudioSource.time >= subtitleTimings[currentLine])
                {
                    subtitleText.text = subtitleLines[currentLine];
                    currentLine++;
                }
            }
            else
            {
                // 如果没有音频，直接按数组显示（保险兜底）
                subtitleText.text = subtitleLines[currentLine];
                currentLine++;
                yield return new WaitForSeconds(3f); // 每句停3秒
            }
            yield return null;
        }

        // 3. 【关键修改】字幕全部显示完后，额外等待 1.5 秒让玩家读完最后一句
        yield return new WaitForSeconds(1.5f);

        // 4. --- 强制解锁流程 ---
        // 不再死等 introAudioSource.isPlaying，直接执行后续操作
        subtitleText.text = "";

        instructionText.text = "请移动左手摇杆走到房门Please move the left joystick to the door\n按下任意背键扳机可以抓取物体Pressing any of the back key triggers will enable you to pick up objects\n按下门把手进入家门Press the door handle to enter the house.";
        instructionText.gameObject.SetActive(true);

        if (_doorScript != null)
        {
            _doorScript.isLocked = false; // 解开门把手逻辑锁
            Debug.Log("开场流程结束，门把手已解锁！");
        }
    }
}