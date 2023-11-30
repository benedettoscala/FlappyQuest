using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Throw;

using UnityEngine;
using UnityEngine.SceneManagement;

public class BirdMovement : MonoBehaviour
{

    private CharacterController Controller;
    private Vector3 Velocity;
    private bool Cooldown;

    private GameObject lookAt;
    public GameObject LookAtSu;
    public GameObject LookAtGiu;

    // Start is called before the first frame update
    void Start()
    {
        Controller = gameObject.GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "hit"){
            SceneManager.LoadScene("FlappyBird");
        }

        if(other.gameObject.tag == "score"){
            Debug.Log("Hai guadagnato un punto");
        }
    }

    // Update is called once per frame
    void Update()
    {
        #region Movement
        Velocity.y += -15 * Time.deltaTime;
        //the bird drops
        

        //se il giocatore preme A sul touch destro
        if (OVRInput.Get(OVRInput.Button.One))
        {
            //se il giocatore non è in cooldown
            if (!Cooldown)
            {
                //il giocatore salta
                Velocity.y = 10;
                //il giocatore entra in cooldown
                Cooldown = true;
                //il giocatore non è più in cooldown dopo 1 secondo
                StartCoroutine(CooldownTimer());
            }
        }
        Controller.Move(Velocity * Time.deltaTime);
        #endregion

        #region Tilting
        if (Velocity.y > 0)
        {
            lookAt = LookAtSu;
        }
        else
        {
            lookAt = LookAtGiu;
        }

        Quaternion LookOnLook = Quaternion.LookRotation(lookAt.transform.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, LookOnLook, 5 * Time.deltaTime);
        #endregion
    }

    private IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(0.2f);
        Cooldown = false;
    }
}
