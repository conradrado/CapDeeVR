using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class LidSnap : MonoBehaviour
{   
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private Rigidbody rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args){
        Debug.Log("뚜껑 잡음");
        rb.isKinematic = false;
        rb.useGravity = false;
    }

    void OnRelease(SelectExitEventArgs args){
        Debug.Log("뚜껑 놓음");
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
