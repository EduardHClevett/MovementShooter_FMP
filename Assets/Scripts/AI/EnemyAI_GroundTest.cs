using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_GroundTest : AI_NavMesh_Base
{
    public float patrolRadius = 5f, attackRadius = 5f;

    GameObject target;

    [SerializeField]
    bool pathDone, pathImpossible;

    Vector3 patrolPos;

    public WeaponContainer_AI weapon;

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

        SetState(State.Patrol);
    }

    protected override void Update()
    {
        base.Update();

        if (target == null)
        {
            RaycastHit playerHit;
            if (Physics.SphereCast(transform.position, patrolRadius, transform.forward, out playerHit, patrolRadius / 2))
            {
                PlayerController pc = playerHit.transform.GetComponent<PlayerController>();
                if (pc != null)
                {
                    Debug.Log("Spotted player!");

                    target = pc.gameObject;

                    SetState(State.Chase);

                    CancelStateTimer();
                }
            }
        }
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
    }

    protected override void ExitIdle()
    {
        base.ExitIdle();

        isResting = false;
    }

    #region Patrol Logic
    //Patrol is very spotty, would like a peer review
    protected override void EnterPatrol()
    {
        base.EnterPatrol();

        agent.stoppingDistance = 0f;

        pathDone = false;
        patrolPos = transform.position + (Random.insideUnitSphere * patrolRadius);
        patrolPos.y += 10;

        RaycastHit hit;

        if (Physics.Raycast(patrolPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            Debug.Log("Found closest ground!");
            patrolPos = hit.point;
        }

        agent.SetDestination(patrolPos);
    }
    
    protected override void UpdatePatrol()
    {
        base.UpdatePatrol();

        if (pathDone)
        {
            isResting = true;
            
            return;
        }

        float destThreshold = 0.1f;

        Vector2 dist = new Vector2(gameObject.transform.position.x - patrolPos.x, gameObject.transform.position.z - patrolPos.z);
        if (dist.magnitude > destThreshold)
            pathDone = false;
        else pathDone = true;
    }

    protected override void ExitPatrol()
    {
        base.ExitPatrol();
    }

    #endregion
    #region Chase Logic
    protected override void EnterChase()
    {
        base.EnterChase();

        agent.stoppingDistance = attackRadius;
    }

    protected override void UpdateChase()
    {
        base.UpdateChase();

        agent.SetDestination(target.transform.position);

        if (Vector3.Distance(transform.position, target.transform.position) < agent.stoppingDistance)
            SetState(State.Attack);
    }
    #endregion
    #region Attack Logic
    protected override void EnterAttack()
    {
        base.EnterAttack();

        agent.isStopped = true;

        weapon.isFiring = true;

        SetStateTimer(previousState, 5f);
    }

    protected override void UpdateAttack()
    {
        base.UpdateAttack();

        weapon.transform.LookAt(target.transform);
        transform.LookAt(target.transform);
    }

    protected override void ExitAttack()
    {
        base.ExitAttack();
        weapon.isFiring = false;

        agent.isStopped = false;
    }


    #endregion

    private void OnDrawGizmos()
    {
        //Draw patrol radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

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
