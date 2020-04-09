using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject cam_following;

    // Camera shake
    public Texture2D shake_pattern;
    public float shake_traumaDecreaseRate;
    public float shake_rotationAmplitude;
    public float shake_horizontalAmplitude;
    public float shake_verticalAmplitude;

    float xOffset;
    float yOffset;
    float angleOffset;

    float shake_trauma;
    int shake_currentNoisePixel;

    void Start() {

    }

    void Update() {
        if (shake_trauma > 0)
            Shake();

        if (Input.GetKeyDown(KeyCode.T))
            ChangeTrauma(0.4f);

        transform.position = new Vector3(cam_following.transform.position.x + xOffset, cam_following.transform.position.y + yOffset, transform.position.z);
        transform.rotation = Quaternion.Euler(0, 0, angleOffset);
    }

    void Shake() {
        if (shake_currentNoisePixel >= shake_pattern.width)
            shake_currentNoisePixel = 0;

        float noiseValueR = (shake_pattern.GetPixel(shake_currentNoisePixel, 0).r * 2f) - 1f;
        float noiseValueG = (shake_pattern.GetPixel(shake_currentNoisePixel, 0).g * 2f) - 1f;
        float noiseValueB = (shake_pattern.GetPixel(shake_currentNoisePixel, 0).b * 2f) - 1f;

        shake_currentNoisePixel++;

        xOffset = noiseValueG * shake_horizontalAmplitude * shake_trauma * shake_trauma;
        yOffset = noiseValueR * shake_horizontalAmplitude * shake_trauma * shake_trauma;
        angleOffset = noiseValueB * shake_rotationAmplitude * shake_trauma * shake_trauma;

        ChangeTrauma(-shake_traumaDecreaseRate * Time.deltaTime);
    }

    void ChangeTrauma(float trauma) {
        shake_trauma = Mathf.Clamp(shake_trauma + trauma, 0f, 1f);
    }

}
