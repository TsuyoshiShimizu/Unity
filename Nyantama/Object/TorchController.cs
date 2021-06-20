using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchController : MonoBehaviour
{
    [SerializeField] private float LightIntensity = 3.0f;
    [SerializeField] private float LightRange = 30.0f;
    [SerializeField] private bool FlashFlag = false;
    [SerializeField] private bool StartON = true;
    [SerializeField] private float OnTime = 5.0f;
    [SerializeField] private float OffTime = 5.0f;
    [SerializeField] private float StartLagTime = 0;

    [SerializeField] private GameObject Torch = null;
    private bool OnFlag = true;
    private bool StartFlag = false;
    private float TimeDelta = 0;

    // Start is called before the first frame update
    void Start()
    {
        Light TorchLight = Torch.GetComponent<Light>();
        TorchLight.intensity = LightIntensity;
        TorchLight.range = LightRange;
        if (FlashFlag && !StartON)
        {
            OnFlag = false;
            Torch.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (FlashFlag)
        {
            TimeDelta += 0.02f;
            if (StartFlag)
            {
                if (OnFlag)
                {
                    if(TimeDelta >= OnTime)
                    {
                        TimeDelta = 0;
                        OnFlag = false;
                        Torch.SetActive(false);
                    }
                }
                else
                {
                    if (TimeDelta >= OffTime)
                    {
                        TimeDelta = 0;
                        OnFlag = true;
                        Torch.SetActive(true);
                    }
                }
            }
            else
            {
                if(TimeDelta >= StartLagTime)
                {
                    TimeDelta = 0;
                    StartFlag = true;
                }
            }
        }
    }


}
