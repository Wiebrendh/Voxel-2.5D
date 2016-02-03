using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour
{

    public GameObject player;
    public float smoothFollowSpeed;
	
	void Update ()
    {
        Vector3 wantedPos = new Vector3(player.transform.position.x, player.transform.position.y + 2.5f, -12.5f);
        this.transform.position = Vector3.Lerp(this.transform.position, wantedPos, smoothFollowSpeed * Time.deltaTime);
	}
}
