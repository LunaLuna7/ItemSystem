using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Item;
using Sirenix.OdinInspector;

namespace Player
{
    //-////////////////////////////////////////////////////
    ///
    /// All the logic for the character controller states and logic is in here!
    ///
    public partial class StateManager : MonoBehaviour
    {
        [SerializeField]
        public StateManager partner;
        public PlayerStats playerStats;

        [System.NonSerialized]
        public float horizontal;
        [System.NonSerialized]
        public float vertical;
        [System.NonSerialized]
        public float moveAmount;
        [System.NonSerialized]
        public Vector3 moveDir;

        [SerializeField]
        private float distanceToGround = 0.5f;

        [SerializeField, Tooltip("How fast player goes down in a jump/fall")]
        private float fallSpeed;

        [System.NonSerialized]
        public float airJumpsTracker;

        public enum PlayerStates
        {
            IDLE,
            MOVING,
            HOLD,
            DEAD
        }

        [Tooltip("Players current game state")]
        public PlayerStates playerState;

        [TabGroup("States"), Tooltip("Player is touching the ground")]
        public bool onGround = true;
        [TabGroup("States")]
        public bool obstacleForward = false;
        [SerializeField, TabGroup("States")]
        private bool groundForward = false;

        [TabGroup("States"), SerializeField, Tooltip("player is in the act of jumping")]
        private bool jumping = false;

        [TabGroup("States"), SerializeField, Tooltip("Player can not be damaged")]
        private bool immune;

        [SerializeField, TabGroup("States")]
        private bool hasReachedMaxVel = false;
        
        public delegate bool SkillDelegate();

        
        [System.NonSerialized]
        public float delta;
        private float groundAngle;
        public float angleFloor{ get; private set; }
        private RaycastHit hit;

        [TabGroup("Components")]
        public Animator anim;
        [TabGroup("Components")]
        public Rigidbody rBody;
        [TabGroup("Components")]
        public LayerMask groundLayers;
        [TabGroup("Components")]
        public Collider col;
        [TabGroup("Components")]
        public PhysicMaterial minimumFric;
        [TabGroup("Components")]
        public PhysicMaterial averFric;


        [System.NonSerialized]
        public Vector3 targetVelocity = Vector3.zero;
        [System.NonSerialized]
        public Quaternion targetRotation = Quaternion.identity;
        [System.NonSerialized]
        public Vector3 targetDir = Vector3.zero;
        [System.NonSerialized]
        public Quaternion tr = Quaternion.identity;

        [TabGroup("Abilities")]
        public CharacterInventory charInventory;
        [TabGroup("Abilities")]
        public bool changingInventory;
        [TabGroup("Abilities")]
        public Transform shockSlashOrigin;
        [TabGroup("Abilities"), SerializeField]
        private GameObject vortexEffect;
        [TabGroup("Abilities")]
        private GameObject vortexInstance;

        //~////////////////////////////////////////////////////
        //
        public void Init()
        {
            SetUpAnimator();
            rBody = GetComponent<Rigidbody>();
            rBody.angularDrag = 999f;
            rBody.drag = 4;
            rBody.constraints = RigidbodyConstraints.FreezeRotation;

            if (col == null)
            {
                col = GetComponent<Collider>();
            }
        }

        //~////////////////////////////////////////////////////
        //
        public void OnUpdate(float d)
        {
            delta = d;
            obstacleForward = false;
            groundForward = false;
            onGround = UpdateOnGround();

            if (onGround)
            {
                col.material = averFric;
                // Use forward checking for dashing on slope which we only care on ground
                UpdateObstacleAndGroundForward();
                
                if (angleFloor < 71)
                {
                    airJumpsTracker = playerStats.currentAirJumps;
                }
            }
            else
            {
                col.material = minimumFric;
            }

            UpdateStoppingState();
            UpdateArsenalSkills();
            UpdatePlayerState();
        }

        ///-////////////////////////////////////////////////////
        ///
        /// All the logic that alternates physics
        ///
        public void OnFixedUpdate(float d)
        {
            delta = d;
            HandleMovement();
        }

        //-////////////////////////////////////////////////////
        //
        private void SetUpAnimator()
        {
            //if (activeModel == null)
            //{
            //Debug.LogError("Need a model game object");
            //}
            //anim = activeModel.GetComponent<Animator>();
        }

