using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour 
{

    
    
	void Start () 
    {
	
	}
	
	void Update () 
    {
	   
	}
    void OnCollisionEnter(Collision collision) 
    {        
        foreach (ContactPoint contact in collision.contacts)
        {
            print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
            
            
        }
    }
    
}
