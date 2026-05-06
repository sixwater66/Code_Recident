using UnityEngine;
using UnityEngine.XR;
using TMPro;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;

public class ElderlyModeManager : MonoBehaviour
{
    public static ElderlyModeManager Instance;

    [Header("老人模式核心开关")]
    public bool elderlyModeEnabled = false;
    public TextMeshProUGUI centerHintText;

    [Header("手动配置抖动点 (将新建的 HandAttach 拖到这里)")]
    public Transform leftHandAttach;
    public Transform rightHandAttach;

    [Header("手柄引用 (把 Left/Right Controller 拖到这里)")]
    public XRBaseInteractor leftHandInteractor;
    public XRBaseInteractor rightHandInteractor;

    [Header("参数设置")]
    [Range(0.01f, 0.2f)] public float handShakeIntensity = 0.02f;
    [Range(5f, 40f)] public float objectShakeFrequency = 20f;
    [Range(0.005f, 0.05f)] public float objectShakeAmount = 0.01f;
    public float maxHoldTime = 3f;

    private bool _isAnchored = false;
    private float _leftHoldTimer = 0f;
    private float _rightHoldTimer = 0f;

    void Awake() { if (Instance == null) Instance = this; }

    void Update()
    {
        if (elderlyModeEnabled) HandleTremorAndDrop();
    }

    public void OnSupportAnchored() => _isAnchored = true;
    public void OnSupportReleased() => _isAnchored = false;

    private void HandleTremorAndDrop()
    {
        ProcessHand(leftHandInteractor, leftHandAttach, XRNode.LeftHand, ref _leftHoldTimer);
        ProcessHand(rightHandInteractor, rightHandAttach, XRNode.RightHand, ref _rightHoldTimer);
    }

    private void ProcessHand(XRBaseInteractor interactor, Transform attach, XRNode node, ref float timer)
    {
        if (interactor == null) return;

        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        device.SendHapticImpulse(0U, handShakeIntensity, 0.1f);

        if (interactor.interactablesSelected.Count > 0)
        {
            if (_isAnchored)
            {
                timer = 0f;
                if (attach != null) attach.localPosition = Vector3.zero;
                return;
            }

            timer += Time.deltaTime;
            // 直接在这里修改我们手动指定的 Attach 点
            if (attach != null)
            {
                attach.localPosition = new Vector3(
                    Mathf.Sin(Time.time * objectShakeFrequency) * objectShakeAmount,
                    Mathf.Cos(Time.time * objectShakeFrequency * 1.1f) * objectShakeAmount,
                    0
                );
            }

            if (timer >= maxHoldTime)
            {
                timer = 0f;
                if (attach != null) attach.localPosition = Vector3.zero;
                interactor.interactionManager.CancelInteractorSelection((IXRSelectInteractor)interactor);
            }
        }
        else
        {
            timer = 0f;
            if (attach != null) attach.localPosition = Vector3.zero;
        }
    }
}