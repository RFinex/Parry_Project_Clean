public class StateMachine<T>
{
    private BaseState<T> currentState;

    private T obj;

    public StateMachine(T obj)
    {
        this.obj = obj;
    }

    public void ChangeState(BaseState<T> state)
    {
        if (currentState != null)
            currentState.Exit(obj);

        currentState = state;

        currentState.Enter(obj);
    }
}
