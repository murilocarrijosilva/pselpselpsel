using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour {

    RectTransform _rectTransform;
    void Start() {
        _rectTransform = GetComponent<RectTransform>();
        Cursor.visible = false;
    }

    void Update() {
        float x = 480f / Screen.width;
        float y = 270f / Screen.height;
        _rectTransform.anchoredPosition = new Vector2(Input.mousePosition.x * x, Input.mousePosition.y * y);
    }
}
