using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public enum ItemType
{
    NormalProp,
    LightSwitch,
    Note,
    Glasses,
    RequiredInspectProp
}

public class Part3InteractableItem : MonoBehaviour
{
    [Header("Basic")]
    public string itemID;
    public string displayName;
    public ItemType itemType;
    public Part3FlowManager flowManager;

    [Header("Correct Placement")]
    public List<string> correctZoneIDs = new List<string>();

    [Header("Inspect Settings")]
    public GameObject highlightObject;
    public GameObject clueImageUI;
    public AudioClip grandmaVoiceClip;
    [TextArea] public string grandmaSubtitle;

    [Header("Audio")]
    public AudioClip grabSFX;
    public AudioClip releaseSFX;
    public AudioClip collisionSFX;
    public float collisionSFXMinVelocity = 0.4f;

    [HideInInspector] public PlacementZone currentHoveredZone;
    [HideInInspector] public bool hasBeenInspected = false;
    [HideInInspector] public bool isCorrectlyPlaced = false;

    private XRBaseInteractable interactable;
    private Rigidbody rb;
    private bool specialTriggered = false;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        rb = GetComponent<Rigidbody>();

        SetHighlight(false);
        HideClueImage();
    }

    private void OnEnable()
    {
        if (interactable == null) return;

        interactable.selectEntered.AddListener(OnGrabbed);
        interactable.selectExited.AddListener(OnReleased);
        interactable.activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        if (interactable == null) return;

        interactable.selectEntered.RemoveListener(OnGrabbed);
        interactable.selectExited.RemoveListener(OnReleased);
        interactable.activated.RemoveListener(OnActivated);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (flowManager == null) return;

        if (rb != null)
            rb.isKinematic = false;

        flowManager.PlaySFX(grabSFX);

        switch (itemType)
        {
            case ItemType.LightSwitch:
                TriggerLightSwitchOnce();
                break;

            case ItemType.Note:
                TriggerNoteOnce();
                break;

            case ItemType.Glasses:
                TriggerGlassesOnce();
                break;

            case ItemType.RequiredInspectProp:
                TryInspectRequiredItem();
                break;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (flowManager == null) return;

        flowManager.PlaySFX(releaseSFX);

        if (itemType == ItemType.RequiredInspectProp)
            HideClueImage();

        if (currentHoveredZone != null &&
            (itemType == ItemType.NormalProp || itemType == ItemType.RequiredInspectProp))
        {
            currentHoveredZone.TryPlaceItem(this);
        }
    }

    private void OnActivated(ActivateEventArgs args)
    {
        if (flowManager == null) return;

        if (itemType == ItemType.LightSwitch)
            TriggerLightSwitchOnce();
        else if (itemType == ItemType.Note)
            TriggerNoteOnce();
    }

    private void TriggerLightSwitchOnce()
    {
        if (specialTriggered) return;
        if (flowManager.CurrentState != Part3State.WaitingForLightSwitch) return;

        specialTriggered = true;
        flowManager.OnLightSwitchInteracted();

        StartCoroutine(DisableInteractionNextFrame());
    }

    private void TriggerNoteOnce()
    {
        if (specialTriggered) return;
        if (flowManager.CurrentState != Part3State.FreeTidying) return;

        specialTriggered = true;
        flowManager.OnNoteInteracted();

        StartCoroutine(DisableInteractionNextFrame());
    }

    private void TriggerGlassesOnce()
    {
        if (specialTriggered) return;
        if (flowManager.CurrentState != Part3State.GlassesDropped) return;

        specialTriggered = true;
        flowManager.OnGlassesGrabbed();
    }

    private void TryInspectRequiredItem()
    {
        if (!flowManager.IsInspectionActive()) return;

        ShowClueImage();
        hasBeenInspected = true;
        flowManager.OnRequiredItemInspected(this);
    }

    private IEnumerator DisableInteractionNextFrame()
    {
        yield return null;

        if (interactable != null)
            interactable.enabled = false;
    }

    public void SetHighlight(bool enable)
    {
        if (highlightObject != null)
            highlightObject.SetActive(enable);
    }

    public void ShowClueImage()
    {
        if (clueImageUI != null)
            clueImageUI.SetActive(true);
    }

    public void HideClueImage()
    {
        if (clueImageUI != null)
            clueImageUI.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionSFX == null || flowManager == null) return;

        if (collision.relativeVelocity.magnitude >= collisionSFXMinVelocity)
            flowManager.PlaySFX(collisionSFX);
    }
}