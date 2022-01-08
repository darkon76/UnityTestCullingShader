using System.Collections.Generic;
using UnityEngine;

public class CullingCoordinator : MonoBehaviour
{
    public float m_occlusionCapsuleHeight = 0f;
    public float m_occlusionCapsuleRadius = 1f;

    // list of objects that will trigger the culling effect
    [SerializeField]
    private HashSet<GameObject> m_importantObjects = new HashSet<GameObject>();

    // include the mouse in the important objects
    [SerializeField]
    private bool m_includeMouse;

    public LayerMask m_layerMask;

    private CollectionSwapper<HashSet<Cullable>, Cullable> m_occludingObjects = new CollectionSwapper<HashSet<Cullable>, Cullable>();

    List<Cullable> cullableList = new List<Cullable>();
    private readonly HashSet<Vector3> _positions = new HashSet<Vector3>();
    private Camera _camera;

    private Collider[] m_overlapCapsuleColliders = new Collider[10];
    private RaycastHit[] m_raycastHits;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if(m_includeMouse)
        {
            m_raycastHits = new RaycastHit[1];
        }
        CullingEvent.OnVisibilityChanged += OnVisibilityChangedHandler;
    }
    private void OnDestroy()
    {
        CullingEvent.OnVisibilityChanged -= OnVisibilityChangedHandler;
    }

    private void OnValidate()
    {
        if (m_includeMouse)
        {
            m_raycastHits = new RaycastHit[1];
        }
    }
    public void OnVisibilityChangedHandler(GameObject gameObject, bool visibility)
    {
        if (visibility)
        {
            m_importantObjects.Add(gameObject);
        }
        else
        {
            m_importantObjects.Remove(gameObject);
        }
    }

    // Update is called once per frame // Handle per frame logic
    public void Update()
    {
        // Can only do occlusion checks if we have a camera
        if (_camera != null)
        {
            // This is the list of positions we're trying not to occlude
            FindImportantPositions();

            // This finds and fills the current objects which are in the way
            FindOccludingObjects();

            // Sets the occlusion.
            SetOccludingObjects();

            //Swaps the occluding objects collections.
            SwapOccludingObjectsCollections();

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

        if(m_includeMouse)
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var numberOfHits = Physics.RaycastNonAlloc(ray, m_raycastHits, 100, m_layerMask);
            if(numberOfHits == 0)
            {
                return;
            }
            Vector3 mousePos = m_raycastHits[0].point;
            _positions.Add(mousePos);
        }
    }

    private void FindOccludingObjects()
    {


        //Swap and clear occluding objects. 
        var currentFrameOccluddingObjects = m_occludingObjects.MainCollection;

        // We want to do a capsule check from each position to the camera, any cullable object we hit should be culled
        foreach (Vector3 pos in _positions)
        {
            Vector3 capsuleStart = (pos);
            capsuleStart.y += m_occlusionCapsuleHeight;

            var numberOfCollidersDetected = Physics.OverlapCapsuleNonAlloc(capsuleStart, _camera.transform.position, m_occlusionCapsuleRadius,m_overlapCapsuleColliders, m_layerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < numberOfCollidersDetected; i++)
            {

                var collider = m_overlapCapsuleColliders[i];

                Cullable cullable = collider.GetComponent<Cullable>();

                if (cullable == null)
                {
                    //Debug.Assert(cullable != null, "Found an object on the occlusion layer without the occlusion component!");
                    continue;
                }

                currentFrameOccluddingObjects.Add(cullable);
            }
        }
    }


    // Update the stored list of occluding objects
    private void SetOccludingObjects()
    {
        var currentFrameOccluddingObjects = m_occludingObjects.MainCollection;
        var lastFrameOccludingObjects = m_occludingObjects.SecondaryCollection;

        foreach (Cullable cullable in currentFrameOccluddingObjects)
        {
            bool contains = lastFrameOccludingObjects.Contains(cullable);

            if (contains)
            {
                // This object was already is occluding no need to change its state.
                lastFrameOccludingObjects.Remove(cullable);
            }
            else
            {
                // This object wasn't detected last frame so we need to mark it as occluding.

                cullable.Occluding = true;
            }
        }

        // Any object left in the old collection frame that isn't occluding need to 
        foreach (Cullable cullable in lastFrameOccludingObjects)
        {
            cullable.Occluding = false;
        }
    }

    private void SwapOccludingObjectsCollections()
    {
        var currentFrameOccluddingObjects = m_occludingObjects.SwapCollections();
        currentFrameOccluddingObjects.Clear();
    }

}