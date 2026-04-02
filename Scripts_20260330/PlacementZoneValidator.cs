using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class PlacementZoneValidator : MonoBehaviour
{
    [Header("验证设置 (Validation Settings)")]
    [Tooltip("期望放入该区域的物体标签 (例如: Medicine)")]
    public string expectedTag;

    [Header("事件 (Events)")]
    public UnityEvent onCorrectItemPlaced;
    public UnityEvent onWrongItemPlaced;
    public UnityEvent onItemRemoved;

    private XRSocketInteractor _socket;
    public bool IsCorrectlyPlaced { get; private set; } = false;

    void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
    }

    void OnEnable()
    {
        // 监听物体放入和拿出 Socket 的事件
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
        // 获取放入的物体
        GameObject placedObject = args.interactableObject.transform.gameObject;

        // 检查标签是否匹配
        if (placedObject.CompareTag(expectedTag))
        {
            Debug.Log($"[{gameObject.name}] 放置正确: {placedObject.name}");
            IsCorrectlyPlaced = true;
            onCorrectItemPlaced.Invoke(); // 触发正确事件 (可用于通知 PuzzleManager)

            // 可选：放置正确后锁定物体，不让玩家再拿走
            // _socket.socketActive = false; 
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 放置错误: {placedObject.name}");
            IsCorrectlyPlaced = false;
            onWrongItemPlaced.Invoke(); // 触发错误提示音效
        }
    }

    private void OnItemRemoved(SelectExitEventArgs args)
    {
        if (IsCorrectlyPlaced)
        {
            IsCorrectlyPlaced = false;
            onItemRemoved.Invoke();
        }
    }
}