using System.Collections.Generic;
using UnityEngine;

/// <summary>Object1: hold Space to choose an angle, release to launch.</summary>
public class RotateShootReturn2D : MonoBehaviour
{
    [Header("Aiming")]
    [SerializeField] private float rotateSpeed = 130f;
    [SerializeField] private float minimumAngle = -90f;
    [SerializeField] private float maximumAngle = 90f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float moveDistance = 15f;
    [SerializeField] private float returnSpeed = 50f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float currentAngle;
    private bool clockwise = true;
    private bool aiming;
    private bool moving;
    private bool returning;
    private readonly HashSet<int> hitObject3ThisShot = new HashSet<int>();

    private KeyCode ControlKey => StartMenuController.Instance != null
        ? StartMenuController.Instance.ControlKey
        : KeyCode.Space;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (StartMenuController.Instance == null || !StartMenuController.Instance.IsPlaying)
        {
            return;
        }

        if (!moving && !returning)
        {
            UpdateAimInput();
        }
        else if (moving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, targetPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                moving = false;
                returning = true;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position, startPosition, returnSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, startPosition) < 0.05f)
            {
                ResetAtStart();
            }
        }
    }

    private void UpdateAimInput()
    {
        if (Input.GetKeyDown(ControlKey))
        {
            aiming = true;
        }

        if (aiming && Input.GetKey(ControlKey))
        {
            currentAngle += (clockwise ? 1f : -1f) * rotateSpeed * Time.deltaTime;
            if (currentAngle >= maximumAngle)
            {
                currentAngle = maximumAngle;
                clockwise = false;
            }
            else if (currentAngle <= minimumAngle)
            {
                currentAngle = minimumAngle;
                clockwise = true;
            }

            transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        }

        if (aiming && Input.GetKeyUp(ControlKey))
        {
            aiming = false;
            moving = true;
            hitObject3ThisShot.Clear();
            targetPosition = transform.position + transform.up * moveDistance;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((!moving && !returning) || StartMenuController.Instance == null || !StartMenuController.Instance.IsPlaying)
        {
            return;
        }

        RandomSnailMove object2 = other.GetComponentInParent<RandomSnailMove>();
        if (object2 != null)
        {
           // object2.DestroyByObject1();
            ResetAtStart();
            return;
        }

        Object3Target object3 = other.GetComponentInParent<Object3Target>();
        if (object3 != null && hitObject3ThisShot.Add(object3.GetInstanceID()))
        {
            object3.RegisterHit(this);
        }
    }

    private void ResetAtStart()
    {
        transform.position = startPosition;
        aiming = false;
        moving = false;
        returning = false;
    }
}
