using UnityEngine;

namespace NoMoreFishAndChips.Cameras
{
    public class Billboard : MonoBehaviour
    {
        // We assume true since Billboarding is generally used by sprites
        [SerializeField] private bool _flip = true;

        private CameraManager _cameraManager;

        private void Awake()
        {
            _cameraManager = GameManager.Instance.Get<CameraManager>();
        }

        private void Update()
        {
            RotateUpdate();
        }

        /// <summary>
        /// Faces the main camera
        /// </summary>
        private void RotateUpdate()
        {
            Vector3 direction = (_cameraManager.MainCamera.transform.position - transform.position).normalized;

            if (_flip)
            {
                direction = -direction;
            }

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            transform.rotation = rotation;
        }
    }
}