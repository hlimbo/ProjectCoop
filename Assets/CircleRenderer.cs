using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRenderer : MonoBehaviour {
	public int segments = 50;
	public float xradius;
	public float yradius;
	public LineRenderer line;
	public Vector3 origin;

	void Start ()
	{
		
		line = gameObject.AddComponent<LineRenderer>() as LineRenderer;
		line.SetVertexCount (segments + 1);
		line.widthMultiplier = 0.075f;
	}

	public void SetOrigin (Vector3 position, Color c){
		line.enabled = true;
		origin = position;
		line.endColor = c; //line.startColor = c;
	}

	public void CreatePoints ()
	{
		
		float x;
		float y;
		float z = 0f;

		float angle = 20f;

		for (int i = 0; i < (segments + 1); i++)
		{
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * xradius;
			y = Mathf.Cos (Mathf.Deg2Rad * angle) * yradius;

			line.SetPosition (i, origin + new Vector3 (x, y, z));

			angle += (360f / segments);
		}
	}
}