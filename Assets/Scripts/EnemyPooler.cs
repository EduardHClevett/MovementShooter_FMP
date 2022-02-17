using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [SerializeField] GameObject objToSpawn;

    public Dictionary<string, Queue<GameObject>> poolDictionary;
    public List<Pool> pools;
    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for(int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                if (obj.GetComponent<AI_Base>())
                    obj.GetComponent<AI_Base>().isPooled = true;
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
            return null;

        if(poolDictionary[tag].Count > 0)
        {
            objToSpawn = poolDictionary[tag].Dequeue();

            objToSpawn.SetActive(true);

            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;

            if (!objToSpawn.GetComponent<NavMeshAgent>().isOnNavMesh)
                objToSpawn.GetComponent<NavMeshAgent>().Warp(objToSpawn.transform.position);
        }
        

        //poolDictionary[tag].Enqueue(objToSpawn);

        return objToSpawn;
    }
}
