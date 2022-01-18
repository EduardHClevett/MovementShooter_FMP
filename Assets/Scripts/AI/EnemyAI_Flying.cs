using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_Flying : AI_Base
{
    #region Patrol Variables
    public float patrolRadius = 10f, obstacleRadius = 5f, stoppingDistance, moveSpeed = 2f;

    Vector3 patrolPos;

    bool pathDone;

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
    #endregion

    #region Idling and Default Functions
    protected override void Start()
    {
        base.Start();

        SetState(State.Patrol);
    }

    protected override void Update()
    {
        base.Update();
        //Will contain player detection, TODO later
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

        stoppingDistance = 0f;

        //Set new patrol destination
        pathDone = false;
        patrolPos = transform.position + (Random.insideUnitSphere * patrolRadius);

        //Check if there are any obstacles in the way
        RaycastHit hit;
        if(Physics.Raycast(transform.position, patrolPos, out hit))
        {
            Debug.Log("Obstacle in the way!");

            RaycastHit avoidanceHit;
            if(!Physics.Raycast(transform.position, patrolPos, out avoidanceHit, obstacleRadius))
            {
                patrolPos = avoidanceHit.point;
            }
            else
            {
                isResting = true;
                return;
            }
        }

        StartCoroutine(MoveEnemy(patrolPos));
    }

    protected override void UpdatePatrol()
    {
        base.UpdatePatrol();
        if(pathDone)
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

    IEnumerator MoveEnemy(Vector3 destination)
    {
        transform.LookAt(destination);

        while(!pathDone)
        {
            transform.position = Vector3.Lerp(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    #endregion

    #region Debug
    private void OnDrawGizmos()
    {//Draw patrol radius
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        Gizmos.DrawWireCube(patrolPos, new Vector3(1, 1, 1));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, obstacleRadius);
    }
    #endregion
}
