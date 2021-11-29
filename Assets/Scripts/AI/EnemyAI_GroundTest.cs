using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI_GroundTest : AI_NavMesh_Base
{
    public float patrolRadius = 5f;

    GameObject target;

    [SerializeField]
    bool isPatrolling, pathDone, pathImpossible;
    [SerializeField]
    bool isResting;

    Vector3 patrolPos;

    protected override void Start()
    {
        base.Start();

        SetState(State.Patrol);
    }
    #region Patrol Logic
    //Patrol is very spotty, would like a peer review
    protected override void EnterPatrol()
    {
        base.EnterPatrol();

        pathDone = false;
        isPatrolling = true;
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

        RaycastHit playerHit;
        if(Physics.SphereCast(transform.position, patrolRadius, transform.forward, out playerHit, patrolRadius/2))
        {
            Debug.Log("Spotted player!");

            target = playerHit.transform.gameObject.GetComponent<PlayerController>().gameObject;

            if (target != null)
            {
                SetState(State.Chase);
                return;
            }
        }

        if (pathDone)
        {
            if(!isResting)
                StartCoroutine(RestPatrol());
        }

        if(isPatrolling)
        {
            if (isResting)
                return;

            float destThreshold = 0.1f;

            Vector2 dist = new Vector2(gameObject.transform.position.x - patrolPos.x, gameObject.transform.position.z - patrolPos.z);
            if (dist.magnitude > destThreshold)
                pathDone = false;
            else pathDone = true;

            //pathDone = (transform.position == new Vector3(patrolPos.x, transform.position.y, patrolPos.z));
        }
        else
        {
            pathDone = false;
            isPatrolling = true;
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
    }

    IEnumerator RestPatrol()
    {
        Debug.LogWarning("Rest invoked!");
        isResting = true;

        yield return new WaitForSeconds(Random.Range(3f, 8f));

        isPatrolling = false;
        isResting = false;   
    }

    protected override void ExitPatrol()
    {
        base.ExitPatrol();

        isResting = false;

        agent.SetDestination(target.transform.position);
    }

    #endregion

    protected override void UpdateChase()
    {
        base.UpdateChase();

        agent.SetDestination(target.transform.position);
    }

    private void OnDrawGizmos()
    {
        //Draw patrol radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.DrawWireCube(patrolPos, new Vector3(1, 1, 1));
    }
}
