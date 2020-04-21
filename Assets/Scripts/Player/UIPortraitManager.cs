using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPortraitManager : MonoBehaviour {

    public Image image;
    public GameObject player;
    public Sprite portrait_hidden;
    public Sprite portrait_revealed;

    void Update() {
        if (!player.GetComponent<LightableObject>()._isInLight && player.GetComponent<Player>().revealed == 0)
            image.sprite = portrait_hidden;
        else
            image.sprite = portrait_revealed;
    }
}
