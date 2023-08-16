public class StateMachine<T> where T : BaseState
{
    //public static event Action<T> StateChanged;
    public T CurrentState { get; private set; }

    public void Initialize(T startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }

    public bool ChangeState(T newState)
    {
        if (newState == CurrentState)
            return false;

        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();

        //StateChanged?.Invoke(CurrenState);
        return true;
    }
}