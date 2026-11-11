using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DummyEnemyDataHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private DummyAttackData data;

    public event Action<DummyAttackData> OnEnter;
    public event Action OnExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnter?.Invoke(data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExit?.Invoke();
    }
}
