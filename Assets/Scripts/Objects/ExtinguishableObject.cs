using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtinguishableObject : MonoBehaviour
{
    public Animator _animator;
    public GameObject _lamp;
    public GameObject[] _smoke;

    public void Extinguish() {
        _animator.SetTrigger("Extinguish");
        _lamp.SetActive(false);

        foreach (GameObject smoke in _smoke)
            smoke.SetActive(true);
    }
}
