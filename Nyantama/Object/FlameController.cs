using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameController : MonoBehaviour
{
    
    [SerializeField] private float Strength = 1.0f;
    [SerializeField] private bool OnOffFlag = false;
    [SerializeField] private float OnTime = 5;
    [SerializeField] private float OffTime = 5;
    [SerializeField] private bool ViewLock = true;
    [SerializeField] private ParticleSystem particle = null;

    private bool OnFlag = true;
    private float TimeDelta = 0;

    private void Awake()
    {
        var main = particle.main;
        main.startLifetime = Strength;
    }

    private void OnValidate()
    {
        if (!ViewLock)
        {
            var main = particle.main;
            main.startLifetime = Strength;
        }
    }

    private void FixedUpdate()
    {
        if (OnOffFlag)
        {
            TimeDelta += 0.02f;
            if (OnFlag)
            {
                if(TimeDelta >= OnTime)
                {
                    TimeDelta = 0;
                    OnFlag = false;
                    particle.Stop();
                }
                
            }
            else
            {
                if(TimeDelta >= OffTime)
                {
                    TimeDelta = 0;
                    OnFlag = true;
                    particle.Play();
                }
            }

        }
    }
}
