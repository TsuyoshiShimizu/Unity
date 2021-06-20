using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireEnhanceCon : MonoBehaviour
{
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        transform.position = PlayerController.PlayerPos;
    }
}
