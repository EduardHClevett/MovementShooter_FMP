using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_Ground : AI_NavMesh_Base
{
    public float attackRadius = 5f;

    GameObject target;

    [SerializeField] Animator anim;

    [SerializeField]
    bool pathDone, pathImpossible;

    Vector3 patrolPos;

    public WeaponData_AI weapon;

    bool isResting
    {
        get { return _isResting; }
        set
        {
            _isResting = value;

            if (_isResting)
                SetState(State.Idle);
        }
    }

    [SerializeField]
    private bool _isResting;

    protected override void Start()
    {
        base.Start();

        target = GameObject.Find("Player");

        SetState(State.Chase);

        weapon.target = target.transform;
    }


    protected override void EnterIdle()
    {
        base.EnterIdle();

        if (isResting)
        {
            if(!SetStateTimer(State.Patrol, Random.Range(3f, 8f)))
            {
                Debug.LogWarning("Failed to set new State!");
            }
        }

        anim.SetTrigger("ToIdle");
    }

    protected override void ExitIdle()
    {
        base.ExitIdle();

        isResting = false;
    }

    #region Chase Logic
    protected override void EnterChase()
    {
        base.EnterChase();

        anim.SetBool("Moving", true);

        agent.stoppingDistance = attackRadius;
    }

    protected override void UpdateChase()
    {
        base.UpdateChase();

        agent.SetDestination(target.transform.position);

        if (Vector3.Distance(transform.position, target.transform.position) < agent.stoppingDistance)
            SetState(State.Attack);
    }

    protected override void ExitChase()
    {
        base.ExitChase();

        anim.SetBool("Moving", false);
    }
    #endregion

    #region Attack Logic
    protected override void EnterAttack()
    {
        base.EnterAttack();

        agent.isStopped = true;

        weapon.isFiring = true;

        anim.SetBool("Attacking", true);
    }

    protected override void UpdateAttack()
    {
        base.UpdateAttack();

        weapon.transform.LookAt(target.transform);

        Vector3 look = new Vector3(target.transform.position.x, 0, target.transform.position.z);

        transform.LookAt(look);

        if (Vector3.Distance(transform.position, target.transform.position) > agent.stoppingDistance)
            SetState(State.Chase);
    }

    protected override void ExitAttack()
    {
        base.ExitAttack();
        weapon.isFiring = false;

        agent.isStopped = false;

        anim.SetBool("Attacking", false);
    }


    #endregion

    private void OnDrawGizmos()
    {
        //Draw patrol radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.DrawWireCube(patrolPos, new Vector3(1, 1, 1));

        Gizmos.color = Color.red;

        if(target != null)
        {
            Gizmos.DrawWireSphere(target.transform.position, 1f);
        }

        if(agent != null)
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
    }
}
