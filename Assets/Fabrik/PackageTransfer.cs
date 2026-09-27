using UnityEngine;

namespace Fabrik
{
    public sealed class PackageTransfer : MonoBehaviour
    {
        [SerializeField] private Vector3 source = Vector3.zero;
        [SerializeField] private Vector3 destination = new Vector3(2f, 0f, 0f);
        [SerializeField, Min(0.1f)] private float duration = 3f;

        private float elapsed;

        private void OnEnable()
        {
            elapsed = 0f;
            transform.localPosition = source;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(source, destination, Mathf.PingPong(elapsed / duration, 1f));
        }
    }
}