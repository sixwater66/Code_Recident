using UnityEngine;

// 挂载在每个可抓取的物品上（药、水杯等）
public class ItemPlacementGuide : MonoBehaviour
{
    [Header("物品的正确归宿")]
    [Tooltip("拖入这个物品真正应该放进去的正确区域的模型 (用来做高亮引导)")]
    public MeshRenderer correctZoneRenderer;

    [Tooltip("专属的错误提示文字 (比如：药应该放在看得见的桌面上)")]
    public string customHintText = "这个好像不是放这里的";
}