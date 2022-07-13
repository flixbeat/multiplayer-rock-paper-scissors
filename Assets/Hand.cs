using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hand : MonoBehaviour
{
   

    private Image image;
    
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }
}
