using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Custom Object/PhysicsMat")]
public class PhysicsMatCon : MonoBehaviour
{
    [SerializeField] private PhysicsMat PMat = PhysicsMat.Normal;
    [SerializeField] private bool ViewChange = true;

    [SerializeField] private Color SpringColor = new Color32(255, 150, 0, 255);
    [SerializeField] private Color PowerfulSpringColor = new Color32(255, 0, 0, 255);
    [SerializeField] private Color WeakFrictionColor = new Color32(0, 255, 255, 255);
    [SerializeField] private Color ZeroFrictionColor = new Color32(0, 0, 255, 255);
    [SerializeField] private Color BigFrictionColor = new Color32(0, 255, 0, 255);
    [SerializeField] private Color PowerfulBigFrictionColor = new Color32(0, 100, 0, 255);

    private enum PhysicsMat { Normal, Spring , PowerfulSpring ,WeakFriction, ZeroFriction, BigFriction, PowerfulBigFriction }
    private void Awake()
    {
        Vector3 BlockSize = transform.localScale;

        Vector3 MaxPos = transform.position + BlockSize / 2;
        Vector3 MiniPos = transform.position - BlockSize / 2;

        if (EventManager.StageMaxPos.x == 1000 || EventManager.StageMaxPos.x < MaxPos.x) EventManager.StageMaxPos.x = MaxPos.x;
        if (EventManager.StageMaxPos.y == 1000 || EventManager.StageMaxPos.y < MaxPos.y) EventManager.StageMaxPos.y = MaxPos.y;
        if (EventManager.StageMaxPos.z == 1000 || EventManager.StageMaxPos.z < MaxPos.z) EventManager.StageMaxPos.z = MaxPos.z;
        if (EventManager.StageMinPos.x == -1000 || EventManager.StageMinPos.x > MiniPos.x) EventManager.StageMinPos.x = MiniPos.x;
        if (EventManager.StageMinPos.y == -1000 || EventManager.StageMinPos.y > MiniPos.y) EventManager.StageMinPos.y = MiniPos.y;
        if (EventManager.StageMinPos.z == -1000 || EventManager.StageMinPos.z > MiniPos.z) EventManager.StageMinPos.z = MiniPos.z;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PMat != PhysicsMat.Normal)
        {
            if (ViewChange)
            {
                List<Material> Mats = new List<Material>();
                if (GetComponents<Renderer>() != null)
                {
                    Renderer[] renders = GetComponents<Renderer>();
                    for (int i = 0; i < renders.Length; i++)
                    {
                        Mats.Add(renders[i].material);
                    }
                }
                if (GetComponentsInChildren<Renderer>() != null)
                {
                    Renderer[] renders = GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renders.Length; i++)
                    {
                        Mats.Add(renders[i].material);
                    }
                }
                if(Mats != null)
                {
                    Color ChangeColor = new Color32(0, 0, 0, 255);
                    if (PMat == PhysicsMat.Spring) ChangeColor = SpringColor;
                    if (PMat == PhysicsMat.PowerfulSpring) ChangeColor = PowerfulSpringColor;
                    if (PMat == PhysicsMat.WeakFriction) ChangeColor = WeakFrictionColor;
                    if (PMat == PhysicsMat.ZeroFriction) ChangeColor = ZeroFrictionColor;
                    if (PMat == PhysicsMat.BigFriction) ChangeColor = BigFrictionColor;
                    if (PMat == PhysicsMat.PowerfulBigFriction) ChangeColor = PowerfulBigFrictionColor;
                    for (int i = 0; i < Mats.Count; i++)
                    {
                        Mats[i].color = ChangeColor;
                    }
                }
            }

            List<Collider> colliders = new List<Collider>();
            if(GetComponents<Collider>() != null)
            {
                Collider[] Colliders = GetComponents<Collider>();
                for (int i = 0; i < Colliders.Length; i++)
                {
                    if (!Colliders[i].isTrigger) colliders.Add(Colliders[i]);
                }
            }
            if (GetComponentsInChildren<Collider>() != null)
            {
                Collider[] Colliders = GetComponentsInChildren<Collider>();
                for (int i = 0; i < Colliders.Length; i++)
                {
                    if (!Colliders[i].isTrigger) colliders.Add(Colliders[i]);
                }
            }

            if(colliders != null)
            {
                for(int i = 0; i < colliders.Count; i++)
                {
                    if (PMat == PhysicsMat.Spring)
                    {     
                        colliders[i].material.bounciness = 1;
                    }
                    if (PMat == PhysicsMat.PowerfulSpring)
                    {
                        colliders[i].material.bounciness = 1;
                        colliders[i].material.bounceCombine = PhysicMaterialCombine.Maximum;
                    }
                    if (PMat == PhysicsMat.WeakFriction)
                    {
                        colliders[i].material.dynamicFriction = 0;
                        colliders[i].material.staticFriction = 0;
                    }
                    if (PMat == PhysicsMat.ZeroFriction)
                    {
                        colliders[i].material.dynamicFriction = 0;
                        colliders[i].material.staticFriction = 0;
                        colliders[i].material.frictionCombine = PhysicMaterialCombine.Minimum;
                    }
                    if (PMat == PhysicsMat.BigFriction)
                    {
                        colliders[i].material.dynamicFriction = 1;
                        colliders[i].material.staticFriction = 1;
                        colliders[i].material.frictionCombine = PhysicMaterialCombine.Maximum;
                    }

                    if (PMat == PhysicsMat.PowerfulBigFriction)
                    {
                        colliders[i].material.dynamicFriction = 2;
                        colliders[i].material.staticFriction = 1;
                        colliders[i].material.frictionCombine = PhysicMaterialCombine.Maximum;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
