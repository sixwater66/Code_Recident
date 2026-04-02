using UnityEngine;

public class VRDebugColor : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private Color _originalColor;

    void Awake()
    {
        // 尝试获取自身或子物体的 MeshRenderer
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (_meshRenderer != null)
        {
            _originalColor = _meshRenderer.material.color;
        }
    }

    // 逻辑上放错了（比如把药放进了抽屉），变成红色报警
    public void ShowErrorColor()
    {
        if (_meshRenderer != null) _meshRenderer.material.color = Color.red;
        Debug.Log("VR验证：触发错误提示！");
    }

    // 东西拿走后，恢复原样
    public void ResetColor()
    {
        if (_meshRenderer != null) _meshRenderer.material.color = _originalColor;
    }
}