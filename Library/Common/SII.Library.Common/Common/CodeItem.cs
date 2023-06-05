using Agro.LibCore.UI;

namespace Agro.Library.Common
{
	public class CodeItem:ICodeItem
	{
		public string Bm { get; set; }
		public string Mc { get; set; }
		public object Tag;
		public CodeItem(string bm = null, string mc = null,string tag=null)
		{
			Bm = bm;
			Mc = mc;
			Tag = tag;
		}
		public object GetCode()
		{
			return Bm;
		}

		public string GetName()
		{
			return Mc;
		}
		public override string ToString()
		{
			return Mc;
		}
	}
}
