using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShowContent : MonoBehaviour
{
    public Toggle toggle;
    public Image toggle_image;
    public Sprite image_on;
    public Sprite image_off;

    public List<Element> Objects = new List<Element>();

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }
    private void OnToggleValueChanged(bool value)
    {
        if(value)
        {
            toggle_image.sprite = image_off;

            foreach (var child in Objects)
            {
                if(child.ShowElement)
                child.gameObject.SetActive(true);
            }
     
        }
        else
        {
            toggle_image.sprite = image_on;

            foreach (var child in Objects)
            {
                if (child.ShowElement)
                    child.gameObject.SetActive(false);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
