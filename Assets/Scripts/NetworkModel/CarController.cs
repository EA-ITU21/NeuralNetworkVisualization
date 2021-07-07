using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Visualisation;

namespace NetworkModel
{
    public class CarController : MonoBehaviour
    {
        public VisualisationManager visualisationManager;
        public Transform frontSensor;
        public Transform rightSensor;
        public Transform leftSensor;

        public LayerMask sensorLayerMask;

        public Text leftSensorText;
        public Text frontSensorText;
        public Text rightSensorText;
        public Text accelerationText;
        public Text yawText;

        private NNet network;
        private Rigidbody rb;

        private Vector3 startPosition;
        private Quaternion startRotation;

        private float leftSensorValue, frontSensorValue, rightSensorValue;
        private float acceleration, yaw;

        private Vector3 lastPosition;
        private float episodeDuration;
        private float fitness;
        private float totalDistanceTravelled;
        private float averageSpeed;

        private Coroutine waitCoroutine;
        private WaitForSeconds waitDuration = new WaitForSeconds(2);

        private const float distanceMultipler = 1.4f;
        private const float avgSpeedMultiplier = 0.2f;

        private const float maxSensorDistance = 20;

        private readonly string[] inputLabels = {"Left Sensor", "Front Sensor", "Right Sensor" };
        private readonly string[] outputLabels = {"Velocity", "Yaw"};

        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        void FixedUpdate()
        {
            if (waitCoroutine != null || network == null)
                return;

            ReadSensors();
            lastPosition = transform.position;

            var outputs = network.RunNetwork(leftSensorValue, frontSensorValue, rightSensorValue);
            yaw = outputs[1];
            acceleration = Sigmoid(outputs[0]);

            episodeDuration += Time.deltaTime;

            MoveCar();
            CalculateFitness();

            visualisationManager.UpdateNetwork();
            UpdateIOInfo();
        }

        void CalculateFitness()
        {
            totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
            averageSpeed = totalDistanceTravelled / episodeDuration;

            fitness = (totalDistanceTravelled * distanceMultipler) + (averageSpeed * avgSpeedMultiplier);

            if (episodeDuration > 20 && fitness < 40)
                EndEpisode();

            if (fitness >= 500)
                EndEpisode();
        }

        void ReadSensors()
        {
            Ray ray = new Ray(leftSensor.position, leftSensor.forward);
            RaycastHit hit;

            leftSensorValue = maxSensorDistance;
            frontSensorValue = maxSensorDistance;
            rightSensorValue = maxSensorDistance;

            if (Physics.Raycast(ray, out hit, maxSensorDistance, sensorLayerMask))
            {
                leftSensorValue = hit.distance / 20;
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }

            ray = new Ray(frontSensor.position, frontSensor.forward);

            if (Physics.Raycast(ray, out hit, maxSensorDistance, sensorLayerMask))
            {
                frontSensorValue = hit.distance / 20;
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }

            ray = new Ray(rightSensor.position, rightSensor.forward);

            if (Physics.Raycast(ray, out hit, maxSensorDistance, sensorLayerMask))
            {
                rightSensorValue = hit.distance / 20;
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }

        void MoveCar()
        {
            var moveVector = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, acceleration * 11.4f), 0.02f);
            moveVector = transform.TransformDirection(moveVector);
            transform.position += moveVector;

            transform.eulerAngles += new Vector3(0, (yaw * 90) * 0.02f, 0);
        }

        private float Sigmoid(float s)
        {
            return (1 / (1 + Mathf.Exp(-s)));
        }

        public void ResetWithNetwork(NNet network, bool wait)
        {
            if (wait)
            {
                if (waitCoroutine != null)
                    StopCoroutine(waitCoroutine);
                waitCoroutine = StartCoroutine(Wait());
            }

            this.network = network;
            visualisationManager.ResetWithNetwork(network, inputLabels, outputLabels);
            Reset();
        }

        void Reset()
        {
            acceleration = 0;
            yaw = 0;

            leftSensorValue = 0;
            frontSensorValue = 0;
            rightSensorValue = 0;

            episodeDuration = 0f;
            totalDistanceTravelled = 0f;
            averageSpeed = 0f;
            fitness = 0f;

            transform.position = startPosition;
            transform.rotation = startRotation;
            lastPosition = startPosition;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Wall"))
            {
                EndEpisode();
            }
        }

        void EndEpisode()
        {
            GenomeManager.instance.EvaluateCurrentGenome(fitness);
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(2);
            waitCoroutine = null;
        }

        void UpdateIOInfo()
        {
            leftSensorText.text = string.Format("Left Sensor: {0}", leftSensorValue);
            frontSensorText.text = string.Format("Front Sensor: {0}", frontSensorValue);
            rightSensorText.text = string.Format("Right Sensor: {0}", rightSensorValue);
            accelerationText.text = string.Format("Velocity: {0}", acceleration);
            yawText.text = string.Format("Yaw: {0}", yaw);
        }
    }
}