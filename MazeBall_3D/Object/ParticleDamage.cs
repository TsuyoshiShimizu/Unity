using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Custom Object/ParticleDamage")]
public class ParticleDamage : MonoBehaviour
{
    private void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.tag == "Player" && EventManager.DamageFlag)
        {
            Time.timeScale = 0.2f;
            EventManager.DamageFlag = false;
            other.GetComponent<PlayerCon>().Damage();
            Invoke("DamageAfter", 0.5f);
        }
    }

    private void DamageAfter()
    {
        EventManager.eventManager.GameOver();
    }
}
