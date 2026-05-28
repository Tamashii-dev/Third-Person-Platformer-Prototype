using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class InventoryUi : MonoBehaviour
{
    private TextMeshProUGUI diamondText;

    //Get the text component 
    void Start()
    {
        diamondText = GetComponent<TextMeshProUGUI>();
    }
    // updates the code for the playerInventory to update the diamondText
    public void UpdateDiamondText(PlayerInventory playerInventory)
    {
        diamondText.text = playerInventory.NumberOfDiamonds.ToString();
    }
}