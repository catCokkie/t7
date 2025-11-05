using Godot;
using SilentTestimony.Interfaces;
using SilentTestimony.UI;

namespace SilentTestimony.World
{
	public partial class TestNote : StaticBody2D, IInteractable
	{
		[Export] public string Title = "测试笔记";
		[Export(PropertyHint.MultilineText)] public string Content = "这是一条用于测试的笔记内容。";

		public string GetInteractPrompt()
		{
			return TranslationServer.Translate("ui.read_note");
		}

		public void Interact(Node2D interactor)
		{
			var reader = GetNodeOrNull<NoteReaderManager>("/root/NoteReaderManager");
			reader?.ShowNote(Title, Content);
		}
	}
}
