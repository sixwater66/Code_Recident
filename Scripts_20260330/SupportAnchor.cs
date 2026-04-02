using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// �Զ����� XRSimpleInteractable (���� Interactable ���Խ����������ᱻ������)
[RequireComponent(typeof(XRSimpleInteractable))]
public class SupportAnchor : MonoBehaviour
{
    [Header("״̬ (Status)")]
    [Tooltip("��ǰ�Ƿ���ҷ���")]
    public bool isBeingHeld = false;

    [Header("�¼� (Events)")]
    public UnityEvent onSupportGrasped;
    public UnityEvent onSupportReleased;

    private XRSimpleInteractable _simpleInteractable;

    void Awake()
    {
        _simpleInteractable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        // �������"ץ��"���֧�ŵ���¼�
        _simpleInteractable.selectEntered.AddListener(OnGrasped);
        _simpleInteractable.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        _simpleInteractable.selectEntered.RemoveListener(OnGrasped);
        _simpleInteractable.selectExited.RemoveListener(OnReleased);
    }

    private void OnGrasped(SelectEnterEventArgs args)
    {
        isBeingHeld = true;
        Debug.Log($"����ѷ�ס֧�ŵ�: {gameObject.name}");
        onSupportGrasped.Invoke();

        // ���������ﴥ����΢���ֱ��𶯷��� (Haptic Feedback) ���ӳ�����
        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
        {
            controllerInteractor.xrController.SendHapticImpulse(0.5f, 0.1f);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isBeingHeld = false;
        Debug.Log($"����ɿ���֧�ŵ�: {gameObject.name}");
        onSupportReleased.Invoke();
    }
}