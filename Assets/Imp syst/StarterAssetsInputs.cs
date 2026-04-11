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

        // Inventory slot selection
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

        // UI / pages
        public bool RightPage;
        public bool LeftPage;

        // Other UI toggles
        public bool Map;
        public bool Keyitem;
        public bool Quests;
        public bool RandomLoot;

        [Header("One Frame Actions")]
        public bool inventory;
        public bool interact;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        [Header("UI")]
        public bool uiBlocked;

        public Vector2 MousePosition {
    get {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }
}

#if ENABLE_INPUT_SYSTEM

        public void OnMove(InputValue value)
        {
            if (uiBlocked) { move = Vector2.zero; return; }
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (uiBlocked) { look = Vector2.zero; return; }

            if (cursorInputForLook)
                LookInput(value.Get<Vector2>());
        }

        public void OnJump(InputValue value)
        {
            if (uiBlocked) { jump = false; return; }
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            if (uiBlocked) { sprint = false; return; }
            SprintInput(value.isPressed);
        }

        public void OnCrouch(InputValue value)
        {
            if (uiBlocked) { crouch = false; return; }
            CrouchInput(value.isPressed);
        }

        public void OnInventory(InputValue value)
        {
            // Inventory should still toggle even when uiBlocked is true,
            // because it's the key that opens/closes the UI.
            if (value.isPressed) inventory = true;
        }

        public void OnInteract(InputValue value)
        {
            // Interact should NOT fire while UI is open.
            if (uiBlocked) return;
            if (value.isPressed) interact = true;
        }

        public void OnSlot1(InputValue value) { if (value.isPressed) Slot1 = true; }
        public void OnSlot2(InputValue value) { if (value.isPressed) Slot2 = true; }
        public void OnSlot3(InputValue value) { if (value.isPressed) Slot3 = true; }
        public void OnSlot4(InputValue value) { if (value.isPressed) Slot4 = true; }
        public void OnSlot5(InputValue value) { if (value.isPressed) Slot5 = true; }
        public void OnSlot6(InputValue value) { if (value.isPressed) Slot6 = true; }
        public void OnSlot7(InputValue value) { if (value.isPressed) Slot7 = true; }
        public void OnSlot8(InputValue value) { if (value.isPressed) Slot8 = true; }
        public void OnSlot9(InputValue value) { if (value.isPressed) Slot9 = true; }
        public void OnSlot0(InputValue value) { if (value.isPressed) Slot0 = true; }

        public void OnRandomLoot(InputValue value) { if (value.isPressed) RandomLoot = true; }

        public void OnScroll(InputValue value)
        {
            scroll = value.Get<float>();
        }

        public void OnRightPage(InputValue value)
        {
            // These are UI navigation inputs; allow them while uiBlocked
            if (value.isPressed) RightPage = true;
        }

        public void OnLeftPage(InputValue value)
        {
            // These are UI navigation inputs; allow them while uiBlocked
            if (value.isPressed) LeftPage = true;
        }

        public void OnMap(InputValue value)
        {
            // UI input; allow while uiBlocked
            if (value.isPressed) Map = true;
        }

        public void OnKeyitem(InputValue value)
        {
            // UI input; allow while uiBlocked
            if (value.isPressed) Keyitem = true;
        }

        public void OnQuests(InputValue value)
        {
            // UI input; allow while uiBlocked
            if (value.isPressed) Quests = true;
        }

#endif

        // These are useful if you ever want to simulate input
        public void MoveInput(Vector2 newMoveDirection) => move = newMoveDirection;
        public void LookInput(Vector2 newLookDirection) => look = newLookDirection;
        public void JumpInput(bool newJumpState) => jump = newJumpState;
        public void SprintInput(bool newSprintState) => sprint = newSprintState;
        public void CrouchInput(bool newCrouchState) => crouch = newCrouchState;

        public void Slot1Input(bool newState) => Slot1 = newState;
        public void Slot2Input(bool newState) => Slot2 = newState;
        public void Slot3Input(bool newState) => Slot3 = newState;
        public void Slot4Input(bool newState) => Slot4 = newState;
        public void Slot5Input(bool newState) => Slot5 = newState;
        public void Slot6Input(bool newState) => Slot6 = newState;
        public void Slot7Input(bool newState) => Slot7 = newState;
        public void Slot8Input(bool newState) => Slot8 = newState;
        public void Slot9Input(bool newState) => Slot9 = newState;
        public void Slot0Input(bool newState) => Slot0 = newState;
        public void RandomLootInput(bool newState) => RandomLoot = newState;

        public void ScrollInput(float newScrollValue) => scroll = newScrollValue;

        public void RightPageInput(bool newState) => RightPage = newState;
        public void LeftPageInput(bool newState) => LeftPage = newState;
        public void MapInput(bool newState) => Map = newState;
        public void KeyitemInput(bool newState) => Keyitem = newState;
        public void QuestsInput(bool newState) => Quests = newState;

        // Optional convenience APIs: consuming clears the flag for you.
        public bool ConsumeInventory()
        {
            if (!inventory) return false;
            inventory = false;
            return true;
        }

        public bool ConsumeInteract()
        {
            if (!interact) return false;
            interact = false;
            return true;
        }

        public bool ConsumeRightPage()
        {
            if (!RightPage) return false;
            RightPage = false;
            return true;
        }

        public bool ConsumeLeftPage()
        {
            if (!LeftPage) return false;
            LeftPage = false;
            return true;
        }

        public bool ConsumeMap()
        {
            if (!Map) return false;
            Map = false;
            return true;
        }

        public bool ConsumeKeyitem()
        {
            if (!Keyitem) return false;
            Keyitem = false;
            return true;
        }

        public bool ConsumeQuests()
        {
            if (!Quests) return false;
            Quests = false;
            return true;
        }
        public bool ConsumeRandomLoot()
        {
            if (!RandomLoot) return false;
            RandomLoot = false;
            return true;
        }

        public void ClearOneFrameInputs()
        {
            inventory = false;
            interact = false;

            RightPage = false;
            LeftPage = false;
            Map = false;
            Keyitem = false;
            Quests = false;

            Slot0 = false;
            Slot1 = false;
            Slot2 = false;
            Slot3 = false;
            Slot4 = false;
            Slot5 = false;
            Slot6 = false;
            Slot7 = false;
            Slot8 = false;
            Slot9 = false;
            RandomLoot = false;

            scroll = 0f;
        }

        private void LateUpdate()
        {
            ClearOneFrameInputs();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        public void SetCursorState(bool newState)
        {
            cursorLocked = newState;
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}