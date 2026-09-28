using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Fabrik
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class WarehouseWalker : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 3f;
        [SerializeField, Min(30f)] private float turnSpeed = 180f;

        private CharacterController controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            float turn = 0f;
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) turn -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) turn += 1f;

            float move = 0f;
            if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) move += 1f;
            if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) move -= 1f;

            transform.Rotate(Vector3.up, turn * turnSpeed * Time.deltaTime);
            controller.SimpleMove(transform.forward * (move * speed));
        }
    }
}