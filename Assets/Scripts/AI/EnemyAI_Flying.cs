using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Flying : AI_Base
{
    #region Patrol Variables
    public float stoppingDistance = 0f, moveSpeed = 2f, scanRadius = 10f;

    [SerializeField] Transform[] waypoints;

    Transform currentWaypoint;

    public Vector3 velocity = Vector3.zero;

    bool pathDone;

    public WeaponData_AI weapon;

    [SerializeField] private bool _isResting;
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
    #endregion

    #region Player Detection Variables
    GameObject target;

    [SerializeField] private Vector3 moveDest = Vector3.zero;
    #endregion

    #region Idling and Default Functions
    protected override void Start()
    {
        base.Start();

        SetState(State.Patrol);
    }

    protected override void EnterIdle()
    {
        base.EnterIdle();

        if (isResting)
        {
            if (!SetStateTimer(State.Patrol, Random.Range(3f, 8f)))
            {
                Debug.LogWarning("Failed to set new State!");
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (target == null)
        {
            RaycastHit playerHit;
            if (Physics.SphereCast(transform.position, scanRadius, transform.forward, out playerHit, scanRadius))
            {
                PlayerController pc = playerHit.transform.GetComponent<PlayerController>();
                if (pc != null)
                {
                    StopAllCoroutines();

                    Debug.Log("Spotted Player!");

                    target = pc.gameObject;

                    SetState(State.Chase);

                    CancelStateTimer();
                }
            }
        }
    }

    protected override void ExitIdle()
    {
        base.ExitIdle();

        isResting = false;
    }
    #endregion
    #region Patrol Functions
    protected override void EnterPatrol()
    {
        base.EnterPatrol();

        pathDone = false;

        currentWaypoint = waypoints[Random.Range(0, waypoints.Length - 1)];

        StartCoroutine(MoveEnemy(currentWaypoint.position));
    }

    protected override void UpdatePatrol()
    {
        base.UpdatePatrol();

        float destThreshold = 0.1f;

        Vector2 dist = new Vector2(gameObject.transform.position.x - currentWaypoint.position.x, gameObject.transform.position.z - currentWaypoint.position.z);
        if (dist.magnitude <= destThreshold)
        { pathDone = true; isResting = true; }
    }

    IEnumerator MoveEnemy(Vector3 destination)
    {
        while(!pathDone)
        {
            transform.LookAt(destination);

            moveDest = destination;

            transform.position = Vector3.SmoothDamp(transform.position, moveDest, ref velocity, Time.deltaTime, moveSpeed);

            yield return null;
        }
    }
    #endregion

    #region Chase Player
    protected override void EnterChase()
    {
        base.EnterChase();

        pathDone = false;

        stoppingDistance = scanRadius;

        StartCoroutine(MoveEnemy(target.transform.position));
    }

    protected override void UpdateChase()
    {
        base.UpdateChase();

        if (Vector3.Distance(transform.position, moveDest) <= stoppingDistance)
            SetState(State.Attack);
    }
    #endregion
    #region Attack Logic
    protected override void EnterAttack()
    {
        base.EnterAttack();

        pathDone = true;

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

        pathDone = false;
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if(currentWaypoint != null)
            Gizmos.DrawCube(currentWaypoint.position, new Vector3(1, 1, 1));

        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(transform.position, scanRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawCube(moveDest, new Vector3(1, 1, 1));
    }
    #endregion
}