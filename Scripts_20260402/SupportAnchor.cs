using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
public class SupportAnchor : MonoBehaviour
{
    [Header("扶住设置")]
    public bool isBeingHeld = false;
    [Range(0.01f, 0.5f)] public float allowedMoveRadius = 0.15f;

    private XRSimpleInteractable _simpleInteractable;
    private IXRHoverInteractor _currentHoverInteractor;
    private Vector3 _lockedPosition;
    private bool _isHovering = false;

    void Awake() => _simpleInteractable = GetComponent<XRSimpleInteractable>();

    void OnEnable()
    {
        _simpleInteractable.hoverEntered.AddListener(OnHoverEnter);
        _simpleInteractable.hoverExited.AddListener(OnHoverExit);
    }

    void OnDisable()
    {
        _simpleInteractable.hoverEntered.RemoveListener(OnHoverEnter);
        _simpleInteractable.hoverExited.RemoveListener(OnHoverExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        _isHovering = true;
        _currentHoverInteractor = args.interactorObject;
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        _isHovering = false;
        if (!isBeingHeld) _currentHoverInteractor = null;
    }

    void Update()
    {
        if (_currentHoverInteractor == null) return;

        XRNode node = GetNodeFromInteractor(_currentHoverInteractor);
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);

        device.TryGetFeatureValue(CommonUsages.trigger, out float triggerVal);
        device.TryGetFeatureValue(CommonUsages.grip, out float gripVal);
        bool bothPressed = triggerVal > 0.5f && gripVal > 0.5f;

        if (!_isHovering && !isBeingHeld) return;

        if (!isBeingHeld)
        {
            if (bothPressed) StartHolding(device);
        }
        else
        {
            float currentDist = Vector3.Distance(_currentHoverInteractor.transform.position, _lockedPosition);
            if (!bothPressed || currentDist > allowedMoveRadius) StopHolding();
        }
    }

    private void StartHolding(InputDevice device)
    {
        isBeingHeld = true;
        _lockedPosition = _currentHoverInteractor.transform.position;
        if (ElderlyModeManager.Instance != null) ElderlyModeManager.Instance.OnSupportAnchored();
        device.SendHapticImpulse(0U, 0.5f, 0.1f);
    }

    private void StopHolding()
    {
        isBeingHeld = false;
        if (ElderlyModeManager.Instance != null) ElderlyModeManager.Instance.OnSupportReleased();
        if (!_isHovering) _currentHoverInteractor = null;
    }

    private XRNode GetNodeFromInteractor(IXRHoverInteractor interactor)
    {
        if (ElderlyModeManager.Instance != null)
        {
            // 通过引用比对确认左右手
            if (interactor as Object == ElderlyModeManager.Instance.leftHandInteractor as Object)
                return XRNode.LeftHand;
        }
        return XRNode.RightHand;
    }
}