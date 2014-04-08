using System;
using UnityEngine;
using ZapLoader;
using System.Collections;
using System.Collections.Generic;

namespace ZapLoader
{
	public class ZapLoader : MonoBehaviour {
		/// <summary>
		/// This structure keeps the loaded chunks in memory and store the last time they have been requested
		/// </summary>
		Dictionary<Vector3, float> loadedChunks = new Dictionary<Vector3, float>();
		public bool debugChunks = false;

		/// <summary>
		/// List of chunks to be deleted
		/// </summary>
		List<Vector3> obsoleteChunks = new List<Vector3>();

		/// <summary>
		/// How long before unused chunks gets deleted (in seconds) -- Has to be higher than the agents refresh or it might prune the chunk before the agent re-requests it
		/// </summary>
		public float chunkCheckTimer = 2.0f;

		/// <summary>
		/// How often the chunk destroying happens
		/// </summary>
		public float pruneLoopInterval = .1f;

		/// <summary>
		/// The size of the chunks.
		/// </summary>
		public float _chunkSize = 1000.0f;
		public float chunkSize
		{
			get
			{
				return _chunkSize;
			}
		}

		/// <summary>
		/// Singleton pointing to ZapLoader in the scene. If it does not exist, will create a blank gameobject with the ZapLoader attached.
		/// </summary>
		public static ZapLoader _instance;
		public static ZapLoader instance
		{
			get
			{
				if (_instance == null)
				{
					GameObject go = new GameObject("#ZapLoader");
					_instance = go.AddComponent<ZapLoader>();
				}

				return _instance;
			}
		}

		/// <summary>
		/// A class that implements the IChunkLoader interface (Plugins for loading chunks)
		/// </summary>
		IChunkLoader chunkLoader;
		public string chunkLoadingPlugin;

		// Negative zero workaround
		private static readonly long NegativeZeroBits = BitConverter.DoubleToInt64Bits(-0.0);

		void Awake ()
		{
			// If no instance has been set when this object awakened
			if (_instance == null)
			{
				_instance = this;
			}else{
				Debug.LogWarning("An object with ZapLoader has been instantiated but an instance has been created already. The component will be removed from this object");
				Destroy (this);
			}

			// Creates an instance of the ChunkLoader
			try
			{
				chunkLoader = (IChunkLoader)gameObject.AddComponent(chunkLoadingPlugin);
			}catch{
				Debug.LogError("An error occured while loading the chunk plugin.");
			}

		}

		void Start ()
		{
			StartCoroutine("PruneChunks");
		}

		void Update ()
		{
			CheckChunks(Time.time);
		}

		void OnDrawGizmos ()
		{
			if (!debugChunks)
				return;
			//--
			Vector3 chunkRadius = new Vector3(chunkSize, chunkSize, chunkSize);
			foreach(KeyValuePair<Vector3, float> chunk in loadedChunks)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireCube(chunk.Key, chunkRadius);
			}

			foreach(Vector3 chunk in obsoleteChunks)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(chunk, chunkRadius);
			}
		}

		/// <summary>
		/// Check which chunks should be in the pruning queue
		/// </summary>
		/// <param name="currentTime">Current time.</param>
		void CheckChunks(float currentTime)
		{
			foreach(KeyValuePair<Vector3, float> chunk in loadedChunks)
			{
				if ( (currentTime - chunk.Value) >= chunkCheckTimer)
				{
					if (!obsoleteChunks.Contains(chunk.Key))
						obsoleteChunks.Add(chunk.Key);
				}
			}
		}

		IEnumerator PruneChunks ()
		{
			for(;;)
			{
				if (obsoleteChunks.Count > 0)
				{
					// Make sure our chunk is still present
					if (loadedChunks.ContainsKey(obsoleteChunks[0]))
					{
						Vector3 key = obsoleteChunks[0];
						// Verify the timer is still valid
						float chunkTime = loadedChunks[key];
						if ( (Time.time - chunkTime) >= chunkCheckTimer)
						{
							// Ask the loader to destroy
							chunkLoader.DestroyChunk(key);

							// Remove the chunk from the queue
							loadedChunks.Remove(key);
						}
					}

					// We processed this chunk, remove it from obsolete chunks
					obsoleteChunks.RemoveAt(0);
				}

				yield return new WaitForSeconds(pruneLoopInterval);
			}
		}

		/// <summary>
		/// Renders the chunk content at position+radius (Using the plugin of your choice)
		/// </summary>
		/// <param name="at">Position to generate the chunk.</param>
		/// <param name="radius">Radius of the requesting object (In case it overlaps multiple chunks)</param>
		public void RenderChunks (Vector3 at, Vector3 radius)
		{
			radius /= 2;

			// Calculate bounds
			for (float x = RoundUpToChunk(at.x-radius.x); x <= RoundUpToChunk(at.x+radius.x); x+= chunkSize)
			{
				for (float y = RoundUpToChunk(at.y-radius.y); y <= RoundUpToChunk(at.y+radius.y); y+= chunkSize)
				{
					for (float z = RoundUpToChunk(at.z-radius.z); z <= RoundUpToChunk(at.z+radius.z); z += chunkSize)
					{
						Vector3 key = new Vector3(x,y,z);

						// Ask the loader to create this chunk if it's not rendered
						if (!loadedChunks.ContainsKey(key))
							chunkLoader.LoadChunk(key);

						// Add the chunk to the list of chunks to be loaded, along with the time it was requested
						loadedChunks[key] = Time.time;
					}
				}
			}
		}

		// TODO: Find a cleaner way to eliminate negative zeros
		float RoundUpToChunk (float number)
		{
				number = chunkSize * Mathf.Round(number / chunkSize);

			if (BitConverter.DoubleToInt64Bits(number) == NegativeZeroBits)
			{
				return 0;
			}else{
				return number;
			}
		}


	}
}
