using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRSimpleInteractable))]
public class DoorTransition : MonoBehaviour
{
    [Header("场景跳转设置")]
    public string nextSceneName = "Scene2";

    [Header("流程控制")]
    [Tooltip("如果为 true，玩家按扳机也无法触发开门")]
    public bool isLocked = true;

    [Header("门把手动画设置")]
    [Tooltip("按下动画播放完后，等待多少秒再跳转场景")]
    public float delayBeforeTransition = 0.5f;

    [Header("可选：音效")]
    public AudioSource doorClickSound;

    private XRSimpleInteractable _interactable;
    private Animator _animator;
    private bool _isOpening = false;

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

    private void OnGrabDoorHandle(SelectEnterEventArgs args)
    {
        // 如果还没听完音频（被锁住）或者已经在开门了，直接返回
        if (isLocked || _isOpening) return;

        _isOpening = true;

        // 手柄震动 (XRI 3.0+)
        if (args.interactorObject is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(0.5f, 0.2f);
        }

        // 播放音效
        if (doorClickSound != null) doorClickSound.Play();

        // 触发你设置好的 Animator 参数
        if (_animator != null)
            _animator.SetTrigger("OpenTrigger");

        StartCoroutine(WaitAndTransition());
    }

    private IEnumerator WaitAndTransition()
    {
        yield return new WaitForSeconds(delayBeforeTransition);
        SceneManager.LoadScene(nextSceneName);
    }
}