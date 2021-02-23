# UnityStateMachine

Simple C# state machine that accepts enums as states and transition triggers. Configurable through a fluent interface.

Usage example:

```
public class Foo
{
    private enum State
    {
        Idle,
        Running
    }
    
    private enum Trigger
    {
        StartTask,
        EndTask
    }

    private StateMachine<State,Trigger> stateMachine;

    public Foo()
    {
        stateMachine = new StateMachine<State,Trigger>(State.Idle); //Default state passed to constructor
        
        stateMachine.ConfigureState(State.Idle)
            .Allow(Trigger.RunTask,State.Running);
            
        stateMachine.ConfigureState(State.Running)
            .Allow(Trigger.EndTask,State.Idle);
            
        Console.WriteLine(stateMachine.CurrentState);//Outputs Idle
        stateMachine.PerformTransition(Trigger.StartTask);
        Console.WriteLine(stateMachine.CurrentState);//Outputs Running
    }
}
```
