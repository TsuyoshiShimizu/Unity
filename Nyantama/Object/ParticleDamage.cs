using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Custom Object/ParticleDamage")]
public class ParticleDamage : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    public int Damage { get { return damage; } set { damage = value; } }

    private void OnParticleCollision(GameObject other)
    {
        if(other.gameObject.tag == "Player" && PlayerController.PlayerDamgagFlag && !PlayerController.AvoidFlag && GameDirector.GamePlay && !PlayerController.RushNoDamageFlag )
        {
            PlayerController.PlayerDamgagFlag = false;
            other.GetComponent<PlayerController>().PlayerDamage(Damage);
        }
    }
}
