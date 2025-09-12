using UnityEngine;

public class HandData : MonoBehaviour
{
    public enum HandModelType {left, right};

    public Transform root;
    public Animator animator;
    public Transform[] fingers;

    public HandModelType modelType;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
