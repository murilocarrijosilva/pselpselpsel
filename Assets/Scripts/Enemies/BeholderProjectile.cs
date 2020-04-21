using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeholderProjectile : MonoBehaviour {

    public float loadTime;
    public Vector2 direction;
    public float speed;

    float currentTime;

    void Start() {
        currentTime = loadTime;
    }

    void Update() {
        if (currentTime <= 0) {
            GetComponent<Animator>().SetTrigger("shoot");
            GetComponent<Rigidbody2D>().velocity = direction * speed;
            GetComponent<AudioSource>().enabled = true;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        if (Physics2D.OverlapBox(transform.position, GetComponent<BoxCollider2D>().size, transform.rotation.eulerAngles.z, 1 << 8)) {
            Destroy(gameObject);
        }

        Collider2D hit = Physics2D.OverlapBox(transform.position, GetComponent<BoxCollider2D>().size, transform.rotation.eulerAngles.z, 1 << 10);
        if (hit) {
            hit.gameObject.GetComponent<Player>().Damage();
            Destroy(gameObject);
        }

        currentTime -= Time.deltaTime;
    }

}
