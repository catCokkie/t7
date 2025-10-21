using Godot;
using System;

// 确保命名空间正确 (Godot 可能会自动设为 SilentTestimony.Global)
namespace SilentTestimony.Global
{
	/// <summary>
	/// 全局事件总线。
	/// 负责在各个解耦的系统之间传递核心游戏事件。
	/// </summary>
	public partial class GlobalEventBus : Node
	{
		// -----------------------------------------------------------------
		// 玩家相关事件 (Player Events)
		// -----------------------------------------------------------------
		
		/// <summary>
		/// 当玩家被AI发现时发出。
		/// </summary>
		/// <param name="enemyWhoSpotted">发现玩家的AI节点</param>
		[Signal] public delegate void PlayerSpottedEventHandler(Node2D enemyWhoSpotted);

		/// <summary>
		/// 当玩家的健康值改变时发出。
		/// </summary>
		/// <param name="newHealth">新的健康值</param>
		[Signal] public delegate void PlayerHealthChangedEventHandler(int newHealth);

		/// <summary>
		/// 当玩家的理智值改变时发出。
		/// </summary>
		/// <param name="newSanity">新的理智值</param>
		[Signal] public delegate void PlayerSanityChangedEventHandler(int newSanity);


		// -----------------------------------------------------------------
		// AI / 潜行相关事件 (AI / Stealth Events)
		// -----------------------------------------------------------------
		
		/// <summary>
		/// 当游戏中产生噪音时发出（例如：玩家跑步、物体倒塌）。
		/// AI系统会监听此事件。
		/// </summary>
		/// <param name="location">噪音的全局位置</param>
		/// <param name="radius">噪音的传播半径</param>
		/// <param name="instigator">噪音制造者 (可选, 可能是玩家或某个道具)</param>
		[Signal] public delegate void NoiseMadeEventHandler(Vector2 location, float radius, Node instigator = null);

		
		// -----------------------------------------------------------------
		// 叙事 / AVG 事件 (Narrative / AVG Events)
		// -----------------------------------------------------------------

		/// <summary>
		/// 当玩家获得新的证据时发出。
		/// </summary>
		/// <param name="evidenceID">证据的唯一ID</param>
		[Signal] public delegate void EvidenceFoundEventHandler(string evidenceID);

		/// <summary>
		/// 当关键对话选项被选择时发出。
		/// </summary>
		/// <param name="dialogueKey">对话的唯一标识符</param>
		/// <param name="choiceIndex">所选选项的索引</param>
		[Signal] public delegate void DialogueChoiceMadeEventHandler(string dialogueKey, int choiceIndex);
	}
}
