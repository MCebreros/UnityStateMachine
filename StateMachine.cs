using System;
using System.Collections.Generic;
using System.Globalization;

public sealed class StateMachine<TState, TTrigger> where TState : struct, IConvertible
	where TTrigger : struct, IConvertible
{
	private readonly Dictionary<TState, State<TState, TTrigger>> statesMap;
	private State<TState, TTrigger> currentState;

	public TState CurrentState => currentState.ID;

	public StateMachine(TState initialState)
	{
		if (!typeof(TState).IsEnum || !typeof(TTrigger).IsEnum)
			throw new ArgumentException("Both arguments should be enumerators");
		statesMap = new Dictionary<TState, State<TState, TTrigger>>();
		ConfigureState(initialState);
		currentState = statesMap[initialState];
	}

	/// <summary>
	/// Adds the state to the machine and returns an object that allows for state configuration by chaining methods
	/// </summary>
	/// <returns>The state.</returns>
	/// <param name="stateID">State I.</param>
	public IState<TState, TTrigger> ConfigureState(TState stateID)
	{
		if (statesMap.ContainsKey(stateID))
			return statesMap[stateID];
		// ReSharper disable once RedundantIfElseBlock
		//Else for readability
		else
		{
			State<TState, TTrigger> newState = new State<TState, TTrigger>(stateID);
			statesMap.Add(stateID, newState);
			return newState;
		}
	}

	/// <summary>
	/// Moves the Machine to the next state based on the tirgger passed.
	/// Also executes the respective OnExit and OnEntry methods given that they are set.
	/// </summary>
	/// <param name="trigger">Trigger.</param>
	public void PerformTransition(TTrigger trigger)
	{
		TState nextState;
		try
		{
			nextState = currentState.triggersMap[trigger];
		}
		catch (KeyNotFoundException e)
		{
			throw new KeyNotFoundException(e.Source + " " + trigger.ToString(CultureInfo.InvariantCulture) +
										   " is not in the " +
										   currentState.ID.ToString(CultureInfo.InvariantCulture) +
										   " State trigger's map");
		}
		
		TState lastState = currentState.ID;

		currentState.onExitAction?.Invoke(nextState);
		currentState = statesMap[nextState];
		currentState.onEntryAction?.Invoke(lastState);
	}

	//TODO: Write an AllowWithArguments to define a transition with parameters
	private void PerformTransitionWithParameters(TTrigger trigger, params object[] args)
	{
		TState nextState;
		try
		{
			nextState = currentState.triggersMap[trigger];
		}
		catch (KeyNotFoundException e)
		{
			throw new KeyNotFoundException(e.Source + " " + trigger.ToString(CultureInfo.InvariantCulture) +
										   " is not in the " +
										   currentState.ID.ToString(CultureInfo.InvariantCulture) +
										   " State trigger's map");
		}

		TState lastState = currentState.ID;
		currentState.onExitAction?.Invoke(nextState);
		currentState = statesMap[nextState];
		currentState.onEntryAction?.Invoke(lastState);
	}

	/// <summary>
	/// Executes the current state's Update Action (if there is one)
	/// </summary>
	public void Update()
	{
		currentState.updateAction?.Invoke();
	}

	/// <summary>
	/// Executes the current state's Fixed Update Action (if there is one)
	/// </summary>
	public void FixedUpdate()
	{
		currentState.fixedUpdateAction?.Invoke();
	}

	private sealed class State<S, T> : IState<S,T> where S : struct, IConvertible where T : struct, IConvertible
	{
		public Action<S> onEntryAction = null;
		public Action<S> onExitAction = null;
		public Action updateAction = null;
		public Action fixedUpdateAction = null;
		public readonly Dictionary<T, S> triggersMap = new Dictionary<T, S>();

		public S ID { get; }

		public State(S stateID)
		{
			ID = stateID;
		}
		
		public IState<S, T> Allow(T trigger, S targetState)
		{
			triggersMap.Add(trigger, targetState);
			return this;
		}
		
		public IState<S, T> SetOnEntry(Action<S> entryMethod)
		{
			onEntryAction = entryMethod;
			return this;
		}
		
		public IState<S, T> SetOnExit(Action<S> exitMethod)
		{
			onExitAction = exitMethod;
			return this;
		}

		public IState<S, T> SetUpdateMethod(Action updateMethod)
		{
			updateAction = updateMethod;
			return this;
		}

		public IState<S, T> SetFixedUpdateMethod(Action fixedUpdateMethod)
		{
			fixedUpdateAction = fixedUpdateMethod;
			return this;
		}
	}
}

public interface IState<S,T> where S : struct, IConvertible where T : struct, IConvertible
{
	/// <summary>
	/// Adds a trigger and a target state to the state's trigger map so the State Machine can perform a transition 
	/// </summary>
	/// <returns>The trigger.</returns>
	/// <param name="trigger">Trigger.</param>
	/// <param name="targetState">Target state.</param>
	IState<S, T> Allow(T trigger, S targetState);

	/// <summary>
	/// Takes in a method with a state parameter which represents the previous state
	/// </summary>
	/// <returns>The on entry.</returns>
	/// <param name="entryMethod">Entry method.</param>
	IState<S, T> SetOnEntry(Action<S> entryMethod);

	/// <summary>
	/// Takes in a method with a state parameter which represents the upcoming state
	/// </summary>
	/// <returns>The on exit.</returns>
	/// <param name="exitMethod">Exit method.</param>
	IState<S, T> SetOnExit(Action<S> exitMethod);

	IState<S, T> SetUpdateMethod(Action updateMethod);

	IState<S, T> SetFixedUpdateMethod(Action fixedUpdateMethod);
}
