using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PushableObject : MonoBehaviour
{
    Rigidbody2D _rb2d;

    public float mass;

    void Start() {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    public void PushBack(Vector2 direction, float force) {
        _rb2d.velocity += direction * (force / mass);
    }

}
