using UnityEngine;
using UnityEngine.UI;

namespace Visualisation
{
    public class NeuronLabel : MonoBehaviour
    {
        public Text labelText;
        public RectTransform labelRect;

        private Neuron target;
        private VisualisationCamera visualisationCamera;

        private bool left;

        private const float labelOffset = 2f;

        public void Initialize(Neuron target, string label, bool left, VisualisationCamera visualisationCamera)
        {
            labelText.text = label;
            this.target = target;
            this.visualisationCamera = visualisationCamera;
            this.left = left;

            labelRect.pivot = new Vector2(left ? 1 : 0, 0.5f);

            Update();
        }

        void Update()
        {
            if (!target)
                return;

            var targetPosition = target.output.position + labelOffset * (left ? Vector3.left : Vector3.right);
            var viewport = visualisationCamera.cam.WorldToViewportPoint(targetPosition);

            if (viewport.x < 0 || viewport.x > 1 || viewport.y < 0 || viewport.y > 1 || viewport.z < 0 || visualisationCamera.cam.transform.position.z > target.output.position.z)
                labelRect.gameObject.SetActive(false);
            else
                labelRect.gameObject.SetActive(true);

            if (target && labelRect.gameObject.activeInHierarchy)
                transform.position = visualisationCamera.cam.WorldToScreenPoint(targetPosition);
        }
    }
}