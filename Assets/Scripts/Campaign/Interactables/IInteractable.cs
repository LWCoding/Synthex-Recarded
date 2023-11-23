using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{

    /// <summary>
    ///  This is a comment.
    /// </summary>
    void OnInteract();
    void OnLocationEnter();
    void OnLocationExit();
    
    void OnMouseDown();
    void OnMouseOver();
    void OnMouseExit();

}