        //-////////////////////////////////////////////////////
        ///
        /// Updates all the physics related movement
        ///
        public void HandleMovement()
        {
            float yVelocity = rBody.velocity.y;
            HandleGravity(ref yVelocity);
            
            targetVelocity = new Vector3(0, yVelocity, 0);

            // we return to prevent moving
            if (playerState != PlayerStates.HOLD && playerState != PlayerStates.DEAD)
            {
                float targetSpeed = playerStats.currentSpeed; //runSpeed.Value;
                HandleStoppingState();
                
                float xVelocity = moveDir.x * (targetSpeed * moveAmount);
                float zVelocity = moveDir.z * (targetSpeed * moveAmount);

                targetVelocity = new Vector3(xVelocity, yVelocity, zVelocity);
                targetDir = moveDir;
                tr = Quaternion.LookRotation(targetDir);
                targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * playerStats.currentTurnSpeed);
                HandleArsenalSkills();

                HandleSlopeGlide();
            }

            rBody.velocity = targetVelocity;
            transform.rotation = targetRotation;
        }

        //-////////////////////////////////////////////////////
        ///
        /// Enhances control on player stopping and start up velocity
        ///
        private void HandleStoppingState()
        {
            if (playerState == PlayerStates.IDLE)
            {
                hasReachedMaxVel = false;
            }
            else if (moveAmount >= 1f)
            {
                hasReachedMaxVel = true;
            }

            // Provides great precision on running by making a quick stop in movement once player lets go of running trigger
            if (playerState == PlayerStates.MOVING && hasReachedMaxVel && onGround)
            {
                if (moveAmount > 0f && moveAmount < .75f)
                {
                    moveAmount = 0f;
                }
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Used every frame to know the characters locomotion
        /// and limitation states
        ///
        public void UpdatePlayerState()
        {
            if (changingInventory)
            {
                playerState = PlayerStates.HOLD;
            }
            // Player is Idle
            else if (moveDir == Vector3.zero)
            {
                playerState = PlayerStates.IDLE;
            }
            // Player is moving
            else if(moveDir != Vector3.zero)
            {
                playerState = PlayerStates.MOVING;
            }

            // Add player is Dead
        }

        //-////////////////////////////////////////////////////
        ///
        private void UpdateStoppingState()
        {
            if (playerState == PlayerStates.IDLE)
            {
                hasReachedMaxVel = false;
            }
            else if (moveAmount >= 1f)
            {
                hasReachedMaxVel = true;
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Checks if we are grounded and updates players position
        ///
        private bool UpdateOnGround()
        {
            bool r = false;

            Vector3 origin = transform.position + (Vector3.up * distanceToGround);
            hit = new RaycastHit();
            bool isHit = false;

            FindGround(origin, ref hit, ref isHit);

            // If our center raycast doesnt hit ground then we create 4 more around the player
            // to check around the player if they are barely touching a floor edge
            if (isHit == false)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 newOrigin = origin;
                    switch (i)
                    {
                        case 0: //front
                            newOrigin += Vector3.forward / 3;
                            break;
                        case 1: //back
                            newOrigin -= Vector3.forward / 3;
                            break;
                        case 2: //right
                            newOrigin += Vector3.right / 3;
                            break;
                        case 3: //left
                            newOrigin -= Vector3.right / 3;
                            break;
                        default:
                            Debug.LogError("switch given wrong case parameter");
                            break;
                    }

                    FindGround(newOrigin, ref hit, ref isHit);

                    if (isHit)
                    {
                        break;
                    }
                }
            }

            r = isHit;

            if (r != false)
            {
                // We do this so that the player is put on ground relative to the raycast hit
                // This is done so we can use an airbound collider rather than it touching the floor
                // and therefore does not get on the way of crazy ground shapes 
                Vector3 targetPosition = transform.position;
                targetPosition.y = hit.point.y;
                transform.position = targetPosition;
            }

            return r;
        }

        //-////////////////////////////////////////////////////
        ///
        /// Checks if there is colliders infront and ahead of us
        /// to give more control for specific actions that require
        /// to behave different on going up or down slopes
        ///
        private void UpdateObstacleAndGroundForward()
        {
            Vector3 origin = transform.position;
            origin += Vector3.up * 0.75f;

            IsClear(origin, transform.forward, 2.5f, ref obstacleForward);

            if (obstacleForward == false)
            {
                origin += transform.forward * .6f;
                IsClear(origin, -Vector3.up, distanceToGround * 12, ref groundForward);
            }
            else
            {
                if (Vector3.Angle(transform.forward, moveDir) > 30)
                {
                    obstacleForward = false;
                }
            }
        }

        //-////////////////////////////////////////////////////
        ///
        private void HandleGravity(ref float yVelocity)
        {
            if (onGround == false && jumping == false)
            {
                yVelocity = rBody.velocity.y - fallSpeed;
            }
        }

        //-////////////////////////////////////////////////////
        ///
        private void HandleSlopeGlide()
        {
            if (onGround && angleFloor > 70)
            {
                targetVelocity.y -= fallSpeed * 2;
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Helper method to check raycast hit ground
        ///
        public void FindGround(Vector3 origin, ref RaycastHit hit, ref bool isHit)
        {
            Vector3 dir = -Vector3.up;
            float dis = distanceToGround + 0.5f;

            Debug.DrawRay(origin, dir * dis, Color.red);

            if (Physics.Raycast(origin, dir, out hit, dis, groundLayers))
            {
                angleFloor = Vector3.Angle(Vector3.up, hit.normal);
                isHit = true;
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Shoots rays to check for colliders
        ///
        private void IsClear(Vector3 origin, Vector3 direction, float distance, ref bool isHit)
        {
            RaycastHit hit;
            Debug.DrawRay(origin, direction * distance, Color.green);

            if (Physics.Raycast(origin, direction, out hit, distance, groundLayers))
            {
                isHit = true;
            }
            else
            {
                isHit = false;
            }

            if (obstacleForward)
            {
                Vector3 incomingVec = hit.point - origin;
                Vector3 reflectVect = Vector3.Reflect(incomingVec, hit.normal);
                float angle = Vector3.Angle(incomingVec, reflectVect);

                if (angle < 80)
                {
                    isHit = false;
                }
            }

            if (groundForward)
            {
                if (horizontal != 0 || vertical != 0)
                {
                    Vector3 p1 = transform.position;
                    Vector3 p2 = hit.point;
                    float diffy = p1.y - p2.y;
                    groundAngle = diffy;
                }
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Calls all equipped power-up's update
        ///
        private void UpdateArsenalSkills()
        {
            if (playerState == PlayerStates.HOLD || playerState == PlayerStates.DEAD)
            {
                return;
            }

            foreach (ItemActivePickUps_SO eachPower in charInventory.equiptItems)
            {
                if (eachPower != null)
                {
                    eachPower.UpdateSkill(this);
                }
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Calls all equipped power-up's handle
        ///
        private void HandleArsenalSkills()
        {
            if (playerState == PlayerStates.HOLD || playerState == PlayerStates.DEAD)
            {
                return;
            }

            foreach (ItemActivePickUps_SO eachPower in charInventory.equiptItems)
            {
                if (eachPower != null)
                {
                    eachPower.HandleSkill(this);
                }
            }
        }

        //-////////////////////////////////////////////////////
        ///
        private void OnTriggerEnter(Collider other)
        {
            HandleArsenalSkillsOnTrigger(other);
        }

        //-////////////////////////////////////////////////////
        ///
        private void OnCollisionEnter(Collision other)
        {
            HandleArsenalSkillsOnCollision(other, this);
        }

        //-////////////////////////////////////////////////////
        ///
        /// Calls all equipped power-up's OnTrigger
        ///
        private void HandleArsenalSkillsOnTrigger(Collider other)
        {
            foreach (ItemActivePickUps_SO eachPower in charInventory.equiptItems)
            {
                if (eachPower != null)
                {
                    eachPower.OnSkillTrigger(other);
                }
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Calls all equipped power-up's OnCollision
        ///
        private void HandleArsenalSkillsOnCollision(Collision other, StateManager stateManager)
        {
            foreach (ItemActivePickUps_SO eachPower in charInventory.equiptItems)
            {
                if (eachPower != null)
                {
                    eachPower.OnSkillCollision(other, stateManager);
                }
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// gets the vortex effects ready for use
        public void SetUpVortex()
        {
            vortexInstance = Instantiate(vortexEffect);
            vortexInstance.transform.SetParent(this.gameObject.transform);
            vortexInstance.transform.localPosition = Vector3.zero;
        }

        //-////////////////////////////////////////////////////
        ///
        public void EnableVortex(bool argEnabled)
        {
            // if item is already equipt or for any chance vortex was not initialise then we force before
            // doing object pooling with vortex
            if (vortexInstance == null)
            {
                SetUpVortex();
            }

            vortexInstance.SetActive(argEnabled);
        }

        //-////////////////////////////////////////////////////
        ///
        /// Sets wether the player is changing Inventory
        /// 
        public void ChangeInventoryToggle()
        {
            changingInventory = changingInventory == false;

            // Update UI
            charInventory.EnableEquiptmentHover(changingInventory);        
        }

        //-////////////////////////////////////////////////////
        ///
        /// Handles Dropping an Equipment
        /// 
        public void DropEquipment()
        {
            changingInventory = false;

            // Hanldes the logic
            charInventory.DropEquipmentItem(this);

            // Handles UI
            charInventory.EnableEquiptmentHover(changingInventory);
        }
    }
}
