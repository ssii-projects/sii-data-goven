using Agro.LibCore;
using GeoAPI.Geometries;


/*
yxm created at 2019/1/8 15:19:24
*/
namespace Agro.GIS
{
	public interface IGraphicsContainer : IGraphicsContainerSelect
	{
		void AddElement(IElement element);//, int zorder = 0);
		void DeleteElement(IElement element, bool fDispose = true);
		void DeleteAllElements(bool fDispose = true);
		bool ContainElement(IElement element);
		void LocateElements(IPoint Point, double Tolerance, Action<IElement> callback);
		//void LocateElementsByEnvelope(IDisplay dc, OkEnvelope Envelope, Action<IElement> callback);
		void UpdateElement(IElement element);
		void EnumElement(Action<IElement> callback);
		IElement FindElement(Func<IElement,bool> predicate);
	}

	/// <summary>
	/// 2019-1-3
	/// </summary>
	public interface IGraphicsContainerSelect
	{
		void SelectElement(IElement Element);
		void SelectElements(IEnumerable<IElement> Elements);
		void SelectAllElements();
		void UnselectElement(IElement Element);
		void UnselectElements(IEnumerable<IElement> Elements);
		void UnselectAllElements();
		IEnumerable<IElement> SelectedElements { get; }
		int ElementSelectionCount { get; }
		IElement SelectedElement(int Index);
		//ISelectionTracker GetSelectionTracker(int Index);
		bool IsElementSelected(IElement Element);
		IElement DominantElement { get; set; }
		OkEnvelope GetSelectionBounds();// IDisplay pIDisplay);
		IFrameElement FindFrame(object vt);
	}


	public class SymbolElement : ElementBase
	{
		public ISymbol Symbol
		{
			get;
			set;
		}
		public override void Draw(/*IDisplay dc,IDisplayTransformation trans,*/ICancelTracker cancel, OutputMode mode)
		{
			if (Symbol == null || Geometry == null || Geometry.IsEmpty || m_pIDisplay != null)
			{
				return;
			}
			try
			{
				var dc = m_pIDisplay!;
				var trans = _trans;// dc.DisplayTransformation;
				Symbol.SetupDC(dc, trans);
				Geometry.Project(trans?.SpatialReference);
				Symbol.Draw(Geometry);
				Symbol.ResetDC();
			}
			catch (Exception ex)
			{
				//Log.WriteLine(ex);
			}
		}
		public override IPolygon? QueryOutline()//IDisplay pIDisplay,IDisplayTransformation trans)
		{
			throw new NotImplementedException();
		}

	}


	public class GraphicsContainer : IGraphicsContainerSelectImpl, IGraphicsContainer,IDisposable
	{
		internal class ElementItem
		{
			public IElement Element;
			public int ZOrder;
		}
		internal readonly List<ElementItem> _elements = new();
		internal Action<ElementItem> OnElementAdded;

		public GraphicsContainer()
		{
			//base.OnElementSelectChanged+=()=>OnE
		}

		#region IGraphicsContainer
		public void AddElement(IElement element)//, int zorder = 0)
		{
			var ei = new ElementItem
			{
				Element = element,
				//ZOrder = zorder
			};
			_elements.Add(ei);
			//_elements.Sort((a, b) =>
			//{
			//	return a.ZOrder > b.ZOrder ? 1 : -1;
			//});
			OnElementAdded?.Invoke(ei);
		}
		public void DeleteElement(IElement element,bool fDispose=true)
		{
			_elements.RemoveAll(a =>
			{
				return a.Element == element;
			});
			if (fDispose && element is IDisposable d)
			{
				d.Dispose();
			}
		}
		public void DeleteAllElements(bool fDispose = true)
		{
			List<ElementItem> lst = null;
			if (fDispose)
			{
				lst = new List<ElementItem>(_elements);
			}
			_elements.Clear();
			if (lst != null)
			{
				foreach (var e in lst)
				{
					if (e.Element is IDisposable d)
					{
						d.Dispose();
					}
				}
			}
		}
		public bool ContainElement(IElement element)
		{
			return _elements.Find(a => { return a.Element == element; }) != null;
		}
		public void Render(/*IDisplay display,IDisplayTransformation trans, */ICancelTracker cancel, OutputMode mode)
		{
			foreach (var ei in _elements)
			{
				if (cancel.Cancel())
				{
					break;
				}
				ei.Element.Draw(/*display,trans,*/ cancel, mode);
			}
		}
		public void LocateElements(IPoint Point, double Tolerance, Action<IElement> callback)
		{
			if (Point == null)
				return;

			for (int i = _elements.Count - 1; i >= 0; --i)
			{
				var e = _elements[i].Element;
				var bHit = e.HitTest(Point.X, Point.Y, Tolerance);
				if (bHit)
				{
					callback(e);
					break;
				}
			}
		}
		//public void LocateElementsByEnvelope(IDisplay dc, OkEnvelope Envelope, Action<IElement> callback)
		//{
		//	var pIRO = GeometryUtil.MakePolygon(Envelope);
		//	for (int i = _elements.Count - 1; i >= 0; i--)
		//	{
		//		var e = _elements[i].Element;
		//		var geo = e.QueryOutline(dc,);
		//		if (pIRO.Contains(geo))
		//		{//if (!pIRO.Disjoint(geo)){
		//			callback(e);
		//		}
		//	}
		//}
		public void UpdateElement(IElement element)
		{
		}
		public IFrameElement FindFrame(object frameObject)
		{
			foreach(var it in _elements)
			{
				if (it.Element is IFrameElement pIFrameElement)
				{
					if (pIFrameElement.Object == frameObject)
					{
						return pIFrameElement;
					}
				}
			}
			return null;
		}
		public void EnumElement(Action<IElement> callback)
		{
			foreach (var e in _elements)
			{
				EnumElement(e.Element, callback);
			}
		}
		public IElement FindElement(Func<IElement,bool> predicate)
		{
			IElement find = null;
			foreach (var e in _elements)
			{
				var fContinue=EnumElement2(e.Element, el=>
				{
					if (predicate(el))
					{
						find = el;
						return false;
					}
					return true;
				});
				if (!fContinue)
				{
					break;
				}
			}
			return find;
		}
		#endregion

