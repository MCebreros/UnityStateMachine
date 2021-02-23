# UnityStateMachine

Simple C# state machine that accepts enums as states and transition triggers. Allowed triggers as well as entry, exit and update methods per state are configurable through a fluent interface.

Usage example:

```csharp
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
            .Allow(Trigger.EndTask,State.Idle)
            .SetOnEntry(RunningEntry);
            
        Console.WriteLine(stateMachine.CurrentState);//Outputs "Idle"
        stateMachine.PerformTransition(Trigger.StartTask);//Outputs "Starting to run"
        Console.WriteLine(stateMachine.CurrentState);//Outputs "Running"
    }
    
    private RunningEntry(State previous)
    {
        Console.WriteLine("Starting to run");
    }
}
```
