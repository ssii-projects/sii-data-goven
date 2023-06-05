using System;

namespace Agro.LibCore
{
	public interface ICancelable
	{
		bool Cancel { get; set; }
		//private bool _cancel;
		//public bool IsCancel() { return _cancel; }
		//public void SetCancel(bool cancel)
		//{
		//	_cancel = cancel;
		//}
	}
	public class Cancelable : ICancelable
	{
		public bool Cancel { get; set; }
	}
	public class CancelItem<T>: Cancelable
	{
		public T Item { get; set; }
		public CancelItem(T item = default)
		{
			Item = item;
		}
        public void OverWrite(CancelItem<T> rhs)
        {
            Item = rhs.Item;
            Cancel = rhs.Cancel;
        }

        //public static explicit operator CancelItem<T>(CancelItem<ITask> v)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
