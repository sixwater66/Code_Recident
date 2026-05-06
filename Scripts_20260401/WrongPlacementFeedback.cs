using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro; // 用于修改文字内容

[RequireComponent(typeof(XRSocketInteractor))]
public class WrongPlacementFeedback : MonoBehaviour
{
    [Header("UI 反馈设置")]
    [Tooltip("拖入悬浮文字物体 (必须包含 TextMeshPro 组件)")]
    public GameObject hintTextObject;
    public Color highlightColor = new Color(1f, 0.8f, 0.2f, 0.8f);

    private XRSocketInteractor _socket;
    private Coroutine _feedbackRoutine;
    private TextMeshPro _tmpText;

    void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();
        if (hintTextObject != null)
        {
            _tmpText = hintTextObject.GetComponent<TextMeshPro>();
        }
    }

    void OnEnable() => _socket.hoverEntered.AddListener(OnHoverEnter);
    void OnDisable() => _socket.hoverEntered.RemoveListener(OnHoverEnter);

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (args.interactableObject is IXRSelectInteractable selectInteractable)
        {
            // 如果 Socket (结合你的 PlacementZoneValidator) 拒绝了这个物体
            if (!_socket.CanSelect(selectInteractable))
            {
                // 获取物品身上的“向导信息”
                ItemPlacementGuide guide = args.interactableObject.transform.GetComponent<ItemPlacementGuide>();
                TriggerWrongFeedback(guide);
            }
        }
    }

    private void TriggerWrongFeedback(ItemPlacementGuide guide)
    {
        if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(ShowFeedbackCoroutine(guide));
    }

    IEnumerator ShowFeedbackCoroutine(ItemPlacementGuide guide)
    {
        MeshRenderer targetRenderer = null;
        Color originalColor = Color.white;

        // 1. 设置文字内容并显示
        if (hintTextObject != null)
        {
            if (_tmpText != null && guide != null)
                _tmpText.text = guide.customHintText; // 读取物品的专属文字

            hintTextObject.SetActive(true);
        }

        // 2. 读取物品的“老家”并高亮
        if (guide != null && guide.correctZoneRenderer != null)
        {
            targetRenderer = guide.correctZoneRenderer;
            originalColor = targetRenderer.material.color; // 记住它原本的颜色
            targetRenderer.material.color = highlightColor; // 变色
        }

        // 等待 3 秒
        yield return new WaitForSeconds(3f);

        // 3. 恢复原状
        if (hintTextObject != null) hintTextObject.SetActive(false);
        if (targetRenderer != null) targetRenderer.material.color = originalColor;
    }
}