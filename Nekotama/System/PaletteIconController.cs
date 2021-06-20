using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PaletteIconController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform ApIcon = null;
    private Vector2 FirstPos;
    private GameObject ActionPalette;
    [SerializeField] private int iconNumber = 0;

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        ApIcon.position += new Vector3(eventData.delta.x, eventData.delta.y, 0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      //  ActionPalette.GetComponent<PaletteController>().ClickUP(iconNumber);
        ApIcon.position = FirstPos;
    }
    
    private void Reset()
    {
        ApIcon = GetComponent<RectTransform>();
    }
    

    // Start is called before the first frame update
    void Start()
    {
        FirstPos = ApIcon.position;
        ActionPalette = GameObject.Find("ActionPalette");
    }
}
