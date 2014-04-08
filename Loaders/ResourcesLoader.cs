using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZapLoader
{
	class ResourcesLoader : MonoBehaviour, IChunkLoader {

		Dictionary<Vector3, GameObject> chunks = new Dictionary<Vector3, GameObject>();

		void IChunkLoader.LoadChunk (Vector3 position)
		{
			print ("Load Chunk " + position);
		}

		void IChunkLoader.DestroyChunk (Vector3 position)
		{
			print ("Destroy Chunk " + position);
		}
	}
}
