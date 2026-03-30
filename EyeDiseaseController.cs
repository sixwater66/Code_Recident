using UnityEngine;

// 这是一个用于总控两种眼疾效果的脚本
public class EyeDiseaseController : MonoBehaviour
{
    // 1. 引用右眼模糊效果脚本
    // 确保你的 RightEyeBlurEffect.cs 脚本已存在并挂载在 Main Camera 上
    [Header("1. 右眼模糊效果引用 (RightEyeBlurEffect)")]
    public RightEyeBlurEffect blurEffect;

    // 2. 引用中心黑点效果脚本 (使用确切的类名 ScotomaSimulator)
    // 确保 ScotomaSimulator.cs 已挂载在 Main Camera 上
    [Header("2. 中心黑点效果引用 (ScotomaSimulator)")]
    public ScotomaSimulator scotomaEffect;

    [Header("3. 效果设置")]
    // 默认的模糊强度
    public float blurIntensity = 0.005f;

    void Start()
    {
        // 游戏开始时，确保所有效果都是关闭的
        SetNormalVision();
    }

    // XR Grab Interactable 的 Select Entered 事件调用：开启所有效果
    public void OnPickUpGlasses()
    {
        Debug.Log("Pick Up: 开启眼疾模拟 (Blur & Scotoma)");

        // 1. 开启右眼模糊效果 (通过 RightEyeBlurEffect 脚本的公共属性控制)
        if (blurEffect != null)
        {
            blurEffect.blurSize = blurIntensity;
            blurEffect.isEffectActive = true;
        }

        // 2. 开启中心黑点效果 (通过启用 ScotomaSimulator 脚本控制)
        if (scotomaEffect != null)
        {
            // 通过启用脚本来启动 Scotoma 的 OnRenderImage 流程
            scotomaEffect.enabled = true;
        }
    }

    // XR Grab Interactable 的 Select Exited 事件调用：关闭所有效果
    public void OnDropGlasses()
    {
        Debug.Log("Drop: 恢复正常视觉");
        SetNormalVision();
    }

    public void SetNormalVision()
    {
        // 关闭模糊
        if (blurEffect != null)
        {
            blurEffect.isEffectActive = false;
        }

        // 关闭黑点
        if (scotomaEffect != null)
        {
            scotomaEffect.enabled = false;
        }
    }
}