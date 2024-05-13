using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollowForHand : MonoBehaviour
{
    public float yOffset = 0f;
    public float zOffset = 0f;
    private Animator _animator;
    private Camera _mainCam;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _mainCam = Camera.main;
    }

    void Update()
    {

        Vector3 mousePosition = _mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.y += yOffset;
        mousePosition.z += zOffset;
        transform.position = Vector3.Lerp(transform.position, mousePosition, 5f);
        if (Input.GetMouseButtonDown(0))
        {
            _animator.Play("handanim");
        }
    }
}