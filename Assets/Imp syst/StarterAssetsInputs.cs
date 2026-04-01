using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool crouch;
		public bool Slot1;
		public bool Slot2;
		public bool Slot3;	
		public bool Slot4;
		public bool Slot5;
		public bool Slot6;
		public bool Slot7;
		public bool Slot8;
		public bool Slot9;
		public bool Slot0;
		public float scroll;
		public bool RightPage;
		public bool LeftPage;
		public bool Map;
		public bool Keyitem;
		public bool Quests;

		[Header("One Frame Actions")]
		public bool inventory;
		public bool interact;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value) => MoveInput(value.Get<Vector2>());

		public void OnLook(InputValue value)
		{
			if (cursorInputForLook)
				LookInput(value.Get<Vector2>());
		}

		public void OnJump(InputValue value) => JumpInput(value.isPressed);
		public void OnSprint(InputValue value) => SprintInput(value.isPressed);
		public void OnCrouch(InputValue value) => CrouchInput(value.isPressed);
		public void OnInventory(InputValue value)
		{
			if (value.isPressed) inventory = true;
		}
		public void OnInteract(InputValue value)
		{
			if (value.isPressed) interact = true;
		}
		public void OnSlot1(InputValue value)
		{
			if (value.isPressed) Slot1 = true;
		}
		public void OnSlot2(InputValue value)
		{
			if (value.isPressed) Slot2 = true;
		}
		public void OnSlot3(InputValue value)
		{
			if (value.isPressed) Slot3 = true;
		}
		public void OnSlot4(InputValue value)
		{
			if (value.isPressed) Slot4 = true;
		}
		public void OnSlot5(InputValue value)
		{
			if (value.isPressed) Slot5 = true;
		}
		public void OnSlot6(InputValue value)
		{
			if (value.isPressed) Slot6 = true;
		}
		public void OnSlot7(InputValue value)
		{
			if (value.isPressed) Slot7 = true;
		}
		public void OnSlot8(InputValue value)
		{
			if (value.isPressed) Slot8 = true;
		}
		public void OnSlot9(InputValue value)
		{
			if (value.isPressed) Slot9 = true;
		}
		public void OnSlot0(InputValue value)
		{
			if (value.isPressed) Slot0 = true;
		}
		public void OnScroll(InputValue value)
		{
    		scroll = value.Get<float>();
		}
		public void OnRightPage(InputValue value)
		{
			if (value.isPressed) RightPage = true;
		}
		public void OnLeftPage(InputValue value)
		{
			if (value.isPressed) LeftPage = true;
		}
		public void OnMap(InputValue value)
		{
			if (value.isPressed) Map = true;
		}
				public void OnKeyitem(InputValue value)
		{
			if (value.isPressed) Keyitem = true;
		}
		public void OnQuests(InputValue value)
		{
			if (value.isPressed) Quests = true;
		}
#endif

		public void MoveInput(Vector2 newMoveDirection) => move = newMoveDirection;
		public void LookInput(Vector2 newLookDirection) => look = newLookDirection;
		public void JumpInput(bool newJumpState) => jump = newJumpState;
		public void SprintInput(bool newSprintState) => sprint = newSprintState;
		public void CrouchInput(bool newCrouchState) => crouch = newCrouchState;
		public void Slot1Input(bool newSlot1State) => Slot1 = newSlot1State;
		public void Slot2Input(bool newSlot2State) => Slot2 = newSlot2State;
		public void Slot3Input(bool newSlot3State) => Slot3 = newSlot3State;
		public void Slot4Input(bool newSlot4State) => Slot4 = newSlot4State;
		public void Slot5Input(bool newSlot5State) => Slot5 = newSlot5State;
		public void Slot6Input(bool newSlot6State) => Slot6 = newSlot6State;
		public void Slot7Input(bool newSlot7State) => Slot7 = newSlot7State;
		public void Slot8Input(bool newSlot8State) => Slot8 = newSlot8State;
		public void Slot9Input(bool newSlot9State) => Slot9 = newSlot9State;
		public void Slot0Input(bool newSlot0State) => Slot0 = newSlot0State;
		public void ScrollInput(float newScrollValue) => scroll = newScrollValue;
		public void RightPageInput(bool newSlot0State) => RightPage = newSlot0State;
		public void leftPageInput(bool newSlot0State) => LeftPage = newSlot0State;
		public void MapInput(bool newSlot0State) => Map = newSlot0State;
		public void KeyitemInput(bool newSlot0State) => Keyitem = newSlot0State;
		public void QuestsInput(bool newSlot0State) => Quests = newSlot0State;

		public void ClearOneFrameInputs()
		{
			inventory = false;
			interact = false;
			RightPage = false;
			LeftPage = false;
			Map = false;
			
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}