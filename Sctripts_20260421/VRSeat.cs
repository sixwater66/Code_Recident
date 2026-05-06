using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class VRSeat : MonoBehaviour
{
    [Header("玩家引用")]
    public XROrigin xrOrigin;

    [Header("位置锚点设置")]
    public Transform seatHeadAnchor;
    public Transform standAnchor;

    [Header("移动控制锁定")]
    public MonoBehaviour[] movementProvidersToDisable;

    private XRSimpleInteractable _interactable;
    private bool _isSitting = false;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        _interactable.selectEntered.AddListener(OnTogglePosture);
    }

    void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(OnTogglePosture);
    }

    private void OnTogglePosture(SelectEnterEventArgs args)
    {
        if (!_isSitting)
            SitDown();
        else
            StandUp();
    }

    private void SitDown()
    {
        _isSitting = true;

        foreach (var provider in movementProvidersToDisable)
        {
            if (provider != null) provider.enabled = false;
        }

        xrOrigin.MoveCameraToWorldLocation(seatHeadAnchor.position);
        xrOrigin.MatchOriginUpCameraForward(seatHeadAnchor.up, seatHeadAnchor.forward);
    }

    private void StandUp()
    {
        _isSitting = false;

        xrOrigin.transform.position = standAnchor.position;
        xrOrigin.transform.rotation = standAnchor.rotation;

        foreach (var provider in movementProvidersToDisable)
        {
            if (provider != null) provider.enabled = true;
        }
    }
}