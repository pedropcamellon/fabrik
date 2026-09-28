using UnityEngine;

namespace Assets.Fabrik
{
    public sealed class WarehouseCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3.5f, -5f);

        public void SetTarget(Transform value)
        {
            target = value;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = target.TransformPoint(offset);
            transform.LookAt(target.position + Vector3.up);
        }
    }
}