using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TimeScaleSettings : MonoBehaviour
    {
        public Slider slider;
        public GameObject pauseButton;
        public GameObject resumeButton;
        public GameObject stepButton;

        private float timeScaleBeforePause;

        private bool stepBegin;
        private bool stepEnd;

        void Start()
        {
            slider.value = Time.timeScale;
        }

        public void OnSliderValueChanged()
        {
            Time.timeScale = slider.value;

            pauseButton.SetActive(true);
            resumeButton.SetActive(false);
            stepButton.SetActive(false);
        }

        public void OnPauseButtonClicked()
        {
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0;

            pauseButton.SetActive(false);
            resumeButton.SetActive(true);
            stepButton.SetActive(true);
        }

        public void OnResumeButtonClicked()
        {
            Time.timeScale = timeScaleBeforePause;

            pauseButton.SetActive(true);
            resumeButton.SetActive(false);
            stepButton.SetActive(false);
        }

        public void OnStepButtonClicked()
        {
            stepBegin = true;
        }

        void Update()
        {
            if (stepBegin)
            {
                Time.timeScale = 1;
                stepBegin = false;
                stepEnd = true;
            }
        }

        void FixedUpdate()
        {
            if (stepEnd)
            {
                stepEnd = false;
                Time.timeScale = 0;
            }
        }
    }
}