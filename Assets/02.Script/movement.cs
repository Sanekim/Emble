﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

public class movement : MonoBehaviour
{
    // 스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    // 점프 높이 변수
    [SerializeField]
    private float jumpForce;


    // 상태 변수
    private bool isRun = false;
    private bool isGround = true;
    private bool isCrouch = false;

    // 땅 착지 여부
    private CapsuleCollider capsuleCollider;

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // 민감도 조정 변수 
    [SerializeField]
    private float lookSensitivity;


    // 카메라 한계
    [SerializeField]
    private float cameraRotationLimit;
    private float currentCameraRotationX;

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid;

    //아이템 집을 수 있는 범위내
    bool isInRange = false;
    bool isGrab = false;

    //현재 잡고 있는 오브젝트 
    GameObject grabObject;
    public GameObject[] robots;
    public Image ui;


    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed;

        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;

    }

    // Update is called once per frame
    void Update()
    {
        TryCrouch();
        IsGround();
        TryJump();
        TryRun();
        Move();
        CameraRotation();
        CharacterRotation();
        TryCatch();
        SetUi();
    }

    private void FixedUpdate() {
        myRigid.angularVelocity = Vector3.zero;
    }

    // 앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    private void TryCatch() {

        bool isPressButton = Input.GetButtonDown("Grab");

        if (isPressButton) {
            if (isGrab) {
                robots[grabObject.layer - 8].SetActive(false);
                grabObject.transform.position = theCamera.transform.position;
                grabObject.SetActive(true);
                isGrab = false;
            }
            else if (!isGrab) {
                if(isLookAtRobot() && isInRange) {
                    grabObject.SetActive(false);
                    robots[grabObject.layer - 8].SetActive(true);
                    isGrab = true;
                }
            }
        }
       
    }


    private bool isLookAtRobot() {
        RaycastHit hit;
        if (Physics.Raycast(theCamera.transform.position, theCamera.transform.forward, out hit, 15)) {
            if (hit.collider.tag == "robot") {
                grabObject = hit.collider.gameObject;
                return true;
            } else {
                return false;
            }
        }
        return false;
    }

    private void SetUi() {
        RaycastHit hit;
        if (Physics.Raycast(theCamera.transform.position, theCamera.transform.forward, out hit, 15)) {
            if (hit.collider.tag == "robot" && isInRange && !isGrab) {
                ui.gameObject.SetActive(true);
            } else {
                ui.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "robot") {
            isInRange = true;
        } else 
        {
            isInRange = false;
        }
    }


    // 앉기
    private void Crouch()
    {
        isCrouch = !isCrouch;

        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        StartCoroutine(CrouchCoroutine());
    }

    // 부드럽게 앉기
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.2f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);

            if (count > 15)
                break;

            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }

    // 땅에 닿았는지 여부 확인
    private void IsGround()
    {
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    // 점프 시도
    private void TryJump()
    {
        if (Input.GetKey(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    // 점프
    private void Jump()
    {
        // 앉은 상태에서 점프 시 앉기 해제
        if (isCrouch)
        {
            Crouch();
        }

        myRigid.velocity = transform.up * jumpForce;
    }


    // 달리기 시도
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (isCrouch == false)
            {
                Running();
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    // 달리기
    private void Running()
    {
        isRun = true;
        applySpeed = runSpeed;

    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;

    }

    // 움직임 실행
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    // 좌우 캐릭터 회전
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }

    // 상하 카메라 회전
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        float _cameraRotationX = _xRotation * lookSensitivity;

        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
}
