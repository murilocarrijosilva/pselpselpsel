using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PortraitManager : MonoBehaviour {

    public Image image;
    public GameObject player;
    public Sprite portrait_hidden;
    public Sprite portrait_revealed;

    void Update() {
        if (player.GetComponent<PlayerMovement>().isInLight)
            image.sprite = portrait_revealed;
        else
            image.sprite = portrait_hidden;
    }
}
