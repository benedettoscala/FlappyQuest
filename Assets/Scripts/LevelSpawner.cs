using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    public GameObject worldCenter;
    public GameObject Template;
    public GameObject TemplateEmpty;
    public GameObject SpawnTo;
    private float DistanceTravelled = 0;
    // Start is called before the first frame update
    void Start()
    {
        GameObject Spawned = Instantiate(TemplateEmpty, SpawnTo.transform.position, transform.rotation);
        Spawned.transform.parent = transform;
        float spostamentoXLocale = 25f;
        SpawnTo.transform.Translate(spostamentoXLocale, 0f, 0f, Space.Self);
        GameObject Spawned1 = Instantiate(TemplateEmpty, SpawnTo.transform.position, transform.rotation);
        Spawned1.transform.parent = transform;
        SpawnTo.transform.Translate(spostamentoXLocale, 0f, 0f, Space.Self);
        GameObject Spawned2 = Instantiate(TemplateEmpty, SpawnTo.transform.position, transform.rotation);
        Spawned2.transform.parent = transform;
        
        GameObject Spawned3 = Instantiate(TemplateEmpty, Spawned2.transform.position, transform.rotation);
        Spawned3.transform.Translate(spostamentoXLocale, 0f, 0f, Space.Self);
        Spawned3.transform.parent = transform;

    }


    // Update is called once per frame
    void Update()
    {
        transform.localPosition += new Vector3(-5 * Time.deltaTime,0, 0);
        
        if(transform.localPosition.x - DistanceTravelled < -25)
        {
            DistanceTravelled = transform.localPosition.x;
            GameObject spawned = Instantiate(Template, SpawnTo.transform.position, transform.rotation);
            spawned.transform.parent = transform;
        }

    } 

    
} 



