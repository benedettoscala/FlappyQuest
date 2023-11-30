using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using static OVRSpatialAnchor;
//using static UnityEditor.Progress;

public class AnchorTutorialUIManager : MonoBehaviour
{

    /// <summary>
    /// Anchor Tutorial UI manager singleton instance
    /// </summary>
    public static AnchorTutorialUIManager Instance;


    [SerializeField]
    private GameObject _saveableAnchorPrefab;
    public GameObject SaveableAnchorPrefab => _saveableAnchorPrefab;

    [SerializeField, FormerlySerializedAs("_saveablePreview")]
    private GameObject _saveablePreview;

    [SerializeField, FormerlySerializedAs("_saveableTransform")]
    private Transform _saveableTransform;


    [SerializeField]
    private GameObject _nonSaveableAnchorPrefab;
    public GameObject NonSaveableAnchorPrefab => _nonSaveableAnchorPrefab;

    [SerializeField, FormerlySerializedAs("_nonSaveablePreview")]
    private GameObject _nonSaveablePreview;

    [SerializeField, FormerlySerializedAs("_nonSaveableTransform")]
    private Transform _nonSaveableTransform;


    private OVRSpatialAnchor _workingAnchor;

    private List<OVRSpatialAnchor> _allSavedAnchors; //we've written these to the headset (green only)
    private List<OVRSpatialAnchor> _allRunningAnchors; //these are currently running (red and green)

    private System.Guid[] _anchorSavedUUIDList; //simulated external location, like PlayerPrefs
    private int _anchorSavedUUIDListSize;
    private const int _anchorSavedUUIDListMaxSize = 50;

