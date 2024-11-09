using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [SerializeField] private Transform _player;

    void Start()
    {
        Camera minimapCamera = GetComponent<Camera>();
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = Color.black;
    }

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        transform.position = new Vector3(_player.position.x, _player.position.y, transform.position.z);
    }
}
