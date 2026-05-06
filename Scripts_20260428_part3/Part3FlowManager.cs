using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum Part3State
{
    IntroDarkRoom,
    WaitingForLightSwitch,
    LightSwitchDialogue,
    FreeTidying,
    GlassesDropped,
    ElderVisionIntro,
    InspectHighlightedItems,
    RepositionItems,
    Ending,
    Finished
}

public class Part3FlowManager : MonoBehaviour
{
    [Header("UI")]
    public SubtitleUIManager subtitleUI;
    public GameObject instructionUI;
    public TextMeshProUGUI instructionText;

    [Header("Audio")]
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Lighting")]
    public Light directionalLight;
    public float darkIntensity = 0.05f;
    public float brightIntensity = 0.95f;

    [Header("Vision Effect")]
    [Tooltip("如果你用的是一个单独的眼疾特效物体，比如 Overlay/Volume，就拖这里。不要拖 Main Camera。")]
    public GameObject elderVisionEffectObject;

    [Tooltip("如果你的眼疾效果脚本挂在 Main Camera 上，就把 Main Camera 上的 EyeDiseaseController 拖这里。推荐使用这个。")]
    public EyeDiseaseController eyeDiseaseController;

    [Header("Light Switch")]
    public Animator lightSwitchAnimator;
    public string lightSwitchTurnOnTrigger = "TurnOn";
    public AudioClip switchSFX;

    [Header("Glasses")]
    public GameObject glassesObject;
    public Rigidbody glassesRigidbody;
    public Collider glassesCollider;

    [Header("Door")]
    public Animator doorAnimator;
    public string doorOpenTrigger = "Open";

    [Header("Flow Audio")]
    public AudioClip introDarkRoomClip;
    public string introDarkRoomSubtitle = "这房间怎么这么暗？这谁看得见啊？";

    public AudioClip lightBrokenClip;
    public string lightBrokenSubtitle = "这灯怎么打不开啊？还得我们来修。";

    public AudioClip tidyIntroClip;
    public string tidyIntroSubtitle = "这房间都挤成这样了还往里头塞东西，哪放得下啊？还是收拾收拾吧……";

    public AudioClip glassesDropClip;
    public string glassesDropSubtitle = "哪来的眼镜？捡起来先吧";

    public AudioClip elderVisionIntroClip;
    public string elderVisionIntroSubtitle = "这是什么？！怎么黑乎乎的？";

    public AudioClip inspectIntroClip;
    public string inspectIntroSubtitle = "这些东西怎么了？";

    public AudioClip endingClip;
    public string endingSubtitle = "（电话铃）妈？你们看完医生了吗？我来接你们，东西放着我们来收……";

    [Header("Required Items")]
    public List<Part3InteractableItem> requiredInspectItems = new List<Part3InteractableItem>();

    public Part3State CurrentState { get; private set; }

    private readonly HashSet<Part3InteractableItem> inspectedItems = new HashSet<Part3InteractableItem>();
    private readonly HashSet<Part3InteractableItem> correctlyPlacedItems = new HashSet<Part3InteractableItem>();

    // 记录自由整理阶段每个物品最后放在哪个 Zone
    private readonly Dictionary<Part3InteractableItem, PlacementZone> freePlacedZones =
        new Dictionary<Part3InteractableItem, PlacementZone>();

    private bool lightSwitchUsed = false;
    private bool noteUsed = false;
    private bool glassesUsed = false;
    private bool endingStarted = false;

    private void Start()
    {
        InitScene();
        StartCoroutine(IntroRoutine());
    }

    private void InitScene()
    {
        CurrentState = Part3State.IntroDarkRoom;

        SetDirectionalLight(false);
        SetVisionEffect(false);

        if (instructionUI != null)
            instructionUI.SetActive(false);

        if (glassesObject != null)
            glassesObject.SetActive(false);

        if (glassesRigidbody != null)
        {
            glassesRigidbody.isKinematic = true;
            glassesRigidbody.useGravity = true;
        }

        if (glassesCollider != null)
            glassesCollider.enabled = false;

        inspectedItems.Clear();
        correctlyPlacedItems.Clear();
        freePlacedZones.Clear();

        foreach (var item in requiredInspectItems)
        {
            if (item != null)
            {
                item.SetHighlight(false);
                item.HideClueImage();
                item.isCorrectlyPlaced = false;
            }
        }
    }

