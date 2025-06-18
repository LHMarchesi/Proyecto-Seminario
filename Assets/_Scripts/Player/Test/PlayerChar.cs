using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CrouchInput
{
    None,Toggle
}

public enum Stance
{
    Stand, Crouch, Slide
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public CrouchInput Crouch;
    public bool JumpSustain;
}

public class PlayerChar : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float walkResponse = 25f;
    [SerializeField] private float crouchResponse = 20f;
    [Space]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [Range(0f,1f)]
    [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float gravity = -90f;

    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f;
    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
    [Range(0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;

    public void Initialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;
        motor.CharacterController = this;
    }

    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        _requestedMovement = input.Rotation * _requestedMovement;

        _requestedJump = _requestedJump || input.Jump;
        _requestedSustainedJump = input.JumpSustain;
        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,
            _ => _requestedCrouch
        };
    }

    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;
        var cameraTargetHeight = currentHeight *
           (
           _state.Stance is Stance.Stand
           ? standCameraTargetHeight
           : crouchCameraTargetHeight
           );

        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        cameraTarget.localPosition = Vector3.Lerp
            (
            a: cameraTarget.localPosition,
            b: new Vector3(0f, cameraTargetHeight, 0f),
            t: 1f- Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
        root.localScale = Vector3.Lerp
            (
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
            );
    }

   public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) 
   {
        if (motor.GroundingStatus.IsStableOnGround)
        {
            var groundedMovement = motor.GetDirectionTangentToSurface
                    (
                    direction: _requestedMovement,
                    surfaceNormal: motor.GroundingStatus.GroundNormal
                    ) * _requestedMovement.magnitude;


            //Sliding
            {
                var moving = groundedMovement.sqrMagnitude > 0f;
                var crouching = _state.Stance is Stance.Crouch;
                var wasStanding = _lastState.Stance is Stance.Stand;
                var wasInAir = !_lastState.Grounded;
                if (moving && crouching && wasStanding && (wasStanding || wasInAir))
                {
                    _state.Stance = Stance.Slide;
                    var slideSpeed = Mathf.Max(slideStartSpeed, currentVelocity.magnitude);
                    currentVelocity = motor.GetDirectionTangentToSurface(direction: currentVelocity, surfaceNormal: motor.GroundingStatus.GroundNormal) * slideSpeed;
                }
            }

            //Move
            if(_state.Stance is Stance.Stand or Stance.Crouch)
            {
                var speed = _state.Stance is Stance.Stand
                  ? walkSpeed
                  : crouchSpeed;

                var response = _state.Stance is Stance.Stand
                    ? walkResponse
                    : crouchResponse;


                var targetVelocity = groundedMovement * speed;
                currentVelocity = Vector3.Lerp
                    (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                    );
            }
            else
            {
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                if(currentVelocity.magnitude < slideEndSpeed)
                {
                    _state.Stance = Stance.Crouch;
                }
            }

        }
        else
        {
            if(_requestedMovement.sqrMagnitude > 0f)
            {
                var planarMovement = Vector3.ProjectOnPlane
                    (vector: _requestedMovement,
                    planeNormal: motor.CharacterUp) * _requestedMovement.magnitude;

                var currentPlanarVelocity = Vector3.ProjectOnPlane
                    (
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp
                    );


                var movementForce = planarMovement * airAcceleration * deltaTime;
                var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                currentVelocity += targetPlanarVelocity - currentPlanarVelocity; 

            }
            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if (_requestedSustainedJump && verticalSpeed > 0f)
                effectiveGravity *= jumpSustainGravity;
            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        if (_requestedJump)
        {
            _requestedJump = false;
            motor.ForceUnground(time: 0f);

            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }
        
   }
   public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
   {
        var forward = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, motor.CharacterUp);

        if (forward != Vector3.zero)
         currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
   }

   public void BeforeCharacterUpdate(float deltaTime)
   {
        _tempState = _state;
        if(_requestedCrouch && _state.Stance is Stance.Stand)
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions
                (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: 0f
                );
        }
   }
   public void PostGroundingUpdate(float deltaTime) { }
   public void AfterCharacterUpdate(float deltaTime)
   { 
        if(!_requestedCrouch && _state.Stance is not Stance.Stand)
        {
            _state.Stance = Stance.Stand;
            motor.SetCapsuleDimensions
                (
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: 0f
                );
        }
        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _lastState = _tempState;
   }


   public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
   public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
   public bool IsColliderValidForCollisions(Collider coll) => true;
   public void OnDiscreteCollisionDetected(Collider hitCollider) { }
   public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    public Transform GetCameraTarget() => cameraTarget;
}


