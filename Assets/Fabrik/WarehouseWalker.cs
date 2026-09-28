using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Fabrik
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class WarehouseWalker : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 3f;
        [SerializeField, Min(30f)] private float turnSpeed = 540f;
        [SerializeField, Min(0f)] private float pushForce = 35f;

        private CharacterController controller;
        private Camera viewCamera;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            viewCamera = Camera.main;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            float horizontal = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float vertical = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);

            Transform cameraTransform = viewCamera != null ? viewCamera.transform : transform;
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 movement = Vector3.ClampMagnitude(forward * vertical + right * horizontal, 1f);
            controller.SimpleMove(movement * speed);

            if (forward.sqrMagnitude > 0f)
            {
                Quaternion facing = Quaternion.LookRotation(forward, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, facing, turnSpeed * Time.deltaTime);
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.rigidbody;
            if (body == null || body.isKinematic)
            {
                return;
            }

            Vector3 pushDirection = Vector3.ProjectOnPlane(hit.moveDirection, Vector3.up);
            if (pushDirection.sqrMagnitude > 0f)
            {
                body.AddForceAtPosition(pushDirection.normalized * pushForce, hit.point, ForceMode.Impulse);
            }
        }
    }
}