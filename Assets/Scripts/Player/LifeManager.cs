using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour {
    public Sprite heartFilled;
    public Sprite heartEmpty;
    public GameObject player;
    public Image heart_0;
    public Image heart_1;
    public Image heart_2;

    void Update() {
        int hp = player.GetComponent<Player>().hp;

        heart_0.sprite = heartEmpty;
        heart_1.sprite = heartEmpty;
        heart_2.sprite = heartEmpty;

        if (hp > 0)
            heart_0.sprite = heartFilled;

        if (hp > 1)
            heart_1.sprite = heartFilled;

        if (hp > 2)
            heart_2.sprite = heartFilled;

    }
}
