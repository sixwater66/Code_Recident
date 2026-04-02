using UnityEngine;
using System.Collections;

public class WrongPlacementFeedback : MonoBehaviour
{
    [Header("触发设置 (Trigger Settings)")]
    [Tooltip("会触发错误提示的物体标签")]
    public string wrongItemTag = "Med";
    [Tooltip("弹开物体的力度")]
    public float bounceForce = 2f;

    [Header("UI 反馈 (UI Feedback)")]
    [Tooltip("拖入刚刚制作的悬浮文字物体")]
    public GameObject hintTextObject;

    [Header("正确位置高亮引导 (Highlight Correct Zone)")]
    [Tooltip("拖入正确放置区的模型(例如那个木盘子)")]
    public MeshRenderer correctZoneRenderer;
    [Tooltip("高亮时的颜色 (默认半透明红黄)")]
    public Color highlightColor = new Color(1f, 0.8f, 0.2f, 0.8f);

    private Color _originalColor;
    private Coroutine _feedbackRoutine;

    void Start()
    {
        // 记录正确区域原本的颜色
        if (correctZoneRenderer != null)
        {
            _originalColor = correctZoneRenderer.material.color;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 如果进入区域的物体标签是 Med
        if (other.CompareTag(wrongItemTag))
        {
            // 1. 给物体一个斜向上的弹力 (模拟没放稳滑落或弹开)
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 方向：向上 + 向自身后方
                Vector3 bounceDir = (Vector3.up + transform.forward * -1f).normalized;
                rb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);
            }

            // 2. 触发文字与高亮反馈
            if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = StartCoroutine(ShowFeedbackCoroutine());
        }
    }

    IEnumerator ShowFeedbackCoroutine()
    {
        // 开启文字，改变正确区域颜色
        if (hintTextObject != null) hintTextObject.SetActive(true);
        if (correctZoneRenderer != null) correctZoneRenderer.material.color = highlightColor;

        // 持续显示 3 秒
        yield return new WaitForSeconds(3f);

        // 恢复原状
        if (hintTextObject != null) hintTextObject.SetActive(false);
        if (correctZoneRenderer != null) correctZoneRenderer.material.color = _originalColor;
    }
}