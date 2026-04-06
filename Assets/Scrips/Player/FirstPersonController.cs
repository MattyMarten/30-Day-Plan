using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;

		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;

		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;

		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Tooltip("How fast the crouch transition happens")]
		public float CrouchTransitionSpeed = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;

		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;

		[Tooltip("Time required before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Crouch Settings")]
		[Tooltip("CharacterController height while standing (auto-captured at Start if 0)")]
		public float StandingHeight = 0f;

		[Tooltip("CharacterController height while crouched")]
		public float CrouchingHeight = 1.2f;

		[Tooltip("How far to lower the CinemachineCameraTarget when crouched")]
		public float CrouchCameraOffset = 0.6f;

		[Tooltip("Layers considered as blockers when trying to stand")]
		public LayerMask StandBlockLayers = ~0;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;

		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;

		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;

		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Crouch Stand Check")]
		[Tooltip("Radius used to check if there is room to stand up")]
		public float StandCheckRadius = 0.3f;

		[Tooltip("How far upward to check when trying to stand")]
		public float StandCheckDistance = 1f;

		[Header("Jump Headroom Check")]
		[Tooltip("Minimum free space above the character to allow jumping")]
		public float JumpHeadroom = 0.2f;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;

		[Tooltip("How far in degrees you can move the camera up")]
		public float TopClamp = 90.0f;

		[Tooltip("How far in degrees you can move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private readonly float _terminalVelocity = 53.0f;

		// timeout delta time
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		// crouch
		private float _standingHeight;
		private float _targetHeight;

		private Vector3 _standingCenter;
		private Vector3 _crouchingCenter;

		private Vector3 _camStandLocalPos;
		private Vector3 _camCrouchLocalPos;

#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;

			// --- Crouch init ---
		_standingHeight = (StandingHeight > 0f) ? StandingHeight : _controller.height;
		_targetHeight = _standingHeight;

		_standingCenter = _controller.center;

			// When we shrink the capsule, we also move its center down so feet stay planted.
		_crouchingCenter = _standingCenter;
		_crouchingCenter.y -= (_standingHeight - CrouchingHeight) * 0.5f;

			// Camera target positions (local space)
		_camStandLocalPos = CinemachineCameraTarget.transform.localPosition;
		_camCrouchLocalPos = _camStandLocalPos + Vector3.down * CrouchCameraOffset;
		}

		private void Update()
		{
			GroundedCheck();
			JumpAndGravity();
			Crouch();
			Move();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			Vector3 spherePosition = new Vector3(
				transform.position.x,
				transform.position.y - GroundedOffset,
				transform.position.z
			);

			Grounded = Physics.CheckSphere(
				spherePosition,
				GroundedRadius,
				GroundLayers,
				QueryTriggerInteraction.Ignore
			);
		}

		private void CameraRotation()
		{
			if (_input.look.sqrMagnitude >= _threshold)
			{
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				CinemachineCameraTarget.transform.localRotation =
					Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			if (_input.move == Vector2.zero)
				targetSpeed = 0.0f;

			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			Vector3 inputDirection = Vector3.zero;

			if (_input.move != Vector2.zero)
			{
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			Vector3 movement = inputDirection.normalized * (_speed * Time.deltaTime);
			Vector3 verticalMovement = new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime;

			_controller.Move(movement + verticalMovement);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				_fallTimeoutDelta = FallTimeout;

				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					if (CanJump())
					{
						_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
					}
					else
					{
						_input.jump = false;
					}
				}

				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				_jumpTimeoutDelta = JumpTimeout;

				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				_input.jump = false;
			}

			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private void Crouch()
		{
    		bool wantsCrouch = _input.crouch;

    		if (wantsCrouch)
    		{
        		_targetHeight = CrouchingHeight;
    		}
    		else
    		{
        			// Only stand if there's room
        		_targetHeight = CanStandUp() ? _standingHeight : CrouchingHeight;
    		}

    		bool isCrouchingTarget = _targetHeight <= (CrouchingHeight + 0.01f);

    			// Smoothly adjust controller capsule
    		_controller.height = Mathf.Lerp(_controller.height, _targetHeight, Time.deltaTime * CrouchTransitionSpeed);

    		Vector3 targetCenter = isCrouchingTarget ? _crouchingCenter : _standingCenter;
    		_controller.center = Vector3.Lerp(_controller.center, targetCenter, Time.deltaTime * CrouchTransitionSpeed);

   				// Smoothly adjust camera target
    		Vector3 camTarget = isCrouchingTarget ? _camCrouchLocalPos : _camStandLocalPos;
    		CinemachineCameraTarget.transform.localPosition =
        	Vector3.Lerp(CinemachineCameraTarget.transform.localPosition, camTarget, Time.deltaTime * CrouchTransitionSpeed);
		}

		private bool CanStandUp()
		{
				// If we're already essentially standing, allow
			if (_controller.height >= _standingHeight - 0.02f)
				return true;

			float radius = Mathf.Max(StandCheckRadius, _controller.radius * 0.95f);

				// Bottom of the capsule in world space (approx)
			Vector3 worldCenter = transform.TransformPoint(_controller.center);
			Vector3 bottom = worldCenter + Vector3.down * (_controller.height * 0.5f - radius);

				// Extra headroom needed to reach standing height
			float extra = (_standingHeight - _controller.height);
			if (extra <= 0f) return true;

			return !Physics.SphereCast(
				bottom,
				radius,
				Vector3.up,
				out _,
				extra,
				StandBlockLayers,
				QueryTriggerInteraction.Ignore
			);
		}

		private bool CanJump()
		{
				// We cast upward from near the top of the capsule
			float radius = _controller.radius * 0.95f;

			Vector3 worldCenter = transform.TransformPoint(_controller.center);

				// Top of capsule sphere center (approx)
			Vector3 top = worldCenter + Vector3.up * (_controller.height * 0.5f - radius);

				// If something is very close above our head, don't allow starting a jump.
			return !Physics.SphereCast(
				top,
				radius,
				Vector3.up,
				out _,
				JumpHeadroom,
				StandBlockLayers,
				QueryTriggerInteraction.Ignore
			);
		}

		private static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f) angle += 360f;
			if (angle > 360f) angle -= 360f;
			return Mathf.Clamp(angle, min, max);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			Gizmos.color = Grounded ? transparentGreen : transparentRed;

			Gizmos.DrawSphere(
				new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
				GroundedRadius
			);
		}
	}
}