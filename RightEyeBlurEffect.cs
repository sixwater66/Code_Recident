using UnityEngine;

[ExecuteInEditMode] // 让效果在编辑器里也能看到（不用运行）
[RequireComponent(typeof(Camera))]
public class RightEyeBlurEffect : MonoBehaviour
{
    public Shader blurShader;
    [Range(0.0f, 0.02f)]
    public float blurSize = 0.005f;

    // 效果开关
    public bool isEffectActive = false;

    private Material _material;

    // 自动创建材质
    Material material
    {
        get
        {
            if (_material == null)
            {
                _material = new Material(blurShader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
            return _material;
        }
    }

    // 核心：Unity 会在渲染完场景后自动调用这个函数
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (blurShader == null || !isEffectActive)
        {
            // 如果没开启，直接把原图传给下一步，不做处理
            Graphics.Blit(source, destination);
            return;
        }

        // 传递参数给 Shader
        material.SetFloat("_BlurSize", blurSize);
        material.SetFloat("_Enable", 1.0f); // 确保 Shader 内部逻辑开启

        // 执行渲染：Source(原图) -> Material(Shader处理) -> Destination(屏幕)
        Graphics.Blit(source, destination, material);
    }

    void OnDisable()
    {
        if (_material)
        {
            DestroyImmediate(_material);
        }
    }
}