using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    [Header("字幕显示设置")]
    public TextMeshProUGUI subtitleText;           // 拖入字幕Text
    public float subtitleDisplayDuration = 3.5f;   // 每句字幕最少显示多久（秒）

    [Header("跟随设置")]
    public Transform xrCamera;                     // 拖入 XR Rig 的 Main Camera（头部）
    public Vector3 offset = new Vector3(0, -0.2f, 2.0f); // 字幕在玩家前方偏移（X左右, Y上下, Z距离）

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = xrCamera != null ? xrCamera.GetComponent<Camera>() : Camera.main;

        // 确保Text初始隐藏
        if (subtitleText != null)
            subtitleText.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (xrCamera == null) return;

        // 让字幕始终跟随玩家头部前方
        Vector3 targetPosition = xrCamera.TransformPoint(offset);
        transform.position = targetPosition;

        // 始终面向玩家（Billboard效果）
        transform.LookAt(xrCamera.position);
        transform.Rotate(0, 180f, 0);   // 因为LookAt默认背对，需要翻转
    }

    // 外部调用：显示一句字幕（自动消失）
    public IEnumerator ShowSubtitle(string text)
    {
        if (subtitleText == null) yield break;

        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(subtitleDisplayDuration);

        subtitleText.gameObject.SetActive(false);
    }
}