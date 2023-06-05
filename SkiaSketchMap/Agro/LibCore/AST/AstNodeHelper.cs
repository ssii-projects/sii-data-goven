using Agro.GIS;
using Agro.LibCore;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace Agro.LibCore.AST
{

	public class RowEntity : AstEntityBase
    {
        private IRow _featrue;
        public RowEntity(IRow feature = null)
        {
            _featrue = feature;
        }
        public void SetFeature(IRow feature)
        {
            _featrue = feature;
        }

        public override object GetFieldValue(int iField)
        {
            return _featrue.GetValue(iField);
        }
        public override object GetPropertyValue(string propertyName)
        {
            if (_featrue != null)
            {
                var iField = _featrue.Fields.FindField(propertyName);
                return iField >= 0 ? _featrue.GetValue(iField) : null;
            }
            return null;
        }
        public override int FindField(string fieldName)
        {
            if (_featrue != null)
            {
                var iField = _featrue.Fields.FindField(fieldName);
                return iField;
            }
            return -1;
        }
        public override IGeometry GetGeometry()
        {
            return null;
        }
    }
    public class FeatureEntity : AstEntityBase
    {
        private IFeature _featrue;
        public FeatureEntity(IFeature feature = null)
        {
            _featrue = feature;
        }
        public void SetFeature(IFeature feature)
        {
            _featrue = feature;
        }
        public override object GetPropertyValue(string propertyName)
        {
            if (_featrue != null)
            {
                var iField = _featrue.Fields.FindField(propertyName);
                return iField >= 0 ? _featrue.GetValue(iField) : null;
            }
            return null;
        }
        public override int FindField(string fieldName)
        {
            if (_featrue != null)
            {
                var iField = _featrue.Fields.FindField(fieldName);
                return iField;
            }
            return -1;
        }
        public override IGeometry GetGeometry()
        {
            return _featrue == null ? null : _featrue.Shape;
        }
    }

    /// <summary>
    /// 绘制节点的辅助类
    /// </summary>
    public class DrawNodeHelper
    {
        /// <summary>
        /// 水平分割线的上下间隔
        /// </summary>
        public double VMargin { get; set; }
        /// <summary>
        /// 水平方向文本间的间隔
        /// </summary>
        public double HMargin = 2;// 1;
        public double _penWidth = 1;

        public Func<string, string> CallFieldValue{ get; set; }
        public DrawNodeHelper(Func<string, string>? callFieldValue = null)
        {
            if (callFieldValue == null)
            {
                CallFieldValue = (fieldName) =>
                {
                    return fieldName;
                };
            }
            else
            {
                CallFieldValue=callFieldValue;
            }
        }
        public MyRect CalcRect(ASTNode node, Func<string, MyRect> callTextRect,double dpi=0)
        {
            if (node == null)
                return null;
            //_vMargin = vMargin;
            MyRect rc = null;
            if (node is AstDoubleNode || node is AstStringNode || node is AstIntNode)
            {
                rc = callTextRect(node.ToString());
            }
            else if (node is AstFieldNode fieldNode)
            {
                rc = callTextRect(CallFieldValue(fieldNode.fieldName));
            }
            else if (node is AstOprNode oprNode)
            {
                double refScale = 0, mapScale = 1;
                var leftNode = oprNode.children[0];
                var rc1 = CalcRect(leftNode, callTextRect);
                var rightNode = oprNode.children[1];
                var rc2 = CalcRect(rightNode, callTextRect);
                rc = new MyRect();
                switch (oprNode.oper)
                {
                    case OperatorType.OPR_DIVIDE: //divide
                    case OperatorType.OPR_HLINE:
                        {
                            double iWidth = Math.Max(rc1.Width(), rc2.Width());
                            double nSpan = VMargin * 2;
                            nSpan = 0 == refScale ? nSpan : (nSpan * refScale / mapScale + .5f);
                            #region yxm 2018-6-22
                            double penWidth = _penWidth;
                            if (dpi != 0)
                            {
                                nSpan = (float)DpiUtil.POINTS2PIXELS2(nSpan, (float)dpi);
                                penWidth = DpiUtil.POINTS2PIXELS2(penWidth, (float)dpi);
                            }
                            #endregion
                            nSpan += penWidth;// Util.CalcTextRect("-", _typeFace).Height();
                            double iHeight = rc1.Height() + rc2.Height() + nSpan;
                            rc.SetRect(0, 0, iWidth, iHeight);
                            oprNode.rcOperator.SetRect(0, 0, iWidth, nSpan);
                            break;
                        }
                    default: //else
                        {
                            double nMargin = 0;
                            var rcOp = callTextRect("+");
                            int xn = oprNode.oper != OperatorType.OPR_CONCAT ? 2 : 1;
                            //if (oprNode.oper != OperatorType.OPR_CONCAT)
                            {
                                nMargin = (0 == refScale ? HMargin : (HMargin * refScale / mapScale + .5f)) * xn;
                                //nMargin += rcOp.Width();
                            }
                            double iWidth = rc1.Width() + rc2.Width() + nMargin;
                            double iHeight = Math.Max(rc1.Height(), rc2.Height());
                            rc.SetRect(0, 0, iWidth, iHeight);
                            oprNode.rcOperator.SetRect(0, 0, nMargin, rcOp.Height());
                            break;
                        }
                }
            }
            else if (node is AstFuncNode funcNode)
            {
                rc = callTextRect(funcNode.funcName + "(");
                funcNode.rcLeft.SetRect(rc);
                funcNode.rcRight.SetRect(callTextRect(")"));
                funcNode.rcComma.SetRect(callTextRect(","));
                rc.right += funcNode.rcRight.Width();
                for (var i = 0; i < funcNode.children.Count; ++i)
                {
                    var r = CalcRect(funcNode.children[i], callTextRect);
                    rc.right += r.Width();
                    rc.bottom = Math.Max(rc.bottom, r.bottom);
                    if (i > 0)
                    {

                        rc.right += funcNode.rcComma.Width();// calcTextRect(",").Width();
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            node.rc.SetRect(rc);
            return rc;
        }

        public void Draw(ASTNode node, double centerX, double centerY, bool bDrawDivider
            , Action<string, MyRect> onDrawText
            , Action<Point, Point> onDrawLine,double dpi=0)
        {
            if (node == null)
                return;
            var rc = new MyRect(node.rc);
            double offsetX = centerX - rc.Width() / 2;
            double offsetY = centerY - rc.Height() / 2;
            rc.OffsetRect(offsetX, offsetY);
            if (node is AstDoubleNode || node is AstIntNode || node is AstStringNode)
            {
                onDrawText(node.ToString(), rc);
            }
            else if (node is AstFieldNode fieldNode)
            {
                onDrawText(CallFieldValue(fieldNode.fieldName), rc);
            }
            else if (node is AstOprNode opNode)
            {
                var leftNode = opNode.children[0];
                var rightNode = opNode.children[1];
                switch (opNode.oper)
                {
                    case OperatorType.OPR_DIVIDE: //divide
                    case OperatorType.OPR_HLINE:
                        if (bDrawDivider && rc.Width() > 2)
                        {
                            double x = (rc.left + rc.right) / 2;
                            double y = rc.top + leftNode.rc.Height() / 2;
                            Draw(leftNode, x, y, bDrawDivider, onDrawText, onDrawLine);
                            y = rc.bottom - rightNode.rc.Height() / 2;
                            Draw(rightNode, x, y, bDrawDivider, onDrawText, onDrawLine);

                            #region yxm 2018-6-22
                            var vMargin = VMargin;
                            if (dpi != 0)
                            {
                                vMargin = (float)DpiUtil.POINTS2PIXELS2(vMargin, (float)dpi);
                            }
                            #endregion
                            var y1 = rc.top + leftNode.rc.Height();// + VMargin;
                            var y2 = rc.bottom - rightNode.rc.Height();
                            y = (y1 + y2) / 2; // (rc.top + rc.bottom) / 2;
                            var p0 = new Point((int)rc.left, (int)y);
                            var p1 = new Point((int)rc.right, (int)y);
                            onDrawLine(p0, p1);
                        }
                        break;
                    default: //else
                        {
                            double y = (rc.top + rc.bottom) / 2;
                            Draw(leftNode, rc.left + leftNode.rc.Width() / 2, y, bDrawDivider, onDrawText, onDrawLine);
                            Draw(rightNode, rc.right - rightNode.rc.Width() / 2, y, bDrawDivider, onDrawText, onDrawLine);

                            if (opNode.oper != OperatorType.OPR_CONCAT) // concat;
                            {
                                if (rc.Width() > 0)
                                {
                                    string ch = null;
                                    if (opNode.oper == OperatorType.OPR_PLUS)
                                        ch = "+";
                                    else if (opNode.oper == OperatorType.OPR_MINUS)
                                        ch = "-";
                                    else
                                        ch = "*";
                                    var r = new MyRect(rc.left + leftNode.rc.Width() + HMargin, y - opNode.rcOperator.Height() / 2
                                        ,opNode.rcOperator.Width(),opNode.rcOperator.Height()
                                        );
                                    onDrawText(ch, r);
                                }
                            }
                            break;
                        }
                }
            }
            else if (node is AstFuncNode funcNode)
            {
                string s = funcNode.funcName + "(";
                var rc1 =new MyRect(funcNode.rcLeft);// calcTextRect(s);
                rc1.CenterAt(rc.left + rc1.Width() / 2, centerY);
                onDrawText(s, rc1);
                double x = rc1.right;
                for (var i = 0; i < funcNode.children.Count; ++i)
                {
                    if (i > 0)
                    {
                        s = ",";
                        rc1.SetRect(funcNode.rcComma);// = calcTextRect(s);
                        rc1.CenterAt(x + rc1.Width() / 2, centerY);
                        onDrawText(s, rc1);
                        x += rc1.Width();
                    }
                    var cn = funcNode.children[i];
                    Draw(cn, x + cn.rc.Width() / 2, centerY, bDrawDivider, onDrawText, onDrawLine);
                    x += cn.rc.Width();
                }
                s = ")";
                rc1.SetRect(funcNode.rcRight);// = calcTextRect(s);
                rc1.CenterAt(rc.right - rc1.Width() / 2, centerY);
                onDrawText(s, rc1);
            }
        }
    }

    /// <summary>
    /// 简化节点的辅助类
    /// </summary>
    public class SymplifyNodeHelper
    {
        /// <summary>
        ///  简化语法树：将原始的语法树简化为一个可以用于显示的简单语法树；
        ///  简化规则：
        ///     1.通过求值将函数节点和算术表达式节点简化为基本节点；
        ///     2.对语句的简化规则：依序对每个语句进行简化直到找到一个满足标注条件的结果；
        /// can return null
        /// </summary>
        /// <param name="node"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static ASTNode Simplify(ASTNode node, AstEntityBase feature)
        {
            try
            {
                if (node is AstDoubleNode || node is AstIntNode || node is AstStringNode || node is AstFieldNode)
                {
                    return node;
                }
                else if (node is AstOprNode opNode)
                {
                    var fn = AstFuncHelper.Convert(opNode);
					if (fn != null)
					{
						return AstFunctorFactory.GetFunctor(fn.funcName).Simplify(fn, feature);
					}
					else
					{
						
						for (var i = 0; i < opNode.children.Count; ++i)
						{
							var n = opNode.children[i];
							opNode.children[i] = Simplify(n, feature);
						}
						return opNode;
					}
				}else if (node is AstFuncNode funNode)
                {
                    return AstFunctorFactory.GetFunctor(funNode.funcName).Simplify(funNode, feature);
                }
                else if (node is AstStatementNode stNode)
                {
                    foreach (var n in stNode.children)
                    {
                        var pn=Simplify(n, feature);
                        if (pn != null)
                            return pn;
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return null;
        }
    }

    public class PredicateNodeHelper
    {
        private ASTNode _node;
        private IRow _row;
        private bool _fOK = false;
        private readonly RowEntity _featureEntity = new RowEntity();

        public readonly HashSet<string> fields = new HashSet<string>();

        public HashSet<string> ParseFields(ASTNode node)
        {
            _node = node;
            fields.Clear();
            DoParseFields(node);
            return fields;
        }
        internal void UpdateRow(IRow row)
        {
            _row = row;
        }
        public bool Prepare(IRow r)
        {
            _row = r;
            _fOK = true;
            Prepare(_node);
            return _fOK;
        }

        public bool IsOK()
        {
            if (_fOK)
            {
                var o=Evaluate(_node);
				if (o is bool b)
				{
					return b;
				}
				else if (o is int i)
					return i != 0;
				else if (o is string s)
					return s != "0";
            }
            return false;
        }


        private void Prepare(ASTNode node)
        {
            if (!_fOK)
            {
                return;
            }
            if (node is AstFieldNode fieldNode)
            {
                var fieldName = fieldNode.fieldName;
                var n=_row.Fields.FindField(fieldName);
                if (n < 0)
                {
                    _fOK = false;
                    return;
                }
                fieldNode.fieldIndex = n;
            }
            else if (node is AstOprNode oprNode)
            {
				oprNode.children.ForEach(n => Prepare(n));
            }
            else if (node is AstFuncNode fn)
            {
				fn.children.ForEach(n => Prepare(n));
            }
            else
            {
                //System.Diagnostics.Debug.Assert(false);
            }
        }

        private object Evaluate(ASTNode node)
        {
            if (node == null )
                return null;
            var r = _row;
			if (node is AstOprNode aon)
			{
				node = AstFuncHelper.Convert(aon);
				System.Diagnostics.Debug.Assert(node != null);
			}

			if (node is AstDoubleNode || node is AstStringNode || node is AstIntNode)
			{
				return node.GetValue();
			}
			else if (node is AstFieldNode fieldNode)
			{
				var n = fieldNode.fieldIndex;
				var o = r.GetValue(n);
				return o;
			}
			else if (node is AstFuncNode funcNode)
			{
				var fct = AstFunctorFactory.GetFunctor(funcNode.funcName);
				if (fct != null)
				{
					_featureEntity.SetFeature(r);
					var o = fct.eval(funcNode, _featureEntity);
					return o;
				}
			}
			else
			{
				//System.Diagnostics.Debug.Assert(false);
			}
			return null;
        }

        private void DoParseFields(ASTNode node)
        {
            if (node is AstFieldNode)
            {
                var fieldNode = node as AstFieldNode;
                var fieldName = fieldNode.fieldName;
                fields.Add(fieldName);
            }
            else if (node is AstOprNode oprNode)
			{
				oprNode.children.ForEach(n => DoParseFields(n));
			}
			else if (node is AstFuncNode fn)
			{
				fn.children.ForEach(n => DoParseFields(n));
			}
			else
			{
				// System.Diagnostics.Debug.Assert(false);
			}
		}
    }
}
