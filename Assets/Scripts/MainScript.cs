using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

/// <summary>
/// Listens for touch events and performs an AR raycast from the screen touch point.
/// AR raycasts will only hit detected trackables like feature points and planes.
///
/// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
/// and moved to the hit position.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class MainScript : MonoBehaviour {
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    public GameObject m_PlacedPrefab, PlayButton, StopButton;
    public Text InstructionText;

    UnityEvent placementUpdate;
    GameObject visualObject;
    public enum UserState
    {
        Ready,
        Playing
    }
    private UserState currentState = UserState.Ready;
    private float timeForPlacement = 5.0f;

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
        InstructionText.text = "Play Music!";
        PlayButton.SetActive(false);
        StopButton.SetActive(true);
    }

    public void stopMusic()
    {
        InstructionText.text = "Stop Music!";
        StopButton.SetActive(false);
        PlayButton.SetActive(true);
    }

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();

        if (placementUpdate == null)
            placementUpdate = new UnityEvent();

            placementUpdate.AddListener(DiableVisual);
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
                transform.GetComponent<ARPlaneManager>().SetTrackablesActive(false);
                currentState = UserState.Playing;
                PlayButton.SetActive(true);
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
                spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
            }
            else
            {
                spawnedObject.transform.position = hitPose.position;
            }
            placementUpdate.Invoke();
        }
    }

    public void DiableVisual()
    {
        visualObject.SetActive(false);
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;
}
