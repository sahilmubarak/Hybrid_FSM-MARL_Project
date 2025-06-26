using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDetectorColloider : MonoBehaviour
{                         
    // Public list to hold references to all GameObjects currently within the trigger.
    public List<GameObject> detectedAisles = new List<GameObject>();
    public List<string> detectedAisleNames = new List<string>();

    private ShopperAgentController shopper;
    private void Start()
    {
        shopper = GetComponentInParent<ShopperAgentController>();
    }

    // Called when an object enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        // Add the GameObject if it's not already in the list.
        if (!detectedAisles.Contains(other.gameObject))
        {
            string tag = other.gameObject.tag;
            string name = other.gameObject.name;
            switch (tag)
            {
                //Destract the agent when a new aisle enters the radius
                default:
                    // If the tag isnt an aisle dont to anything
                    break;
                case "Aisle_Fresh":
                    detectedAisles.Add(other.gameObject); 
                    detectedAisleNames.Add(name);
                    shopper.DistractShopper(0, name);
                    break;          
                case "Aisle_Essentials":
                    detectedAisles.Add(other.gameObject);
                    shopper.DistractShopper(1, name);
                    detectedAisleNames.Add(name);
                    break;          
                case "Aisle_Others":
                    detectedAisles.Add(other.gameObject);
                    shopper.DistractShopper(2, name);
                    detectedAisleNames.Add(name);
                    break;          
                case "Aisle_Offers":
                    detectedAisles.Add(other.gameObject);
                    shopper.DistractShopper(3, name);
                    detectedAisleNames.Add(name);
                    break;
            }
        }
    }

    // Called when an object exits the trigger.
    private void OnTriggerExit(Collider other)
    {
        // Remove the GameObject if it exists in the list.
        if (detectedAisles.Contains(other.gameObject))
        {
            detectedAisles.Remove(other.gameObject);
            detectedAisleNames.Remove(other.gameObject.name);
        }
    }      
}
