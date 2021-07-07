using UnityEngine;
using System.Collections;

namespace Visualisation
{
    public class VisualisationCamera : MonoBehaviour
    {
        public Camera cam;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private float startOrthographicSize;

        private bool movementLocked;

        private const float moveSpeed = 20f;
        private const float fastMoveSpeed = 60f;
        private const float rotateSpeed = 6f;

        private const float focusDuration = 0.4f;
        private const float focusDistance = VisualisationManager.neuronInterval - 0.5f;
        private const float focusOrthographicSize = 14f;

        void Start()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
            startOrthographicSize = cam.orthographicSize;
        }

        public void OnProjectionToggleButtonClicked()
        {
            cam.orthographic = !cam.orthographic;
        }

        public void OnResetButtonClicked()
        {
            if (movementLocked)
                return;

            StartCoroutine(SetPositionAndRotation(startPosition, startRotation, startOrthographicSize));
        }

        public void FocusOnNeuron(Vector3 neuronPosition)
        {
            if (movementLocked)
                return;

            var targetPosition = neuronPosition + Vector3.up * focusDistance + Vector3.back * 2;
            var targetRotation = Quaternion.Euler(new Vector3(90f, 0, 0));
            StartCoroutine(SetPositionAndRotation(targetPosition, targetRotation, focusOrthographicSize));
        }

        IEnumerator SetPositionAndRotation(Vector3 targetPosition, Quaternion targetRotation, float targetOrthographicSize)
        {
            movementLocked = true;

            float percent = 0;
            var startPosition = transform.position;
            var startRotation = transform.rotation;
            var startOrthographicSize = cam.orthographicSize;

            while (percent < 1)
            {
                percent += Time.unscaledDeltaTime / focusDuration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, percent);
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, percent);
                cam.orthographicSize = Mathf.Lerp(startOrthographicSize, targetOrthographicSize, percent);
                yield return null;
            }

            movementLocked = false;
        }

        void Update()
        {
            if (movementLocked)
                return;

            var _moveSpeed = Input.GetKey(KeyCode.LeftShift) ? fastMoveSpeed : moveSpeed;

            if (Input.GetKey(KeyCode.A))
            {
                transform.position = transform.position + (-transform.right * _moveSpeed * Time.unscaledDeltaTime);
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.position = transform.position + (transform.right * _moveSpeed * Time.unscaledDeltaTime);
            }

            if (Input.GetKey(KeyCode.W))
            {
                if (cam.orthographic)
                    cam.orthographicSize -= _moveSpeed / 2 * Time.unscaledDeltaTime;
                else
                    transform.position = transform.position + (transform.forward * _moveSpeed * Time.unscaledDeltaTime);
            }

            if (Input.GetKey(KeyCode.S))
            {
                if (cam.orthographic)
                    cam.orthographicSize += _moveSpeed / 2 * Time.unscaledDeltaTime;
                else
                    transform.position = transform.position + (-transform.forward * _moveSpeed * Time.unscaledDeltaTime);
            }

            if (Input.GetKey(KeyCode.E))
            {
                transform.position = transform.position + (transform.up * _moveSpeed * Time.unscaledDeltaTime);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.position = transform.position + (-transform.up * _moveSpeed * Time.unscaledDeltaTime);
            }

            if (Input.GetKey(KeyCode.Mouse1))
            {
                float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * rotateSpeed;
                float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * rotateSpeed;

                if (transform.forward.y < 0 && newRotationY > 90)
                {
                    newRotationY = 90;
                }
                else if (transform.forward.y > 0 && newRotationY < 270)
                {
                    newRotationY = 270;
                }

                transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, new Vector3(newRotationY, newRotationX, 0f), Time.unscaledDeltaTime * 25);
            }
        }
    }
}