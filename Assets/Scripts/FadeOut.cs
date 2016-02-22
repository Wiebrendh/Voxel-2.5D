using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOut : MonoBehaviour 
{

	[HideInInspector] public bool fade;
	[HideInInspector] public int fadeType;
	public float fadeSpeed;
	public bool disableIfZero;
	CanvasGroup cg;

	void Start ()
	{
		cg = this.GetComponent<CanvasGroup> ();
	}

	void Update () 
	{
		if (fade)
		{
			float temp = Mathf.Lerp (cg.alpha, fadeType, Time.deltaTime * fadeSpeed);
			cg.alpha = temp;

			if (disableIfZero && temp <= 0.01f)
			{
				this.gameObject.SetActive(false);
				cg.alpha = 1;
			}
		}
	}

}
