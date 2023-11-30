using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class SpawnTubes : MonoBehaviour
{
    public GameObject Tubes;
    public GameObject Spawner1;
    public GameObject Spawner2;
    
    // Start is called before the first frame update
    void Start()
    {
        float RandomFloat1 = Random.Range(8.5f, 15f);
        Spawner1.transform.position += new Vector3(0, RandomFloat1, 0);
        GameObject tube1 = Instantiate(Tubes, Spawner1.transform.position, Tubes.transform.rotation);
        float RandomFloat2 = Random.Range(8.5f, 15f);
        Spawner2.transform.position += new Vector3(0, RandomFloat2, 0);
        GameObject tube2 = Instantiate(Tubes, Spawner2.transform.position, Tubes.transform.rotation);
        //make them child of the spawner
        tube1.transform.parent = Spawner1.transform;
        tube2.transform.parent = Spawner2.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
