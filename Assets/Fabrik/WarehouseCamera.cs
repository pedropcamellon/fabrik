using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Fabrik
{
    public sealed class WarehouseCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.65f, 0f, -3.2f);
        [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 1.4f, 0f);
        [SerializeField, Min(0.01f)] private float sensitivity = 0.12f;

        private float yaw;
        private float pitch = 12f;

        public void SetTarget(Transform value)
        {
            target = value;
            InitializeAim();
        }

        private void OnEnable()
        {
            InitializeAim();
            if (Application.isPlaying)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard?.escapeKey.wasPressedThisFrame == true)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (mouse?.leftButton.wasPressedThisFrame == true)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * sensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -25f, 65f);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Quaternion aim = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + pivotOffset;
            transform.SetPositionAndRotation(pivot + aim * shoulderOffset, aim);
        }

        private void InitializeAim()
        {
            if (target == null)
            {
                return;
            }

            Vector3 direction = target.position + pivotOffset - transform.position;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = target.forward;
            }

            Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            yaw = Quaternion.LookRotation(planarDirection, Vector3.up).eulerAngles.y;
            pitch = -Mathf.Asin(direction.normalized.y) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, -25f, 65f);
        }
    }
}