		#region IGraphicsContainerSelect
		public void SelectAllElements()
		{
			base.UnselectAllElements();
			foreach (var e in _elements)
			{
				base.SelectElement(e.Element);
			}
			FireSelectionChanged();
		}
		#endregion

		private static void EnumElement(IElement element, Action<IElement> callback)
		{
			callback(element);
			if (element is IGroupElement ge)
			{
				foreach (var e in ge.Elements)
				{
					EnumElement(e, callback);
				}
			}
		}
		private static bool EnumElement2(IElement element, Func<IElement,bool> callback)
		{
			var fContinue=callback(element);
			if (fContinue)
			{
				if (element is IGroupElement ge)
				{
					foreach (var e in ge.Elements)
					{
						if (!EnumElement2(e, callback))
						{
							return false;
						}
					}
				}
			}
			return fContinue;
		}

		public void Dispose()
		{
			foreach(var e in _elements)
			{
				if(e.Element is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}
	}

	public class IGraphicsContainerSelectImpl // : public IGraphicsContainerSelect
	{
		protected readonly List<IElement> m_arrISelectedElement = new List<IElement>();
		//protected IElement m_pIDominantElement;
		internal Action OnElementSelectChanged;
		public void SelectElement(IElement Element)
		{
			if (Element == null)
				return;
			m_arrISelectedElement.Add(Element);
			FireSelectionChanged();
		}
		public void SelectElements(IEnumerable<IElement> Elements)
		{
			if (Elements == null)
				return;

			foreach (var pIElement in Elements)
			{
				m_arrISelectedElement.Add(pIElement);
			}
			FireSelectionChanged();
		}
		//public void SelectAllElements()
		//{
		//	//		m_arrISelectedElement.Clear();
		//	//		vector<CAdapt<IElement>>>::iterator it;
		//	//		for (it=static_cast<PARENT*>(this)->m_arrIElement.begin(); it!=static_cast<PARENT*>(this)->m_arrIElement.end(); it++)
		//	//		{
		//	//	m_arrISelectedElement.push_back(IElement>((*it).m_T));
		//	//}
		//}
		public void UnselectElement(IElement Element)
		{
			if (Element == null)
				return;
			if (m_arrISelectedElement.Contains(Element))
			{
				m_arrISelectedElement.Remove(Element);
				FireSelectionChanged();
			}
		}
		public void UnselectElements(IEnumerable<IElement> Elements)
		{
			if (Elements == null)
				return;
			foreach (var pIElement in Elements)
			{
				UnselectElement(pIElement);
			}
			FireSelectionChanged();
		}
		public void UnselectAllElements()
		{
			m_arrISelectedElement.Clear();
			FireSelectionChanged();
		}
		public int ElementSelectionCount
		{
			get
			{
				return m_arrISelectedElement.Count();
			}
		}
		public IElement SelectedElement(int Index)
		{
			if (Index < 0 || Index >= m_arrISelectedElement.Count())
				return null;
			return m_arrISelectedElement[Index];
		}
		public IEnumerable<IElement> SelectedElements
		{
			get
			{
				return m_arrISelectedElement;
			}
		}
		public bool IsElementSelected(IElement Element)
		{
			return m_arrISelectedElement.Contains(Element);
		}
		public IElement DominantElement { get; set; }
		//public ISelectionTracker GetSelectionTracker(int Index)
		//{
		//	if (Index < 0 || Index >= m_arrISelectedElement.Count())
		//		return null;
		//	return m_arrISelectedElement[Index].GetSelectionTracker();
		//}
		public OkEnvelope? GetSelectionBounds()// IDisplay pIDisplay)
		{
			//if (pIDisplay == null)
			//	return null;
			var pIEnvelope = new OkEnvelope();
			if (m_arrISelectedElement.Count > 0)
			{
				pIEnvelope.Init(m_arrISelectedElement[0].QueryBounds());// pIDisplay););
			}
			for (int i = 1; i < m_arrISelectedElement.Count; i++)
			{
				var pIEnvelope2 = m_arrISelectedElement[i].QueryBounds();// pIDisplay);
				if (pIEnvelope2 != null && !pIEnvelope2.IsNull)
				{
					pIEnvelope.ExpandToInclude(pIEnvelope2);
				}
			}
			return pIEnvelope;
		}

		protected void FireSelectionChanged()
		{
			OnElementSelectChanged?.Invoke();
		}
	}
}
