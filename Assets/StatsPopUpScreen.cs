using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsPopUpScreen : MonoBehaviour
{
    public ExperienceManager expManager;
    public bool appearedOnce = false;


    public void Update()
    {
        if(expManager.availableStatPoints > 0 && appearedOnce == false)
        {
            appearedOnce = true;
            this.gameObject.SetActive(true);
        }
    }
}
