using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{

    WALKING,
    ROLLING,
}

public class PlayerManager : MonoBehaviour
{

    [SerializeField]
    private float Speed = 10;

    [SerializeField]
    private float MaxVelocityChange = 2;

    [SerializeField]
    private float JumpVelocity = 9;


    [SerializeField]
    private Rigidbody Rigidbody;


    [SerializeField]
    private Transform Legs;

    [SerializeField]
    private Transform Dice;

    [SerializeField]
    private Transform CamTarget;

    public PlayerState State { get; set; } = PlayerState.WALKING;

    private Camera cam;

    private bool isGrounded = true;
    private Vector3 groundNormal;

    // Start is called before the first frame update
    void Start()
    {
        cam = FindObjectOfType<Camera>();
        Rigidbody.centerOfMass = Dice.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            if (State == (PlayerState.ROLLING))
            {
                TryBoost();
            }
            else
            {
                StartRolling();
            }
        }

    }

    private void LateUpdate()
    {

    }

    void StartRolling()
    {
        if (SetState(PlayerState.ROLLING))
        {
            Legs.gameObject.SetActive(false);
            Rigidbody.constraints = RigidbodyConstraints.None;

            Rigidbody.AddForce(new Vector3(0, 10f, 0), ForceMode.Impulse);

            var randomTorque = Random.insideUnitCircle * 3f;
            Rigidbody.AddTorque(new Vector3(10 + randomTorque.x, 0, randomTorque.y), ForceMode.Impulse);
            isGrounded = false;
        }

    }

    void StartWalking()
    {
        if (SetState(PlayerState.WALKING))
        {
            Debug.Log(DetectSideUp());

            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            Rigidbody.angularVelocity = Vector3.zero;
            Snap90Deg();

            Legs.rotation = Quaternion.identity;
            Legs.gameObject.SetActive(true);
        }
    }

    void TryBoost()
    {
        if (isGrounded)
        {
            var boostDir = cam.transform.forward;
            boostDir.y = 0f;
            boostDir.Normalize();

            var BoostPower = 13f;

            Rigidbody.AddForce(boostDir * BoostPower + Vector3.up * DetectSideUp() * 2, ForceMode.Impulse);
            Debug.Log("Boost");
        }

    }

    bool SetState(PlayerState newState)
    {
        if (newState == State)
        {
            return false;
        }

        State = newState;

        return true;
    }


    void FixedUpdate()
    {
        CheckGrounded();

        Vector3 velocity = Rigidbody.velocity;
        Vector3 angularVelocity = Rigidbody.angularVelocity;

        if (isGrounded && State == PlayerState.ROLLING && angularVelocity.magnitude < 0.2f && velocity.y < 0.2f)
        {
            Debug.Log("WALK START");

            StartWalking();
        }


        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (targetVelocity.magnitude >= 1)
        {
            targetVelocity.Normalize();
        }

        // IsSwimming = true; // debug

        targetVelocity = cam.transform.TransformDirection(targetVelocity);
        // targetVelocity.y = IsSwimming ? targetVelocity.y : 0;

        float targetSpeed = Speed;


        targetVelocity *= targetSpeed;

        // TODO fix lookdown slow speed

        var localTargetVelocity = CamTarget.InverseTransformDirection(targetVelocity);
        localTargetVelocity.y = 0;


        var transformedVelocity = CamTarget.InverseTransformDirection(velocity);

        var change = isGrounded ? MaxVelocityChange : MaxVelocityChange * 0.3f;
        if (targetVelocity == Vector3.zero)
        {
            change = 0f;// MaxVelocityChange * (isGrounded ? 0.5f : 0);
        }

        // Apply a force that attempts to reach our target velocity
        Vector3 velocityChange = (localTargetVelocity - transformedVelocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -change, change);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -change, change);
        velocityChange.y = 0;

        var transformedVelocityChange = CamTarget.TransformDirection(velocityChange);

        //if (groundNormal != Vector3.zero && Vector3.Angle(groundNormal, transformedVelocityChange) - 90 > MaxSlopeAngle)
        // {
        //    transformedVelocityChange = Vector3.zero;
        //}

        transformedVelocityChange.y = 0;

        Rigidbody.AddForce(transformedVelocityChange, ForceMode.VelocityChange);


        if (State == PlayerState.WALKING && velocity.magnitude > 2f)
        {
            //Vector3 forwardsVector = Rigidbody.velocity.normalized;
            //  transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forwardsVector, Vector3.up) * Quaternion.Euler(90, 0, 0), Time.deltaTime * 30);
            //
        }

        var lookAt = Rigidbody.velocity;
        if (change != 0 && lookAt.magnitude > 5f)
        {
            //lookAt.x = 0;
            //lookAt.z = 0;

            CamTarget.rotation = Quaternion.Euler(0, Quaternion.LookRotation(lookAt).eulerAngles.y, 0);
        }

        //  Rigidbody.MovePosition(transform.position + targetVelocity);

        // Debug.Log(velocityChange);


        // We apply gravity manually for more tuning control
        //Rigidbody.AddForce(gravities.Values.Where(x => x != Vector3.zero).Last(), ForceMode.Acceleration);
    }

    void CheckGrounded()
    {
        var colliderRadius = 0.5f + (State == PlayerState.ROLLING ? 0 : 0.3f);

        RaycastHit groundInfo;
        bool hit = Physics.SphereCast(transform.position, 0.1f,
            Vector3.down, out groundInfo, colliderRadius + .1f);

        if (!hit)
        {
            isGrounded = false;
            return;
        }

        isGrounded = groundInfo.distance < colliderRadius + 0.1f;
        groundNormal = groundInfo.normal;

        //Debug.Log(groundInfo.transform.gameObject.name);

        // TODO change to slope angle code 
        if (isGrounded && Rigidbody.velocity.y <= 0.02f && Vector3.Dot(groundInfo.normal, Vector3.up) > 0.75f)
        {
            //isGrounded = true;

        }
        else
        {
            // isGrounded = false;
        }
    }


    void Snap90Deg()
    {
        var vec = transform.eulerAngles;
        vec.x = Mathf.Round(vec.x / 90) * 90;
        vec.y = Mathf.Round(vec.y / 90) * 90;
        vec.z = Mathf.Round(vec.z / 90) * 90;
        transform.eulerAngles = vec;
    }

    int DetectSideUp()
    {

        if (Vector3.Cross(groundNormal, transform.right).magnitude < 0.5f) //x axis a.b.sin theta <45
                                                                         //if ((int) Vector3.Cross(Vector3.up, transform.right).magnitude == 0) //Previously
        {
            if (Vector3.Dot(groundNormal, transform.right) > 0)
            {
                return 2;
            }
            else
            {
                return 3; // FaceRepresent[OpposingDirectionValues.x];
            }
        }
        else if (Vector3.Cross(groundNormal, transform.up).magnitude < 0.5f) //y axis
        {
            if (Vector3.Dot(groundNormal, transform.up) > 0)
            {
                return 4;
            }
            else
            {
                return 2;// FaceRepresent[OpposingDirectionValues.y];
            }
        }
        else if (Vector3.Cross(groundNormal, transform.forward).magnitude < 0.5f) //z axis
        {
            if (Vector3.Dot(groundNormal, transform.forward) > 0)
            {
                return 1;
            }
            else
            {
                return 6;// FaceRepresent[OpposingDirectionValues.z];
            }
        }

        return 0;

       
    }
}
