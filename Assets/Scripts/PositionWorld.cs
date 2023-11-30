using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using static OVRSpatialAnchor;
public class PositionWorld : MonoBehaviour
{

    [SerializeField]
    private GameObject _saveableAnchorPrefab;
    public GameObject SaveableAnchorPrefab => _saveableAnchorPrefab;

    [SerializeField, FormerlySerializedAs("_saveablePreview")]
    private GameObject _saveablePreview;

    [SerializeField, FormerlySerializedAs("_saveableTransform")]
    private Transform _saveableTransform;

    private OVRSpatialAnchor _workingAnchor;
    private bool WorldCreated = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(_saveableTransform.rotation);
        if(OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            if(!WorldCreated)
            {
                WorldCreated = true;
                //fammi un quaternion in cui prendi la rotazione sull'asse delle y di _saveableTransform
                Quaternion rotation = Quaternion.Euler(0, _saveableTransform.rotation.y, 0);
                var anchor = Instantiate(_saveableAnchorPrefab, _saveableTransform.position, rotation);
                _workingAnchor = anchor.GetComponent<OVRSpatialAnchor>();

                CreateAnchor(_workingAnchor, true);
            }         
        }
    }

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

        

        // we save the saveable (green) anchors only
        if (saveAnchor)
        {
            //when ready, save the anchor.
            osAnchor.Save((anchor, success) =>
            {
                if (success)
                {
                    //save UUID to external storage so we can refer to the anchors in a later session
                    //SaveAnchorUuidToExternalStore(anchor);
                    //Debug.Log("Anchor " + osAnchor.Uuid.ToString() + " Saved!");
                    //keep tabs on anchors in local storage
                    //_allSavedAnchors.Add(anchor);
                }
            });
        }
    }

}
