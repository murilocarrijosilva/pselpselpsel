using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightableObject : MonoBehaviour {

    public int _lightLayer;
    public bool _isInLight;

    SpriteRenderer _renderer;
    BoxCollider2D _col;

    void Start() {
        _renderer = GetComponent<SpriteRenderer>();
        _col = GetComponent<BoxCollider2D>();
    }

    void Update() {
        CheckIfInLight();
    }

    void CheckIfInLight() {
        _isInLight = Physics2D.OverlapBox(transform.position, _col.size, 0, 1 << _lightLayer);

        if (_isInLight)
            _renderer.color = Color.white;
        else
            _renderer.color = new Color(72f / 255f, 139f / 255f, 212f / 255f);
    }

}
