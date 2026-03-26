using Palmmedia.ReportGenerator.Core;
using Unity.VisualScripting;
using UnityEngine;

public class Pivot : MonoBehaviour
{
    #region UI

    [Space]

    [SerializeField]
    private bool _active = true;

    [Space]

    [SerializeField]
    private bool _enableRotation = true;

    [SerializeField]
    private float _mouseSense = 1.5f;

    [Space]

    [SerializeField]
    private bool _enableMovement = true;

    [SerializeField]
    private float _movementSpeed = 10f;

    [SerializeField]
    private float _boostedSpeed = 50f;

    [Space]

    [SerializeField]
    private bool _enableSpeedAcceleration = true;

    [SerializeField]
    private float _speedAccelerationFactor = 1.5f;

    [Space]

    [SerializeField]
    private KeyCode _initPositonButton = KeyCode.R;
    [SerializeField]
    private Transform target;

    #endregion UI

    private CursorLockMode _wantedMode;

    private float _currentIncrease = 1;
    private float _currentIncreaseMem = 0;

    private Vector3 _initPosition;
    private Vector3 _initRotation;

    private float xRotation = 0f;

    private float yRotation = 0f;

    private Vector3 offset = new(0, 1.33f, 0);

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_boostedSpeed < _movementSpeed)
            _boostedSpeed = _movementSpeed;
    }
#endif


    private void Start()
    {
        _initPosition = transform.position;
        _initRotation = transform.eulerAngles;
    }

    private void OnEnable()
    {
        if (_active)
            _wantedMode = CursorLockMode.Locked;
    }

    // Apply requested cursor state
    private void SetCursorState()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = _wantedMode = CursorLockMode.None;
        }

        if (Input.GetMouseButtonDown(0))
        {
            _wantedMode = CursorLockMode.Locked;
        }

        // Apply cursor state
        Cursor.lockState = _wantedMode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != _wantedMode);
    }

    private void CalculateCurrentIncrease(bool moving)
    {
        _currentIncrease = Time.deltaTime;

        if (!_enableSpeedAcceleration || _enableSpeedAcceleration && !moving)
        {
            _currentIncreaseMem = 0;
            return;
        }

        _currentIncreaseMem += Time.deltaTime * (_speedAccelerationFactor - 1);
        _currentIncrease = Time.deltaTime + Mathf.Pow(_currentIncreaseMem, 3) * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (!_active) return;

        SetCursorState();

        if (Cursor.visible) return;

        transform.position = target.position + offset;

        // Movement
        if (_enableMovement)
        {
            Vector3 deltaPosition = Vector3.zero;
            float currentSpeed = _movementSpeed;

            // Calc acceleration
            CalculateCurrentIncrease(deltaPosition != Vector3.zero);

            transform.position += _currentIncrease * currentSpeed * deltaPosition;
        }
        // Rotation
        if (_enableRotation)
        {
            // Pitch
            xRotation -= -Input.GetAxis("Mouse Y") * _mouseSense / 5;

            xRotation = Mathf.Clamp(xRotation, -75f, 75f);

            yRotation += Input.GetAxis("Mouse X") * _mouseSense; 

            if (Input.GetMouseButton(1)) transform.localRotation = Quaternion.Euler(-xRotation, yRotation, 0f);
            
            else transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            // Yaw
        }

        if (Input.GetKey(KeyCode.C))
        {
            transform.Rotate(0, 180, 0);
        }

        // Return to init position
        if (Input.GetKeyDown(_initPositonButton))
        {
            transform.position = _initPosition;
            transform.eulerAngles = _initRotation;
        }
    }
}
