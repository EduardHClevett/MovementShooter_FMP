using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TimerEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            switch(TimerManager.instance.timerGoing)
            {
                case true:
                    TimerManager.instance.FinishTimer();
                    break;
            }
        }    
    }
}
