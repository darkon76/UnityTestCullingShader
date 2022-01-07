using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullingCoordinator : MonoBehaviour
{

    public float m_occlusionCapsuleHeight = 0f;
    public float m_occlusionCapsuleRadius = 1f;

    // list of objects that will trigger the culling effect
    public List<GameObject> m_importantObjects = new List<GameObject>();

    // include the mouse in the important objects
    public bool m_includeMouse;

    public LayerMask m_layerMask;


    // List of all the objects that we've set to occluding state
    private HashSet<Cullable> m_occludingObjects = new HashSet<Cullable>();


    List<Cullable> cullableList = new List<Cullable>();
    private readonly HashSet<Vector3> _positions = new HashSet<Vector3>();
    private readonly HashSet<Cullable> _detectedOccludingObjects = new HashSet<Cullable>();

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }



    // Update is called once per frame // Handle per frame logic
    public void Update()
    {
        // Can only do occlusion checks if we have a camera
        if (_camera != null)
        {
            // This is the list of positions we're trying not to occlude
            FindImportantPositions();

            // This is the list of objects which are in the way
            FindOccludingObjects();

            SetOccludingObjects();
        }
    }

    private void FindImportantPositions()
    {
        _positions.Clear();


        // All units are important
        foreach (GameObject unit in m_importantObjects)
        {
            _positions.Add(unit.transform.position);
        }


        if (m_includeMouse && Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, m_layerMask) && m_includeMouse)
        {
            Vector3 mousePos = hit.point;

            _positions.Add(mousePos);
            
        }
    }

    private void FindOccludingObjects()
    {

        _detectedOccludingObjects.Clear();
        // We want to do a capsule check from each position to the camera, any cullable object we hit should be culled
        foreach (Vector3 pos in _positions)
        {
            Vector3 capsuleStart = (pos);
            capsuleStart.y += m_occlusionCapsuleHeight;

            Collider[] colliders = Physics.OverlapCapsule(capsuleStart, _camera.transform.position, m_occlusionCapsuleRadius, m_layerMask, QueryTriggerInteraction.Ignore);

            // Add cullable objects we found to the list
            foreach (Collider collider in colliders)
            {
                Cullable cullable = collider.GetComponent<Cullable>();
                if (cullable == null)
                {
                    //Debug.Assert(cullable != null, "Found an object on the occlusion layer without the occlusion component!");
                    continue;
                }

                _detectedOccludingObjects.Add(cullable);

            }
        }
    }


    // Update the stored list of occluding objects
    private void SetOccludingObjects()
    {
        foreach (Cullable cullable in _detectedOccludingObjects)
        {
            bool contains = m_occludingObjects.Contains(cullable);

            if (contains)
            {
                cullable.Occluding = true;

                // This object isnt in the old list, so we need to mark it as occluding

            }
            else
            {
                // This object was already in the list, so remove it from the old list
                m_occludingObjects.Remove(cullable);
            }
        }

        // Any object left in the old list, was not in the new list, so it's no longer occludding
        foreach (Cullable cullable in m_occludingObjects)
        {
            cullable.Occluding = false;
        }

        m_occludingObjects = newList;
    }

}