using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageObject : MonoBehaviour
{
    private void Awake()
    {
        Vector3 MaxPos = transform.position + transform.localScale / 2;
        Vector3 MiniPos = transform.position - transform.localScale / 2;

        if (EventManager.StageMaxPos.x == 1000 || EventManager.StageMaxPos.x < MaxPos.x) EventManager.StageMaxPos.x = MaxPos.x;
        if (EventManager.StageMaxPos.y == 1000 || EventManager.StageMaxPos.y < MaxPos.y) EventManager.StageMaxPos.y = MaxPos.y;
        if (EventManager.StageMaxPos.z == 1000 || EventManager.StageMaxPos.z < MaxPos.z) EventManager.StageMaxPos.z = MaxPos.z;
        if (EventManager.StageMinPos.x == -1000 || EventManager.StageMinPos.x > MiniPos.x) EventManager.StageMinPos.x = MiniPos.x;
        if (EventManager.StageMinPos.y == -1000 || EventManager.StageMinPos.y > MiniPos.y) EventManager.StageMinPos.y = MiniPos.y;
        if (EventManager.StageMinPos.z == -1000 || EventManager.StageMinPos.z > MiniPos.z) EventManager.StageMinPos.z = MiniPos.z;
    }

}
