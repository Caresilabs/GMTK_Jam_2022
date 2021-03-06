using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PlayerState
{
    WALKING,
    ROLLING,
    GOAL,
}

public class PlayerManager : MonoBehaviour
{

    public static float LevelTime = 0f;

    [SerializeField]
    private float Speed = 10;

    [SerializeField]
    private float MaxVelocityChange = 2;

    [SerializeField]
    private float JumpVelocity = 10;

    [SerializeField]
    private float RotationForce = 60f;

    [SerializeField]
    private float BoostJumpVelocity = 10f;

    [SerializeField]
    private float JumpRandomPower = 2.5f;

    [SerializeField]
    private float[] BoostHeightByVal;

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

    private Vector3 Spawn;
    private Vector3 DiceDefaultScale;


    [Header("Audio")]
    public AudioClip JumpSound;
    public AudioClip BoostSound;
    public AudioClip WinSound;

    // Start is called before the first frame update
    void Start()
    {
        cam = FindObjectOfType<Camera>();
        Rigidbody.centerOfMass = Dice.localPosition;

        Spawn = transform.position;
        DiceDefaultScale = Dice.localScale;

        LevelTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (State == PlayerState.GOAL)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GlobalManager.Instance.NextLevel();
            }
            return;
        }

        LevelTime += Time.deltaTime;
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            if (State == (PlayerState.ROLLING))
            {
                // TryBoost();
            }
            else
            {
                StartRolling();
            }
        }


        if (State == (PlayerState.ROLLING))
        {
            Vector3 torque = new Vector3(Input.GetAxis("VerticalSpin"), 0, -Input.GetAxis("HorizontalSpin"));
            Rigidbody.AddTorque(cam.transform.TransformDirection(torque) * RotationForce * Time.deltaTime, ForceMode.VelocityChange);
        }
        else if (State == (PlayerState.WALKING))
        {
            //transform.Rotate(cam.transform.forward, 2f);
            transform.localScale = Vector3.one + (Vector3.right * Mathf.Sin(Time.timeSinceLevelLoad * 10f) * 0.05f);
        }
       

        Dice.localScale = Vector3.Lerp(Dice.localScale, DiceDefaultScale, Time.deltaTime * 15f);

    }

    private void LateUpdate()
    {

    }

    public void EnterGoal()
    {
        if (SetState(PlayerState.GOAL))
        {
            GameObject.Find("GameCanvas").transform.GetChild(1).gameObject.SetActive(true);
            GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>().text = $"Time: {(int)LevelTime} sec";
            GlobalManager.Instance.SetTime(LevelTime);
            GameObject.Find("HUD").SetActive(false);
            GlobalManager.PlaySFX(WinSound);
        }
    }

    void StartRolling()
    {
        if (SetState(PlayerState.ROLLING))
        {
            transform.localScale = Vector3.one;
            Legs.gameObject.SetActive(false);
            Rigidbody.constraints = RigidbodyConstraints.None;

            Rigidbody.AddForce(new Vector3(0, JumpVelocity, 0) + Legs.forward * JumpVelocity * 0.5f, ForceMode.Impulse);

            var randomTorque = Random.insideUnitCircle * JumpRandomPower;
            Rigidbody.AddTorque(new Vector3(randomTorque.x, 0, randomTorque.y), ForceMode.Impulse);
            isGrounded = false;
            GlobalManager.PlaySFX(JumpSound);

            Dice.localScale = DiceDefaultScale * 1.35f;
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

            Legs.rotation = Quaternion.identity * Quaternion.Euler(0, 90f, 0);
            Legs.gameObject.SetActive(true);

            Dice.localScale = DiceDefaultScale * 0.8f;

        }
    }

    void TryBoost()
    {
        if (isGrounded)
        {
            var boostDir = cam.transform.forward;
            boostDir.y = 0f;
            boostDir.Normalize();

            var side = DetectSideUp();

            var velocity = Rigidbody.velocity;
            velocity.y = BoostHeightByVal[side];
            Rigidbody.velocity = velocity;

            Rigidbody.AddForce(boostDir * BoostJumpVelocity, ForceMode.Impulse);

            Debug.Log("Boost " + side);
            GlobalManager.PlaySFX(BoostSound);

            Dice.localScale = DiceDefaultScale * (1 + (0.8f* side / 6f));
        }

    }

    public void Kill()
    {
        transform.position = Spawn;
        Rigidbody.velocity = Vector3.zero;
        State = PlayerState.ROLLING;
        StartWalking();
    }

    bool SetState(PlayerState newState)
    {
        if (newState == State || State == PlayerState.GOAL)
        {
            return false;
        }

        State = newState;

        return true;
    }


    void FixedUpdate()
    {
        if (State == PlayerState.GOAL)
        {
            return;
        }

        Vector3 velocity = Rigidbody.velocity;
        Vector3 angularVelocity = Rigidbody.angularVelocity;

        if (isGrounded && State == PlayerState.ROLLING && angularVelocity.magnitude < 0.35f) //&& velocity.y < 0.4f
        {
            Debug.Log("WALK START");

            StartWalking();
            TryBoost();
        }

        CheckGrounded();


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

        var change = isGrounded && State == PlayerState.WALKING ? MaxVelocityChange : MaxVelocityChange * 0.2f;
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

        //  Debug.Log(velocityChange);
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
            transform.Rotate(Vector3.up, Quaternion.LookRotation(velocity).eulerAngles.y - Legs.rotation.eulerAngles.y, Space.World);
            //
        }

        var lookAt = Rigidbody.velocity;
        if (change != 0 && lookAt.magnitude > 5f)
        {
            //lookAt.x = 0;
            //lookAt.z = 0;

            CamTarget.rotation = Quaternion.Lerp(CamTarget.rotation, Quaternion.Euler(0, Quaternion.LookRotation(lookAt).eulerAngles.y, 0), Time.fixedDeltaTime * 2f);
        }

        //  Rigidbody.MovePosition(transform.position + targetVelocity);

        // Debug.Log(velocityChange);


        // We apply gravity manually for more tuning control
        //Rigidbody.AddForce(gravities.Values.Where(x => x != Vector3.zero).Last(), ForceMode.Acceleration);
    }

    void CheckGrounded()
    {
        var colliderRadius = 0.5f + (State == PlayerState.ROLLING ? 0.23f : 0.5f);

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Death"))
            Kill();
        //else
         //   Dice.localScale = Vector3.one * 0.9f;
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

        var normal = Vector3.up;

        if (Vector3.Cross(normal, transform.right).magnitude < 0.5f) //x axis
        {
            if (Vector3.Dot(normal, transform.right) > 0)
            {
                return 4;
            }
            else
            {
                return 3;
            }
        }
        else if (Vector3.Cross(normal, transform.up).magnitude < 0.5f) //y axis
        {
            if (Vector3.Dot(normal, transform.up) > 0)
            {
                return 1;
            }
            else
            {
                return 6;
            }
        }
        else if (Vector3.Cross(normal, transform.forward).magnitude < 0.5f) //z axis
        {
            if (Vector3.Dot(normal, transform.forward) > 0)
            {
                return 5;
            }
            else
            {
                return 2;
            }
        }

        return 0;


    }
}
