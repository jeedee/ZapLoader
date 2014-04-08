using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZapLoader
{
	class CubeLoader : MonoBehaviour, IChunkLoader {

		Dictionary<Vector3, GameObject> cubes = new Dictionary<Vector3, GameObject>();

		void IChunkLoader.LoadChunk (Vector3 position)
		{
			GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
			chunk.transform.localScale = new Vector3(ZapLoader.instance.chunkSize, ZapLoader.instance.chunkSize, ZapLoader.instance.chunkSize);
			chunk.transform.position = position;
			chunk.name = position.ToString();

			cubes.Add(position, chunk);
		}

		void IChunkLoader.DestroyChunk (Vector3 position)
		{
			Destroy (cubes[position]);
			cubes.Remove(position);
		}
	}
}
