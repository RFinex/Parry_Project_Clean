using System;
using System.Collections.Generic;

public class StateMachine<T>
{
    private BaseState<T> currentState;
    private T owner;

    private Dictionary<Type, BaseState<T>> stateDic = new Dictionary<Type, BaseState<T>>();

    public BaseState<T> CurrentState => currentState;

    public StateMachine(T owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// StateMachine 상태 변경 Method
    /// </summary>
    /// <typeparam name="TState"> 전환할 상태 타입 </typeparam>
    public void ChangeState<TState>() where TState : BaseState<T>, new()
    {
        Type stateType = typeof(TState);

        if (currentState != null && currentState.GetType() == stateType)
            return;

        // 해당 상태가 없으면 새로 등록
        if (!stateDic.TryGetValue(stateType, out var nextState))
        {
            nextState = new TState();
            stateDic.Add(stateType, nextState);
        }

        if (currentState != null)
        {
            currentState.Exit(owner);
        }

        currentState = nextState;
        currentState.Enter(owner);
    }
}
