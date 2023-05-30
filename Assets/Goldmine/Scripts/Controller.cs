using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour
{

    public float moveSpeed = 4.5f;
    public float gravity = 9.81f;
    public bool isFrozen = false; // when true, player is prevented from moving

    private CharacterController myController;

    void Awake()
    {
        // store a reference to the CharacterController component on this gameObject
        // it is much more efficient to use GetComponent() once in Start and store
        // the result rather than continually use etComponent() in the Update function
        myController = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!isFrozen)
        {
            // Determine how much should move in the z-direction
            Vector3 movementZ = Input.GetAxis("Vertical") * Vector3.forward * moveSpeed * Time.deltaTime;

            // Determine how much should move in the x-direction
            Vector3 movementX = Input.GetAxis("Horizontal") * Vector3.right * moveSpeed * Time.deltaTime;

            // Convert combined Vector3 from local space to world space based on the position of the current gameobject (player)
            Vector3 movement = transform.TransformDirection(movementZ + movementX);

            // Apply gravity (so the object will fall if not grounded)
            movement.y -= gravity * Time.deltaTime;

            //Debug.Log("Movement Vector = " + movement);

            // Actually move the character controller in the movement direction
            myController.Move(movement);
        }
    }


    public void Freeze(bool frozen)
    {
        isFrozen = frozen;

    }
}

//public class Controller : MonoBehaviour
//{

//    // public variables
//    public float maxSpeed = 5f;
//    public float minSpeedRatio = 0.2f;
//    public Vector3 currentVelocity = new Vector3(0, 0, 0);
//    public float acceleration = 0.02f;
//    public float deceleration = 0.12f;
//    public float smoothTime = 2;
//    public float gravity = 9.81f;
//    public bool isFrozen = false; // when true, player is prevented from moving

//    private CharacterController myController;

//    // Use this for initialization
//    void Start()
//    {
//        // store a reference to the CharacterController component on this gameObject
//        // it is much more efficient to use GetComponent() once in Start and store
//        // the result rather than continually use etComponent() in the Update function
//        myController = gameObject.GetComponent<CharacterController>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (!isFrozen)
//        {
//            // Determine how much should move in the z-direction
//            Vector3 movementZ = Input.GetAxis("Vertical") * Vector3.forward;

//            // Determine how much should move in the x-direction
//            Vector3 movementX = Input.GetAxis("Horizontal") * Vector3.right;

//            Vector3 newVelocityMax = Vector3.Normalize(movementX + movementZ) * maxSpeed * Time.deltaTime;

//            if (newVelocityMax.sqrMagnitude > 0)
//            {
//                currentVelocity = Vector3.Slerp(currentVelocity, newVelocityMax, acceleration);
//                //Debug.Log(currentVelocity);
//                //Debug.Log(newVelocityMax);

//                //if ((currentVelocity.x > 0) && (currentVelocity.x < newVelocityMax.x * minSpeedRatio))

//                //    currentVelocity.x = newVelocityMax.x * minSpeedRatio;

//                //if ((currentVelocity.z > 0) && (currentVelocity.z < newVelocityMax.z * minSpeedRatio))

//                    //currentVelocity.z = newVelocityMax.z * minSpeedRatio;
//            }
//            else
//            {
//                currentVelocity = Vector3.Slerp(currentVelocity, newVelocityMax, deceleration);
//            }

//            // Convert combined Vector3 from local space to world space based on the position of the current gameobject (player)
//            Vector3 movement = transform.TransformDirection(currentVelocity);

//            // Apply gravity (so the object will fall if not grounded)
//            movement.y -= gravity * Time.deltaTime;

//            Debug.Log("Movement Vector = " + currentVelocity);

//            // Actually move the character controller in the movement direction
//            myController.Move(movement);
//        }
//    }

//    public void Freeze(bool frozen)
//    {
//        isFrozen = frozen;
//    }
//}
