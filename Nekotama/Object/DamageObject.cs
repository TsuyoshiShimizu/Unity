using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Custom Object/DamageObject")]
public class DamageObject : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    public int Damage { get { return damage; }  set { damage = value; } }
}
