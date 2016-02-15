using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{

    public GameObject target;
    public float smoothFollowSpeed;
	
	void Update ()
    {
		// Return and search for player if there is none
		if (!target)
		{
			target = GameObject.Find("Player");
			return;
		}

        // Store position in temp position
		Vector3 wantedPos = new Vector3(target.transform.position.x, target.transform.position.y + 1.5f, -30f);
        
		// Add limits
		if (wantedPos.x < 14.055f)
			wantedPos.x = 14.055f;

        // Lerp to position
		this.transform.position = Vector3.Lerp (this.transform.position, wantedPos, smoothFollowSpeed * Time.deltaTime);
	}
}
