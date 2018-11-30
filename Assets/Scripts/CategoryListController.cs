using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CategoryListController : MonoBehaviour {

    public PanelController[] PanelList;

    private Dictionary<string, string> CategoryTitleToId = new Dictionary<string, string>
    {
        { "Music" , "music" },
        { "Nature Scenes", "relaxing%20nature" },
        { "Car Racing" ,"car%20racing" },
        { "Comedy", "comedy" },
        { "News", "canadian%20news"},
        { "Basketball", "basketball" },
        { "Soccer", "soccer" },
        { "Hockey", "hockey" },
        { "Gaming", "gaming" },
        { "Cooking", "cooking" }
    };
    // Use this for initialization
    void Start () {
        SetupCategories();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void SetupCategories()
    {

    }
}
