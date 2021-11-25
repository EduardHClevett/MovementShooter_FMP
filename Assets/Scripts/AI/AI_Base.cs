using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Base : MonoBehaviour, IEntity
{
    public float maxHealth { get; set; } = 100;
    public float currentHealth { get; set; } = 100;


    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

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

    public virtual void Initialize()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

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
