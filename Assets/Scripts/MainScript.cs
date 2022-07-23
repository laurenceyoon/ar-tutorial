using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Listens for touch events and performs an AR raycast from the screen touch point.
    /// AR raycasts will only hit detected trackables like feature points and planes.
    ///
    /// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
    /// and moved to the hit position.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class MainScript : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Instantiates this prefab on a plane at the touch location.")]
        public GameObject m_PlacedPrefab, PlayButton, StopButton;
        public Text InstructionText;

        private float timeForPlacement = 5.0f;
        private enum UserState
        {
            Ready,
            Playing
        }
        private UserState currentState = UserState.Ready;

        /// <summary>
        /// The prefab to instantiate on touch.
        /// </summary>
        public GameObject placedPrefab
        {
            get { return m_PlacedPrefab; }
            set { m_PlacedPrefab = value; }
        }

        /// <summary>
        /// The object instantiated as a result of a successful raycast intersection with a plane.
        /// </summary>
        public GameObject spawnedObject { get; private set; }

        public void playMusic()
        {
            spawnedObject.GetComponent<Goose>().play();
            InstructionText.text = "Playing the music...";
            PlayButton.SetActive(false);
            StopButton.SetActive(true);
        }

        public void stopMusic()
        {
            spawnedObject.GetComponent<Goose>().stop();
            InstructionText.text = "Music stopped";
            PlayButton.SetActive(true);
            StopButton.SetActive(false);
        }

        void Awake()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
        }

        bool TryGetTouchPosition(out Vector2 touchPosition)
        {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }

            touchPosition = default;
            return false;
        }

        void Update()
        {
            // Timer
            if (timeForPlacement <= 0)
            {
                if (currentState != UserState.Playing)
                {
                    currentState = UserState.Playing;
                    PlayButton.SetActive(true);
                    transform.GetComponent<ARPlaneManager>().SetTrackablesActive(false);
                    // option
                    InstructionText.text = "Click play button to start!";
                }
            }
            else if (spawnedObject != null)
            {
                timeForPlacement -= Time.deltaTime;
            }

            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon) && currentState == UserState.Ready)
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                var hitPose = s_Hits[0].pose;

                if (spawnedObject == null)
                {
                    instantiateGoose(hitPose);
                    // option
                    InstructionText.text = "Move around the Goose";
                }
                else
                {
                    spawnedObject.transform.position = hitPose.position;
                }
            }
            transform.GetComponent<ARPlaneManager>().SetTrackablesActive(currentState == UserState.Ready);
        }

        private void instantiateGoose(Pose hitPose)
        {
            spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
            var fmodInstance = FMODUnity.RuntimeManager.CreateInstance("event:/ARTutorial/Goose");
            spawnedObject.GetComponent<Goose>().Init(fmodInstance);
        }

        static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        ARRaycastManager m_RaycastManager;
    }
}