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

    public float cam_maxXDistance;
    public float cam_maxYDistance;
    public float cam_xCorrectSpeed;
    public float cam_yCorrectSpeed;

    float xOffset;
    float yOffset;
    float angleOffset;

    float shake_trauma;
    float shake_maxDashTrauma;
    int shake_currentNoisePixel;

    void Start() {

    }

    void Update() {
        if (shake_trauma + shake_maxDashTrauma > 0)
            Shake();

        float newX = transform.position.x;
        float newY = transform.position.y;

        float distanceX = Mathf.Abs(transform.position.x - cam_following.transform.position.x);
        float distanceY = Mathf.Abs(transform.position.y - cam_following.transform.position.y);

        if (distanceX > cam_maxXDistance)
            newX = Mathf.Lerp(transform.position.x, cam_following.transform.position.x, cam_xCorrectSpeed * (distanceX / cam_maxXDistance));

        if (distanceY > cam_maxYDistance)
            newY = Mathf.Lerp(transform.position.y, cam_following.transform.position.y, cam_yCorrectSpeed * (distanceY / cam_maxYDistance));
        
        transform.position = new Vector3(newX + xOffset, newY + yOffset, transform.position.z);
        transform.rotation = Quaternion.Euler(0, 0, angleOffset);
    }

    void Shake() {
        if (shake_currentNoisePixel >= shake_pattern.width)
            shake_currentNoisePixel = 0;

        float noiseValueR = (shake_pattern.GetPixel(shake_currentNoisePixel, 0).r * 2f) - 1f;
        float noiseValueG = (shake_pattern.GetPixel(shake_currentNoisePixel, 0).g * 2f) - 1f;
        float noiseValueB = (shake_pattern.GetPixel(shake_currentNoisePixel, 0).b * 2f) - 1f;

        shake_currentNoisePixel++;

        xOffset = noiseValueG * shake_horizontalAmplitude * (shake_trauma + shake_maxDashTrauma) * (shake_trauma + shake_maxDashTrauma);
        yOffset = noiseValueR * shake_horizontalAmplitude * (shake_trauma + shake_maxDashTrauma) * (shake_trauma + shake_maxDashTrauma);
        angleOffset = noiseValueB * shake_rotationAmplitude * (shake_trauma + shake_maxDashTrauma) * (shake_trauma + shake_maxDashTrauma);

        AddTrauma(-shake_traumaDecreaseRate * Time.deltaTime);
    }

    public void SetTrauma(float trauma) {
        shake_trauma = Mathf.Clamp(trauma, 0f, 1f);
    }
    public void AddTrauma(float trauma) {
        shake_trauma = Mathf.Clamp(shake_trauma + trauma, 0f, 1f);
        shake_maxDashTrauma = Mathf.Clamp(shake_maxDashTrauma + trauma, 0f, 1f);
    }
    public void DashTrauma() {
        shake_maxDashTrauma = 0.4f;
    }

}
