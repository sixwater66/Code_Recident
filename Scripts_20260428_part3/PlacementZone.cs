using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlacementZone : MonoBehaviour
{
    [Header("Zone Basic")]
    public string zoneID;
    public string zoneName;
    public Transform snapPoint;

    [Header("Accepted Items")]
    public bool allowAnyItemInFreeTidying = true;
    public List<string> acceptedItemIDs = new List<string>();

    [Header("Zone Meaning")]
    [TextArea] public string zoneMeaningText;
    public bool isElderFriendlyZone;
    public bool isYoungTidyZone;
    public bool isDangerousZone;

    [Header("Audio")]
    public AudioClip placeSFX;
    public AudioClip wrongPlaceSFX;

    [Header("Free Tidying")]
    public bool snapDuringFreeTidying = true;
    public bool playSFXDuringFreeTidying = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Part3InteractableItem item = other.GetComponentInParent<Part3InteractableItem>();

        if (item != null)
            item.currentHoveredZone = this;
    }

    private void OnTriggerExit(Collider other)
    {
        Part3InteractableItem item = other.GetComponentInParent<Part3InteractableItem>();

        if (item != null && item.currentHoveredZone == this)
            item.currentHoveredZone = null;
    }

    public void TryPlaceItem(Part3InteractableItem item)
    {
        if (item == null || item.flowManager == null) return;

        Part3State state = item.flowManager.CurrentState;

        Debug.Log($"[尝试放置] 物品={item.displayName}, 当前Zone={zoneID}, 当前状态={state}, 正确Zone={string.Join(",", item.correctZoneIDs)}");

        if (state == Part3State.FreeTidying)
        {
            if (!CanAccept(item)) return;

            if (snapDuringFreeTidying)
                SnapItem(item);

            item.flowManager.RecordFreePlacement(item, this);

            if (playSFXDuringFreeTidying)
                item.flowManager.PlaySFX(placeSFX);

            return;
        }

        if (state == Part3State.InspectHighlightedItems || state == Part3State.RepositionItems)
        {
            bool isCorrect = item.correctZoneIDs.Contains(zoneID);

            if (isCorrect)
            {
                SnapItem(item);
                item.flowManager.PlaySFX(placeSFX);
                item.flowManager.OnItemPlacedCorrectly(item, this);
            }
            else
            {
                item.flowManager.PlaySFX(wrongPlaceSFX);
                item.flowManager.OnItemPlacedWrong(item, this);

                Debug.Log($"[放置错误] {item.displayName} 放到了 {zoneName}，但正确区域应为：{string.Join(",", item.correctZoneIDs)}");
            }
        }
    }

    private bool CanAccept(Part3InteractableItem item)
    {
        if (allowAnyItemInFreeTidying) return true;
        if (acceptedItemIDs == null || acceptedItemIDs.Count == 0) return true;

        return acceptedItemIDs.Contains(item.itemID);
    }

    private void SnapItem(Part3InteractableItem item)
    {
        if (snapPoint == null)
        {
            Debug.LogWarning($"[PlacementZone] {zoneName} 没有设置 SnapPoint。");
            return;
        }

        Rigidbody rb = item.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        item.transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
    }
}