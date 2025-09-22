using UnityEngine;

public class helpMsgTrigger : MonoBehaviour
{

    public Canvas canv;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")){
            canv.enabled = true;
            Debug.Log("yo!!");
        }
    }

    private void OnTriggerExit(Collider other){
        if(other.CompareTag("Player")){
            canv.enabled = false;
        }
    }
    // Update is called once per frame
}
