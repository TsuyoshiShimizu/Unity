using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectColorChange : MonoBehaviour
{
    [SerializeField] private Color[] Color = null;
    [SerializeField] private ParticleSystem[] particle;

    public void ChangeColor(int Num)
    {
        for(int i = 0; i < particle.Length; i++)
        {
            ParticleSystem.MainModule main = particle[i].main;
            main.startColor = Color[Num];
        }
    }
}
