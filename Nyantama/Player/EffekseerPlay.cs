using UnityEngine;

public class EffekseerPlay : MonoBehaviour
{
    private Effekseer.EffekseerEmitter effekseer;
    private void OnEnable()
    {
        if (effekseer == null) effekseer = GetComponent<Effekseer.EffekseerEmitter>();
        effekseer.Play(0);
    }

}
