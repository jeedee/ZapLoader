using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZapLoader
{
	public class ZapAgent : MonoBehaviour {
		/// <summary>
		/// The radius of which the agent will be able to load chunks
		/// </summary>
		public Vector3 radius = Vector3.one;
		public Vector3 boundCenter
		{
			get
			{
				if (!useCameraBounds)
				{
					return this.transform.position;
				}else{
					return cameraRenderCenter;
				}
			}
		}

		/// <summary>
		/// How often this agent will query the chunk index
		/// </summary>
		public float pollRate = 0.01f;
		float _currentTime = 0.0f;

		/// <summary>
		/// Use camera bounds? (If Camera)
		/// </summary>
		public bool useCameraBounds = false;
		Vector3 cameraRenderCenter;

		void OnDrawGizmos ()
		{
			if (useCameraBounds) 
				CameraBounds();

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(boundCenter, radius);
		}

		void CameraBounds ()
		{
			// Abort if no camera is present on the agent gameobject
			if (this.GetComponent<Camera>() == null)
			{
				Debug.LogWarning("You are trying to render bounds of a gameobject without a camera component, skipping.");
				return;
			}

			// Find the center of the camera
			cameraRenderCenter = transform.position + new Vector3(0, 0, GetComponent<Camera>().farClipPlane/2);

			// Set the radius
			float fovHeight = 2.0f * this.GetComponent<Camera>().farClipPlane * Mathf.Tan(GetComponent<Camera>().fieldOfView * 0.5f * Mathf.Deg2Rad);

			radius.x = fovHeight * GetComponent<Camera>().aspect;
			radius.y = fovHeight;
			radius.z = GetComponent<Camera>().farClipPlane;
		}

		void Update ()
		{
			// Use Camera bounds if set
			if (useCameraBounds) 
				CameraBounds();

			if (_currentTime < pollRate)
			{
				_currentTime += Time.deltaTime;
			}else{
				ZapLoader.instance.RenderChunks(boundCenter, radius);
				_currentTime = 0;
			}
		}
	}
}
