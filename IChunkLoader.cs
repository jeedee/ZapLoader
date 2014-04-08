using UnityEngine;
using System.Collections;

namespace ZapLoader
{
	/// <summary>
	/// The base interface for ZapLoader plugins
	/// </summary>
	public interface IChunkLoader {

		/// <summary>
		/// Loads a chunk at position
		/// </summary>
		/// <param name="position">Position.</param>
		void LoadChunk (Vector3 position);

		/// <summary>
		/// Destroy a chunk at position
		/// </summary>
		/// <param name="position">Position.</param>
		void DestroyChunk (Vector3 position);
	}
}
