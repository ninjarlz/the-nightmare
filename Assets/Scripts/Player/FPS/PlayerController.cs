﻿using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : NetworkBehaviour {
    [SerializeField] public float _speed = 3f;
    [SerializeField] private float _lookSensitivity = 3f;
    [SerializeField] private Joystick move;
    [SerializeField] private Joystick look;

    private PlayerMotor _motor;
    private static readonly int IsSprinting = Animator.StringToHash("isSprinting");

    void Start() {
        _motor = GetComponent<PlayerMotor>();

#if UNITY_ANDROID
        move = GameObject.Find("Move").GetComponent<Joystick>();
        look = GameObject.Find("Look").GetComponent<Joystick>();
#endif
    }


    private void Update() {

        if (transform.GetComponent<PlayerEquipment>().Weapon.GetComponent<Animator>().GetBool(IsSprinting)) 
            _speed = 6.5f;
        else  
            _speed = 3f;
        
        
        if (!PauseGame.menuActive) {
            float xMov = 0;
            float zMov = 0;

#if UNITY_ANDROID
            if(Mathf.Abs(move.Horizontal) >= 0.2)
                xMov = move.Horizontal;
            if(Mathf.Abs(move.Vertical) >= 0.2)
                zMov = move.Vertical;
#endif

#if UNITY_STANDALONE
            xMov = Input.GetAxisRaw("Horizontal");
            zMov = Input.GetAxisRaw("Vertical");
#endif

            Vector3 moveHorizontal = transform.right * xMov;
            Vector3 moveVertical = transform.forward * zMov;

            Vector3 velocity = (moveHorizontal + moveVertical).normalized * _speed;

            _motor.Move(velocity);

            float yRot = 0;
            float xRot = 0;

#if UNITY_ANDROID
            if(Mathf.Abs(look.Horizontal) >= 0.2)
                yRot = look.Horizontal;
            if(Mathf.Abs(look.Horizontal) >= 0.2)
                xRot = look.Vertical;
#endif

#if UNITY_STANDALONE
            yRot = Input.GetAxisRaw("Mouse X");
            xRot = Input.GetAxis("Mouse Y");
#endif

            Vector3 rotation = new Vector3(0f, yRot, 0f) * _lookSensitivity;
            _motor.Rotate(rotation);

            float cameraRotationX = xRot * _lookSensitivity;
            _motor.RotateCamera(cameraRotationX);


            Cursor.lockState = CursorLockMode.Locked;
        }

        else {
            _motor.Move(Vector3.zero);
            _motor.Rotate(Vector3.zero);
            _motor.RotateCamera(0f);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}