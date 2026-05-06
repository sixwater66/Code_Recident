using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class PackagePlacementValidator : MonoBehaviour
{
    [Header("验证设置")]
    [Tooltip("期望放入该区域的物体标签 (例如: Package 或 Medicine)")]
    public string expectedTag;

    [Header("事件")]
    public UnityEvent onCorrectItemPlaced;
    public UnityEvent onWrongItemPlaced;
    public UnityEvent onItemRemoved;

    [Header("门把手引用（推荐拖入）")]
    [Tooltip("拖入门把手物体，可提升性能，避免每次查找")]
    public LevelOneDoorController doorController;

    private XRSocketInteractor _socket;
    public bool IsCorrectlyPlaced { get; private set; } = false;

    void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
    }

    void OnEnable()
    {
        _socket.selectEntered.AddListener(OnItemPlaced);
        _socket.selectExited.AddListener(OnItemRemoved);
    }

    void OnDisable()
    {
        _socket.selectEntered.RemoveListener(OnItemPlaced);
        _socket.selectExited.RemoveListener(OnItemRemoved);
    }

    private void OnItemPlaced(SelectEnterEventArgs args)
    {
        GameObject placedObject = args.interactableObject.transform.gameObject;

        if (placedObject.CompareTag(expectedTag))
        {
            Debug.Log($"[{gameObject.name}] 放置正确: {placedObject.name}");
            IsCorrectlyPlaced = true;
            onCorrectItemPlaced.Invoke();

            // 通知门把手：快递已正确放置
            if (doorController != null)
            {
                doorController.SetPackagePlaced();
            }
            else
            {
                // 备用方案：使用新的非过时 API（更快）
                LevelOneDoorController door = FindAnyObjectByType<LevelOneDoorController>();
                if (door != null) door.SetPackagePlaced();
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 放置错误: {placedObject.name}");
            IsCorrectlyPlaced = false;
            onWrongItemPlaced.Invoke();
        }
    }

    private void OnItemRemoved(SelectExitEventArgs args)
    {
        if (IsCorrectlyPlaced)
        {
            IsCorrectlyPlaced = false;
            onItemRemoved.Invoke();

            // 通知门把手：快递被拿走
            if (doorController != null)
            {
                doorController.SetPackageRemoved();
            }
            else
            {
                // 备用方案：使用新的非过时 API
                LevelOneDoorController door = FindAnyObjectByType<LevelOneDoorController>();
                if (door != null) door.SetPackageRemoved();
            }
        }
    }
}