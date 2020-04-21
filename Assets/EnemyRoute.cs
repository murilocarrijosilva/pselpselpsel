using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoute : MonoBehaviour {

    // esse script foi feito muito na pressa e tem um monte de gambiarra pq os triângulos do ConeProjection não funcionam bem quando ele tá rotacionado.

    public GameObject enemy;
    public GameObject alert;

    public float dir;
    public Vector2 maxDist;
    public Vector2 minDist;
    public float pace;

    void Start() {
        dir = 1;
    }

    void Update() {
        if (dir == 1 && transform.position.x >= maxDist.x)
            dir = -1;
        if (dir == -1 && transform.position.x <= minDist.x)
            dir = 1;

        Vector2 target = dir == 1 ? maxDist : minDist;

        transform.position = Vector2.MoveTowards(transform.position, target, pace * Time.deltaTime);

        if (dir == 1) {
            enemy.transform.rotation = Quaternion.Euler(0, 0, -180);
            enemy.GetComponent<SpriteRenderer>().flipY = true;
            
            alert.transform.localRotation = Quaternion.Euler(0, 0, 180);
            alert.transform.localPosition = new Vector2(alert.transform.localPosition.x, Mathf.Abs(alert.transform.localPosition.y) * -1);
        }
        if (dir == -1) {
            enemy.transform.rotation = Quaternion.Euler(0, 0, 0);
            enemy.GetComponent<SpriteRenderer>().flipY = false;
            
            alert.transform.rotation = Quaternion.Euler(0, 0, 0);
            alert.transform.localPosition = new Vector2(alert.transform.localPosition.x, Mathf.Abs(alert.transform.localPosition.y));
        }
    }
}
