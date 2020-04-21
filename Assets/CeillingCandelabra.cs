using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeillingCandelabra : MonoBehaviour
{
    void Start()
    {
        GetComponent<SpringJoint2D>().connectedAnchor = new Vector2(transform.position.x, transform.position.y + 0.0625f);
    }
}
