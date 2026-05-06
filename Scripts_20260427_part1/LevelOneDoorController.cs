using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;

[RequireComponent(typeof(XRSimpleInteractable))]
public class LevelOneDoorController : MonoBehaviour
{
    [Header("场景跳转设置")]
    public string nextSceneName = "Scene2";

    [Header("流程锁状态（需双重达成）")]
    public bool introSequenceFinished = false;   // 由 IntroSequenceManager 调用
    public bool packagePlaced = false;           // 由 PlacementZoneValidator 调用

    [Header("门把手设置")]
    [Tooltip("按下动画播放完后等待多少秒再跳转")]
    public float delayBeforeTransition = 0.8f;
    public AudioSource doorClickSound;

    private XRSimpleInteractable _interactable;
    private Animator _animator;
    private bool _isOpening = false;

    // 只有两个条件都满足才算真正解锁
    private bool IsUnlocked => introSequenceFinished && packagePlaced;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        _interactable.selectEntered.AddListener(OnGrabDoorHandle);
    }

    void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(OnGrabDoorHandle);
    }

    // ====================== 供外部调用的方法 ======================
    public void SetIntroFinished()
    {
        introSequenceFinished = true;
        CheckUnlockStatus();
    }

    public void SetPackagePlaced()
    {
        packagePlaced = true;
        CheckUnlockStatus();
    }

    public void SetPackageRemoved()
    {
        packagePlaced = false;
    }

    private void CheckUnlockStatus()
    {
        if (IsUnlocked)
        {
            Debug.Log("【逻辑】剧情和快递均已就绪，门把手现在可以操作了！");
            // 这里可以加视觉反馈：比如门把手发光、粒子、文字提示等
        }
    }

    // ====================== 交互逻辑 ======================
    private void OnGrabDoorHandle(SelectEnterEventArgs args)
    {
        if (!IsUnlocked || _isOpening)
        {
            Debug.Log("门还锁着呢！请检查：剧情是否听完？快递是否放好？");
            // 可选：给玩家手柄震动提示失败
            if (args.interactorObject is XRBaseInputInteractor inputInteractor)
            {
                inputInteractor.SendHapticImpulse(0.2f, 0.1f); // 轻微震动表示失败
            }
            return;
        }

        _isOpening = true;

        // 成功操作
        if (args.interactorObject is XRBaseInputInteractor inputInteractorSuccess)
        {
            inputInteractorSuccess.SendHapticImpulse(0.6f, 0.25f);
        }

        if (doorClickSound != null) doorClickSound.Play();

        if (_animator != null)
        {
            _animator.SetTrigger("OpenTrigger");
        }

        StartCoroutine(WaitAndTransition());
    }

    private IEnumerator WaitAndTransition()
    {
        yield return new WaitForSeconds(delayBeforeTransition);
        SceneManager.LoadScene(nextSceneName);
    }
}