    Action<OVRSpatialAnchor.UnboundAnchor, bool> _onLoadAnchor;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _allSavedAnchors = new List<OVRSpatialAnchor>();
            _allRunningAnchors = new List<OVRSpatialAnchor>();
            _anchorSavedUUIDList = new System.Guid[_anchorSavedUUIDListMaxSize];
            _anchorSavedUUIDListSize = 0;
            _onLoadAnchor = OnLocalized;
        }
        else
        {
            Destroy(this);
        }
    }

    /*
     * We respond to five button events:
     *
     * Left trigger: Create a saveable (green) anchor.
     * Right trigger: Create a non-saveable (red) anchor.
     * A: Load, Save and display all saved anchors (green only)
     * X: Destroy all runtime anchors (red and green)
     * Y: Erase all anchors (green only)
     * others: no action
     */
    void Update()
    {

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            //create a green (saveable) spatial anchor
            GameObject gs = PlaceAnchor(_saveableAnchorPrefab, _saveableTransform.position, _saveableTransform.rotation); //anchor A
            _workingAnchor = gs.AddComponent<OVRSpatialAnchor>();

            CreateAnchor(_workingAnchor, true);
        }
        else if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            //create a red (non-saveable) spatial anchor.
            GameObject gs = PlaceAnchor(_nonSaveableAnchorPrefab, _nonSaveableTransform.position, _nonSaveableTransform.rotation); //anchor b
            _workingAnchor = gs.AddComponent<OVRSpatialAnchor>();

            CreateAnchor(_workingAnchor, false);
        }
        else if (OVRInput.GetDown(OVRInput.Button.One))
        {
            LoadAllAnchors(); // load saved anchors
        }
        else if (OVRInput.GetDown(OVRInput.Button.Three)) //x button
        {
            //Destroy all anchors from the scene, but don't erase them from storage
            using (var enumerator = _allRunningAnchors.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var spAnchor = enumerator.Current;
                    Destroy(spAnchor.gameObject);
                    //Debug.Log("Destroyed an Anchor " + spAnchor.Uuid.ToString() + "!");
                }
            }

            //clear the list of running anchors
            _allRunningAnchors.Clear();
        }
        else if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            EraseAllAnchors(); // erase all saved (green) anchors
        }
        else // any other button?
        {
            // no other actions tracked
        }
    }

    /****************************** Button Handlers ***********************/



    /******************* Create Anchor Methods *****************/

    public void CreateAnchor(OVRSpatialAnchor spAnchor, bool saveAnchor)
    {
        //use a Unity coroutine to manage the async save
        StartCoroutine(anchorCreated(_workingAnchor, saveAnchor));
    }

    /*
     * Unity Coroutine
     * We need to make sure the anchor is ready to use before we save it.
     * Also, only save if specified
     */
    public IEnumerator anchorCreated(OVRSpatialAnchor osAnchor, bool saveAnchor)
    {
        // keep checking for a valid and localized anchor state
        while (!osAnchor.Created && !osAnchor.Localized)
        {
            yield return new WaitForEndOfFrame();
        }

        //Save the anchor to a local List so we can track during the current session
        _allRunningAnchors.Add(osAnchor);

        // we save the saveable (green) anchors only
        if (saveAnchor)
        {
            //when ready, save the anchor.
            osAnchor.Save((anchor, success) =>
            {
                if (success)
                {
                    //save UUID to external storage so we can refer to the anchors in a later session
                    SaveAnchorUuidToExternalStore(anchor);
                    //Debug.Log("Anchor " + osAnchor.Uuid.ToString() + " Saved!");
                    //keep tabs on anchors in local storage
                    _allSavedAnchors.Add(anchor);
                }
            });
        }
    }



    /******************* Load Anchor Methods **********************/

    public void LoadAllAnchors()
    {
        OVRSpatialAnchor.LoadOptions options = new OVRSpatialAnchor.LoadOptions
        {
            Timeout = 0,
            StorageLocation = OVRSpace.StorageLocation.Local,
            Uuids = GetSavedAnchorsUuids() //GetAnchorsUuidsFromExternalStore()
        };

        //load and localize
        OVRSpatialAnchor.LoadUnboundAnchors(options, _anchorSavedUUIDList =>
        {
            if (_anchorSavedUUIDList == null)
            {
                //Debug.Log("Anchor list is null!");
                return;
            }

            foreach (var anchor in _anchorSavedUUIDList)
            {
                if (anchor.Localized)
                {
                    _onLoadAnchor(anchor, true);
                }
                else if (!anchor.Localizing)
                {
                    anchor.Localize(_onLoadAnchor);
                }
            }
        });
    }

    private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor, bool success)
    {
        var pose = unboundAnchor.Pose;
        GameObject go = PlaceAnchor(_saveableAnchorPrefab, pose.position, pose.rotation);
        _workingAnchor = go.AddComponent<OVRSpatialAnchor>();

        unboundAnchor.BindTo(_workingAnchor);

        // add the anchor to the running total
        _allRunningAnchors.Add(_workingAnchor);
    }

    /*
     * Get all spatial anchor UUIDs saved to local storage
    */
    private System.Guid[] GetSavedAnchorsUuids()
    {
        var uuids = new Guid[_allSavedAnchors.Count];
        using (var enumerator = _allSavedAnchors.GetEnumerator())
        {
            int i = 0;
            while (enumerator.MoveNext())
            {
                var currentUuid = enumerator.Current.Uuid;
                uuids[i] = new Guid(currentUuid.ToByteArray());
                i++;
            }
        }
        //Debug.Log("Returned All Anchor UUIDs!");
        return uuids;
    }

    /******************* Erase Anchor Methods *****************/


    /*
     * If the Y button is pressed, erase all anchors saved
     * in the headset, but don't destroy them. They should remain
     * displayed.
     */
    public void EraseAllAnchors()
    {
        foreach (var tmpAnchor in _allSavedAnchors)
        {
            //use a Unity coroutine to manage the async save
            StartCoroutine(anchorErased(tmpAnchor));
        }

        //we also erase our reference lists
        _allSavedAnchors.Clear();
        RemoveAllAnchorsUuidsInExternalStore();
        return;
    }


    /*
     * Unity Coroutine
     * We need to make sure the anchor is ready to use before we erase it.
     */
    public IEnumerator anchorErased(OVRSpatialAnchor osAnchor)
    {
        while (!osAnchor.Created)
        {
            yield return new WaitForEndOfFrame();
        }

        //when ready, erase the anchor.
        osAnchor.Erase((anchor, success) =>
        {
            if (!success)
            {
                Debug.Log("Anchor " + osAnchor.Uuid.ToString() + " NOT Erased!");
            } else
            {
                Debug.Log("Anchor " + osAnchor.Uuid.ToString() + " Erased!");
                ;
            }

            return;
        });
    }


     /********************************* Display an Anchor Prefab *****************/

    /*
     * Display an anchor prefab, red or  green
     */
    private GameObject PlaceAnchor(GameObject prefab, Vector3  p, Quaternion r)
    {
        //Debug.Log("Placing a new anchor prefab!");
        return Instantiate(prefab, p, r);
    }



    /******************These three methods simulate an external store, such as PlayerPrefs ******************/

    /*
     * Add one spatial anchor to the external store
     */
    private void SaveAnchorUuidToExternalStore(OVRSpatialAnchor spAnchor)
    {
        if(_anchorSavedUUIDListSize < _anchorSavedUUIDListMaxSize)
        {
            _anchorSavedUUIDList[_anchorSavedUUIDListSize] = (spAnchor.Uuid);
            _anchorSavedUUIDListSize++;
            //Debug.Log("Saved Anchor " + spAnchor.Uuid.ToString() + " to a external Store!");
        }
        else
        {
            //Debug.Log("Can't save anchor " + spAnchor.Uuid.ToString() + ", because the store is full!");
        }
        return;
    }

    /*
     * Get all spatial anchor UUIDs saved to the external store
     */
    private System.Guid[] GetAnchorsUuidsFromExternalStore()
    {

        var uuids = new Guid[_anchorSavedUUIDListSize];
        for (int i = 0; i < _anchorSavedUUIDListSize; ++i)
        {
            var uuidKey = "uuid" + i;
            var currentUuid = _anchorSavedUUIDList[i].ToByteArray();
            uuids[i] = new Guid(currentUuid);
        }
        //Debug.Log("Returned All Anchor UUIDs!");
        return uuids;
    }

    /*
     * Empty the external external store
     */
    private void RemoveAllAnchorsUuidsInExternalStore()
    {
        _anchorSavedUUIDList = new System.Guid[_anchorSavedUUIDListMaxSize];
        //Debug.Log("Cleared the external Store!");
        return;
    }

}