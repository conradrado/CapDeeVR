using TMPro;
using UnityEngine;

public class VerticalDoor : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private BoxCollider _boxCollider;



     // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
        {
            _anim.ResetTrigger("Close");
            _anim.SetTrigger("Open");
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player"))
        {
            _anim.ResetTrigger("Open");
            _anim.SetTrigger("Close");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
