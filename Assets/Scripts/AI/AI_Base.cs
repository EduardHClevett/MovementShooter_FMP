using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Base : MonoBehaviour, IEntity
{
    //Implementing Entity values
    public float maxHealth { get; set; } = 100;
    public float currentHealth { get; set; } = 100;

    //Define AI States
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    //Makes sure the state can only be set through this script, child scripts can change states using SetState
    public State currentState 
    { 
        get { return _currentState; } 
        private set
        {
            if(value != _currentState)
            {
                previousState = _currentState;
                _currentState = value;
            }    
        }
    }
    //Do not modify this var directly!
    private State _currentState = State.Idle;
    public State previousState { get; private set; } = State.Idle;
    
    public void SetState(State inState)
    {
        currentState = inState;
        //Execute logic that should happen when a state is left
        switch(previousState)
        {
            case State.Idle:
                ExitIdle();
                break;
            case State.Patrol:
                ExitPatrol();
                break;
            case State.Chase:
                ExitChase();
                break;
            case State.Attack:
                ExitAttack();
                break;
        }
        //Execute logic that should happen when a state is entered
        switch (currentState)
        {
            case State.Idle:
                EnterIdle();
                break;
            case State.Patrol:
                EnterPatrol();
                break;
            case State.Chase:
                EnterChase();
                break;
            case State.Attack:
                EnterAttack();
                break;
        }
    }

    protected virtual void Start() { }
    protected virtual void Awake() { }

    protected virtual void Update()
    {
        //Calls the Update states according to the active state
        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Patrol:
                UpdatePatrol();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Attack:
                UpdateAttack();
                break;
        }
    }

    //On Enter Functions
    protected virtual void EnterIdle() { }
    protected virtual void EnterPatrol() { }
    protected virtual void EnterChase() { }
    protected virtual void EnterAttack() { }

    //On Exit Functions
    protected virtual void ExitIdle() { }
    protected virtual void ExitPatrol() { }
    protected virtual void ExitChase() { }
    protected virtual void ExitAttack() { }

    //On Update Functions
    protected virtual void UpdateIdle() { }
    protected virtual void UpdatePatrol() { }
    protected virtual void UpdateChase() { }
    protected virtual void UpdateAttack() { }

    //Entity function implementations
    public virtual void Initialize()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        Debug.Log("Enemy hit!");
        currentHealth -= damage;

        if (currentHealth <= 0)
            KillEntity();
    }

    public virtual void KillEntity()
    {
        Destroy(gameObject);
    }

    //To be called when we want a delay on state change
    public void SetStateTimer(State inState, float time)
    {
        StartCoroutine(StateTimer(inState, time));
    }

    private IEnumerator StateTimer(State inState, float time)
    {
        yield return new WaitForSeconds(time);

        SetState(inState);
    }
}
