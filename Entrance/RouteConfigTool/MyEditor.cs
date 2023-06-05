using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Windows.Threading;


/*
yxm created at 2019/6/4 16:50:58
*/
namespace RouteConfigTool
{
	public class MyEditor:TextEditor
	{
		private TextEditor textEditor { get { return this; } }
		public MyEditor()
		{
			base.ShowLineNumbers = true;
			this.Loaded += (s, e) =>
			  {
				  InitFolding();

				  DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
				  foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
				  foldingUpdateTimer.Tick += foldingUpdateTimer_Tick;
				  foldingUpdateTimer.Start();
			  };
		}

		#region Folding
		FoldingManager foldingManager;
		AbstractFoldingStrategy foldingStrategy;

		//void HighlightingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//	InitFolding();
		//}
		void InitFolding()
		{
			if (textEditor.SyntaxHighlighting == null)
			{
				foldingStrategy = null;
			}
			else
			{
				switch (textEditor.SyntaxHighlighting.Name)
				{
					case "XML":
						foldingStrategy = new XmlFoldingStrategy();
						textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
						break;
					case "C#":
					case "C++":
					case "PHP":
					case "Java":
						textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(textEditor.Options);
						foldingStrategy = new BraceFoldingStrategy();
						break;
					default:
						textEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.DefaultIndentationStrategy();
						foldingStrategy = null;
						break;
				}
			}
			if (foldingStrategy != null)
			{
				if (foldingManager == null)
					foldingManager = FoldingManager.Install(textEditor.TextArea);
				foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
			}
			else
			{
				if (foldingManager != null)
				{
					FoldingManager.Uninstall(foldingManager);
					foldingManager = null;
				}
			}
		}

		void foldingUpdateTimer_Tick(object sender, EventArgs e)
		{
			if (foldingStrategy != null)
			{
				foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
			}
		}
		#endregion
	}
}
