using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPortraitAlert : MonoBehaviour {
    
    public Image image;
    public GameObject player;

    void Update() {
        if (!player.GetComponent<LightableObject>()._isInLight && player.GetComponent<Player>().revealed == 0)
            image.color = new Color(14f/255f, 4f/255f, 33f/255f);
        else {
            image.color = Color.Lerp(Color.white, Color.red, player.GetComponent<Player>().revealed);
        }
    }
}
