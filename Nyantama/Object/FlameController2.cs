using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameController2 : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private Vector3 RotVec = Vector3.zero;
    [SerializeField] private float Strength = 1.0f;
    [SerializeField] private bool OnOffFlag = false;
    [SerializeField] private float OnTime = 5;
    [SerializeField] private float OffTime = 5;
    [SerializeField] private bool ViewLock = true;
    [HideInInspector]
    [SerializeField] private ParticleSystem particle = null;
    [HideInInspector]
    [SerializeField] private ParticleDamage PDamage = null;
    [HideInInspector]
    [SerializeField] private Transform BeamTransform = null;

    private bool OnFlag = true;
    private float TimeDelta = 0;

    private void Awake()
    {
        var main = particle.main;
        main.startLifetime = Strength;
    }

    private void Start()
    {
        PDamage.Damage = damage;
        BeamTransform.eulerAngles = RotVec;
    }

    private void OnValidate()
    {
        if (!ViewLock)
        {
            var main = particle.main;
            main.startLifetime = Strength;
        }
        BeamTransform.eulerAngles = RotVec;
    }

    private void FixedUpdate()
    {
        if (OnOffFlag)
        {
            TimeDelta += 0.02f;
            if (OnFlag)
            {
                if (TimeDelta >= OnTime)
                {
                    TimeDelta = 0;
                    OnFlag = false;
                    particle.Stop();
                }

            }
            else
            {
                if (TimeDelta >= OffTime)
                {
                    TimeDelta = 0;
                    OnFlag = true;
                    particle.Play();
                }
            }
        }
    }
}
