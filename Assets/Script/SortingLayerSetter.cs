using UnityEngine;
using System.Collections;

public class SortingLayerSetter : MonoBehaviour {

    [SerializeField]
    private string _sortingLayerName = "Default";

    // Use this for initialization
    void Start () {
        GetComponent<Renderer>().sortingLayerName = _sortingLayerName;
    }
}
