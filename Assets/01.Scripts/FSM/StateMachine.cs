using System;
using System.Collections.Generic;

public class StateMachine<T> where T : class
{
    private BaseState<T> currentState;
    private T owner;

    private Dictionary<Type, BaseState<T>> stateDic = new Dictionary<Type, BaseState<T>>();

    public BaseState<T> CurrentState => currentState;

    public BlackBoard blackBoard;

    public StateMachine(T owner, BlackBoard blackBoard)
    {
        this.owner = owner;
        this.blackBoard = blackBoard;
    }

    public void ChangeState(Type type)
    {
        if (currentState != null && currentState.GetType() == type)
            return;

        if (currentState != null)
        {
            currentState.Exit(owner);
            currentState.OnTransition -= ChangeState;
        }

        if (!stateDic.TryGetValue(type, out var nextState))
            return;

        currentState = nextState;
        
        currentState.OnTransition -= ChangeState;
        currentState.OnTransition += ChangeState;

        currentState.Enter(owner);

        //Utils.Log<StateMachine<T>>($"{type.Name}");
    }

    public void AddState<TState>() where TState : BaseState<T>, new()
    {
        if (!stateDic.TryGetValue(typeof(TState), out var nextState))
        {
            nextState = new TState();
            stateDic.Add(typeof(TState), nextState);
        }
    }

    public bool IsState(Type type)
    {
        if(!stateDic.TryGetValue(type, out var targetState))
            return false;

        return currentState == targetState;
    }

    public TState GetState<TState>() where TState : BaseState<T>
    {
        if (stateDic.TryGetValue(typeof(TState), out var targetState))
        {
            return (TState)targetState;
        }

        return null;
    }
}

public class PlayerStateMachine : StateMachine<PlayerController>
{
    public PlayerStateMachine(PlayerController owner, PlayerBlackBoard blackBoard) : base(owner, blackBoard)
    {
        
    }

    public void PlayerAddState<TState>() where TState : PlayerBaseState, new()
    {
        AddState<TState>();

        GetState<TState>().Initialized(blackBoard);
    }
}