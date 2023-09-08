using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{

    void OnInteract();
    void OnLocationEnter();
    void OnLocationExit();
    
    void OnMouseDown();
    void OnMouseOver();
    void OnMouseExit();

}
