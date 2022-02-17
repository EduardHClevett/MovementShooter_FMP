using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_NavMesh_Base : AI_Base
{
    protected NavMeshAgent agent;

    protected override void Awake()
    {
        base.Awake();

        agent = GetComponent<NavMeshAgent>();
    }

    protected override void Update()
    {
        base.Update();
    }


    #region State Manager
    //On Enter Functions
    protected override void EnterIdle() 
    {


    }
    protected override void EnterPatrol() 
    {
        
    }
    protected override void EnterChase() 
    {

    }
    protected override void EnterAttack()
    {
        
    }

    //On Exit Functions
    protected override void ExitIdle()
    {

    }
    protected override void ExitPatrol()
    {

    }
    protected override void ExitChase()
    {

    }
    protected override void ExitAttack()
    {

    }

    //On Update Functions
    protected override void UpdateIdle()
    {

    }
    protected override void UpdatePatrol()
    {

    }
    protected override void UpdateChase()
    {

    }
    protected override void UpdateAttack()
    {

    }
    #endregion
}
