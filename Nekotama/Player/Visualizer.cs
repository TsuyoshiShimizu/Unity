using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HC.Debug;

public class Visualizer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var visualizer = gameObject.AddComponent<ColliderVisualizer>();
        var color = ColliderVisualizer.VisualizerColorType.Blue;
        var message = "足";
        var fontSize = 10;
        visualizer.Initialize(color, message, fontSize);
    }
}
