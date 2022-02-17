using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyPooler))]
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] float spawnRadius = 20f, detectionRadius = 30f;

    EnemyPooler pooler;

    public string poolTag;

    public Vector3 offset;

    [SerializeField] GameObject enemyToSpawn;

    public int timesToSpawn = 5;

    private void Awake()
    {
        pooler = GetComponent<EnemyPooler>();
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        if(Physics.SphereCast(transform.position, detectionRadius, transform.forward, out hit))
        {
            PlayerController pc = hit.collider.GetComponent<PlayerController>();

            if (pc)
                StartCoroutine(StartSpawn());
        }
    }

    IEnumerator StartSpawn()
    {
        for(int i = 0; i < timesToSpawn; i++)
        {

            Vector3 spawnPos;

            NavMeshHit navHit;
            NavMesh.SamplePosition(transform.position + offset, out navHit, spawnRadius, NavMesh.AllAreas);

            spawnPos = navHit.position;

            RaycastHit hit;

            if(Physics.Raycast(spawnPos, Vector3.down, out hit))
            {
                spawnPos = hit.point + offset; 
            }

            //pooler.SpawnFromPool(poolTag, spawnPos, Quaternion.identity).transform.position += Random.insideUnitSphere * spawnRadius;

            Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(2);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