    private IEnumerator IntroRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        yield return PlayVoiceRoutine(introDarkRoomClip, introDarkRoomSubtitle, 3f);
        CurrentState = Part3State.WaitingForLightSwitch;
    }

    public void OnLightSwitchInteracted()
    {
        if (CurrentState != Part3State.WaitingForLightSwitch) return;
        if (lightSwitchUsed) return;

        lightSwitchUsed = true;
        StartCoroutine(LightSwitchRoutine());
    }

    private IEnumerator LightSwitchRoutine()
    {
        CurrentState = Part3State.LightSwitchDialogue;

        if (lightSwitchAnimator != null)
            lightSwitchAnimator.SetTrigger(lightSwitchTurnOnTrigger);

        PlaySFX(switchSFX);

        yield return PlayVoiceRoutine(lightBrokenClip, lightBrokenSubtitle, 3f);

        SetDirectionalLight(true);

        yield return new WaitForSeconds(5f);

        yield return PlayVoiceRoutine(tidyIntroClip, tidyIntroSubtitle, 4f);

        ShowInstruction("试着把这些东西放到你觉得合适的位置");

        CurrentState = Part3State.FreeTidying;
    }

    public void RecordFreePlacement(Part3InteractableItem item, PlacementZone zone)
    {
        if (item == null || zone == null) return;

        // 只记录关键物品。普通物品不用影响后续开门逻辑。
        if (requiredInspectItems.Contains(item))
        {
            freePlacedZones[item] = zone;
        }

        Debug.Log($"[自由整理] {item.displayName} 放在了 {zone.zoneName}。区域含义：{zone.zoneMeaningText}");
    }

    public void OnNoteInteracted()
    {
        if (CurrentState != Part3State.FreeTidying) return;
        if (noteUsed) return;

        noteUsed = true;
        StartCoroutine(NoteRoutine());
    }

    private IEnumerator NoteRoutine()
    {
        CurrentState = Part3State.GlassesDropped;

        HideInstruction();

        if (glassesObject != null)
            glassesObject.SetActive(true);

        if (glassesCollider != null)
            glassesCollider.enabled = true;

        if (glassesRigidbody != null)
        {
            glassesRigidbody.isKinematic = false;
            glassesRigidbody.useGravity = true;
        }

        yield return PlayVoiceRoutine(glassesDropClip, glassesDropSubtitle, 2f);
    }

    public void OnGlassesGrabbed()
    {
        if (CurrentState != Part3State.GlassesDropped) return;
        if (glassesUsed) return;

        glassesUsed = true;
        StartCoroutine(ElderVisionRoutine());
    }

    private IEnumerator ElderVisionRoutine()
    {
        CurrentState = Part3State.ElderVisionIntro;

        if (glassesObject != null)
            glassesObject.SetActive(false);

        SetVisionEffect(true);

        yield return PlayVoiceRoutine(elderVisionIntroClip, elderVisionIntroSubtitle, 3f);

        yield return new WaitForSeconds(5f);

        int wrongCount = EvaluateFreePlacementsAndHighlightWrongItems();

        // 如果玩家一开始全部都放对了，直接结束。虽然这概率不大，但代码不要像人类计划一样脆。
        if (wrongCount == 0)
        {
            HideInstruction();
            StartEnding();
            yield break;
        }

        yield return PlayVoiceRoutine(inspectIntroClip, inspectIntroSubtitle, 2f);

        ShowInstruction("拿起高亮的东西检查一下");

        // 直接进入重新摆放状态，不再要求玩家先把所有物品都查看一遍
        CurrentState = Part3State.RepositionItems;
    }

    private int EvaluateFreePlacementsAndHighlightWrongItems()
    {
        correctlyPlacedItems.Clear();

        int wrongCount = 0;

        foreach (var item in requiredInspectItems)
        {
            if (item == null) continue;

            item.SetHighlight(false);
            item.HideClueImage();
            item.isCorrectlyPlaced = false;

            bool hasRecordedZone = freePlacedZones.TryGetValue(item, out PlacementZone zone);
            bool isCorrect = hasRecordedZone &&
                             zone != null &&
                             item.correctZoneIDs.Contains(zone.zoneID);

            if (isCorrect)
            {
                correctlyPlacedItems.Add(item);
                item.isCorrectlyPlaced = true;
                item.SetHighlight(false);

                Debug.Log($"[初始整理正确] {item.displayName} 已经放在正确位置：{zone.zoneID}");
            }
            else
            {
                wrongCount++;
                item.SetHighlight(true);

                string currentZone = hasRecordedZone && zone != null ? zone.zoneID : "没有放入任何检测区";
                Debug.Log($"[初始整理错误] {item.displayName} 当前区域：{currentZone}，正确区域应为：{string.Join(",", item.correctZoneIDs)}");
            }
        }

        return wrongCount;
    }

    public void OnRequiredItemInspected(Part3InteractableItem item)
    {
        if (item == null) return;
        if (!IsInspectionActive()) return;

        if (item.grandmaVoiceClip != null)
            StartCoroutine(PlayVoiceRoutine(item.grandmaVoiceClip, item.grandmaSubtitle, 2f));

        inspectedItems.Add(item);
    }

    public void OnItemPlacedCorrectly(Part3InteractableItem item, PlacementZone zone)
    {
        if (item == null) return;

        if (CurrentState != Part3State.RepositionItems &&
            CurrentState != Part3State.InspectHighlightedItems)
            return;

        if (correctlyPlacedItems.Contains(item)) return;

        correctlyPlacedItems.Add(item);
        item.SetHighlight(false);
        item.HideClueImage();
        item.isCorrectlyPlaced = true;

        Debug.Log($"[重新放置正确] {item.displayName} 放到了 {zone.zoneName}");

        if (correctlyPlacedItems.Count >= requiredInspectItems.Count)
        {
            StartEnding();
        }
    }

    public void OnItemPlacedWrong(Part3InteractableItem item, PlacementZone zone)
    {
        if (item == null) return;

        if (CurrentState != Part3State.RepositionItems &&
            CurrentState != Part3State.InspectHighlightedItems)
            return;

        correctlyPlacedItems.Remove(item);
        item.isCorrectlyPlaced = false;
        item.SetHighlight(true);

        Debug.Log($"[重新放置错误] {item.displayName} 放到了 {zone.zoneName}，继续高亮。");
    }

    private void StartEnding()
    {
        if (endingStarted) return;
        endingStarted = true;

        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        CurrentState = Part3State.Ending;

        HideInstruction();

        foreach (var item in requiredInspectItems)
        {
            if (item != null)
            {
                item.SetHighlight(false);
                item.HideClueImage();
            }
        }

        if (doorAnimator != null)
            doorAnimator.SetTrigger(doorOpenTrigger);

        yield return new WaitForSeconds(1f);

        yield return PlayVoiceRoutine(endingClip, endingSubtitle, 4f);

        CurrentState = Part3State.Finished;
    }

    private IEnumerator PlayVoiceRoutine(AudioClip clip, string subtitle, float fallbackDuration)
    {
        float duration = clip != null ? clip.length : fallbackDuration;

        if (voiceAudioSource != null && clip != null)
        {
            voiceAudioSource.Stop();
            voiceAudioSource.clip = clip;
            voiceAudioSource.Play();
        }
        else if (clip != null && voiceAudioSource == null)
        {
            Debug.LogWarning("[Part3FlowManager] voiceAudioSource 没有绑定，所以音频无法播放。");
        }

        if (subtitleUI != null && !string.IsNullOrEmpty(subtitle))
        {
            subtitleUI.ShowSubtitle(subtitle, duration);
        }
        else
        {
            Debug.LogWarning("[Part3FlowManager] 字幕没有显示：subtitleUI 未绑定，或 subtitle 文本为空。");
        }

        yield return new WaitForSeconds(duration);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }

    public void ShowInstruction(string text)
    {
        if (instructionUI != null)
            instructionUI.SetActive(true);

        if (instructionText != null)
            instructionText.text = text;
    }

    public void HideInstruction()
    {
        if (instructionUI != null)
            instructionUI.SetActive(false);
    }

    public bool IsInspectionActive()
    {
        return CurrentState == Part3State.InspectHighlightedItems ||
               CurrentState == Part3State.RepositionItems;
    }

    private void SetVisionEffect(bool active)
    {
        if (eyeDiseaseController != null)
        {
            if (active)
                eyeDiseaseController.OnPickUpGlasses();
            else
                eyeDiseaseController.SetNormalVision();

            return;
        }

        if (elderVisionEffectObject != null)
            elderVisionEffectObject.SetActive(active);
    }

    private void SetDirectionalLight(bool bright)
    {
        if (directionalLight != null)
            directionalLight.intensity = bright ? brightIntensity : darkIntensity;
    }
}