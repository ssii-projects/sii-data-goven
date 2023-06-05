namespace Agro.LibCore.AST
{
	public interface IAstFunctor
    {
        object eval(AstFuncNode node, AstEntityBase feature);
        ASTNode Simplify(AstFuncNode node, AstEntityBase feature);
    }

    public class AstFunctorFactory
    {
        private static readonly Dictionary<string, IAstFunctor> _dic = new Dictionary<string, IAstFunctor>();
        static AstFunctorFactory()
        {
            AstFunctorFactory.RegisterFunc("round", new RoundFunc());
            AstFunctorFactory.RegisterFunc("mul", new MulFunc());
            AstFunctorFactory.RegisterFunc("div", new DivFunc());
            AstFunctorFactory.RegisterFunc("add", new AddFunc());
            AstFunctorFactory.RegisterFunc("sub", new SubFunc());
            AstFunctorFactory.RegisterFunc("StrMid", new StrMidFunc());
			AstFunctorFactory.RegisterFunc("Right", new StrRightFunc());

			#region 操作符类型
			AstFunctorFactory.RegisterFunc("$question", new QuestionFunc());
            AstFunctorFactory.RegisterFunc("$equal", new EqualFunc());
			AstFunctorFactory.RegisterFunc("$notequal",new NotFunc(new EqualFunc()));
			AstFunctorFactory.RegisterFunc("$great", new GreatFunc());
            AstFunctorFactory.RegisterFunc("$greatEqual", new GreatEqualFunc());
            AstFunctorFactory.RegisterFunc("$less", new LessFunc());
            AstFunctorFactory.RegisterFunc("$lessEqual", new LessEqualFunc());
            AstFunctorFactory.RegisterFunc("$and", new AndFunc());
            AstFunctorFactory.RegisterFunc("$or", new OrFunc());
            AstFunctorFactory.RegisterFunc("$stmt", new StatementFunc());
            AstFunctorFactory.RegisterFunc("$isnull", new IsNullFunc());
			AstFunctorFactory.RegisterFunc("$isnotnull",new NotFunc(new IsNullFunc()));
			AstFunctorFactory.RegisterFunc("$not", new NotFunc());
			AstFunctorFactory.RegisterFunc("$like", new LikeFunc());
			AstFunctorFactory.RegisterFunc("$in", new InFunc());
			AstFunctorFactory.RegisterFunc("$notin",new NotFunc(new InFunc()));
			AstFunctorFactory.RegisterFunc("$notlike",new NotFunc(new LikeFunc()));
			#endregion

			AstFunctorFactory.RegisterFunc("StrLen", new StrLenFunc());
            AstFunctorFactory.RegisterFunc("StrContains", new StrContainsFunc());

            AstFunctorFactory.RegisterFunc("ShapeArea", new ShapeAreaFunc());
        }
        public static IAstFunctor GetFunctor(string funcName)
        {
            if (_dic.TryGetValue(funcName, out IAstFunctor f))
                return f;
            throw new Exception("使用了未定义的函数：" + funcName);
        }
        public static void RegisterFunc(string funcName, IAstFunctor fctor)
        {
            _dic[funcName] = fctor;
        }
        public static void UnRegisterFunc(string funcName)
        {
            _dic.Remove(funcName);
        }
    }
    public class AstFuncHelper
    {
		public static AstFuncNode Convert(AstOprNode oprNode)
		{
			AstFuncNode fn = null;
			switch (oprNode.oper)
			{
				case OperatorType.OPR_MULT:
					fn = new AstFuncNode("mul"); break;
				case OperatorType.OPR_DIVIDE:
					fn = new AstFuncNode("div"); break;
				case OperatorType.OPR_PLUS:
					fn = new AstFuncNode("add"); break;
				case OperatorType.OPR_MINUS:
					fn = new AstFuncNode("sub"); break;
				case OperatorType.OPR_QUESTION:
					fn = new AstFuncNode("$question"); break;
				case OperatorType.OPR_EQUAL:
					fn = new AstFuncNode("$equal"); break;
				case OperatorType.OPR_NOTEQUAL:
					fn = new AstFuncNode("$notequal");break;
				case OperatorType.OPR_GREAT:
					fn = new AstFuncNode("$great"); break;
				case OperatorType.OPR_GREATTHAN:
					fn = new AstFuncNode("$greatEqual"); break;
				case OperatorType.OPR_LESS:
					fn = new AstFuncNode("$less"); break;
				case OperatorType.OPR_LESSTHAN:
					fn = new AstFuncNode("$lessEqual"); break;
				case OperatorType.OPR_AND:
					fn = new AstFuncNode("$and"); break;
				case OperatorType.OPR_OR:
					fn = new AstFuncNode("$or"); break;
				case OperatorType.OPR_STATEMENT:
					fn = new AstFuncNode("$stmt"); break;
				case OperatorType.OPR_ISNULL:
					fn = new AstFuncNode("$isnull"); break;
				case OperatorType.OPR_ISNOTNULL:
					fn = new AstFuncNode("$isnotnull"); break;
				case OperatorType.OPR_NOT:
					fn = new AstFuncNode("$not");break;
				case OperatorType.OPR_LIKE:
					fn = new AstFuncNode("$like");break;
				case OperatorType.OPR_IN:
					fn = new AstFuncNode("$in");break;
				case OperatorType.OPR_NOTIN:
					fn = new AstFuncNode("$notin");break;
				case OperatorType.OPR_NOTLIKE:
					fn = new AstFuncNode("$notlike");break;
			}
            if (fn != null)
            {
                fn.rc.SetRect(oprNode.rc);
				oprNode.children.ForEach(n => fn.children.Add(n));
            }
            return fn;
        }
    }
    public abstract class AstFunctor : IAstFunctor
    {
        public virtual ASTNode Simplify(AstFuncNode node, AstEntityBase feature)
        {
            return node;
        }

		public abstract object eval(AstFuncNode node, AstEntityBase feature);

		protected bool IsEqual(object ol, object or)
		{
			if (or != null && ol != null)
			{
				return ol.ToString() == or.ToString();
			}
			else if (or == null && ol == null)
			{
				return true;
			}
			return false;
		}
    }

    /// <summary>
    /// ?条件函数
    /// 示例：1>0?'ok'
    /// </summary>
    public class QuestionFunc : AstFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            if (node.children.Count != 2)
            {
                throw new Exception("question函数接受2个参数！");
            }
            var o = AstEvaluate.Eval(node.children[0], feature);
            var b = SafeConvertAux.ToDouble(o);
            if (b != 0)
            {
                return AstEvaluate.Eval(node.children[1], feature);
            }
            return null;
        }
        public override ASTNode Simplify(AstFuncNode node, AstEntityBase feature)
        {
            if (node.children.Count != 2)
            {
                throw new Exception("question函数接受2个参数！");
            }
            var o = AstEvaluate.Eval(node.children[0], feature);
            var b = SafeConvertAux.ToDouble(o);
            if (b != 0)
            {
                return SymplifyNodeHelper.Simplify(node.children[1], feature);
            }
            return null;
        }
    }

    /// <summary>
    /// 语句函数
    /// </summary>
    public class StatementFunc : AstFunctor
    {
		public override object eval(AstFuncNode node, AstEntityBase feature)
		{
			throw new NotImplementedException();
		}

		public override ASTNode Simplify(AstFuncNode node, AstEntityBase feature)
        {
            var ln = SymplifyNodeHelper.Simplify(node.children[0], feature);
            if (ln != null)
                return ln;
            return SymplifyNodeHelper.Simplify(node.children[1], feature);
        }
    }

    #region 数字相关
    public abstract class NumberFunctor : AstFunctor
    {
        public override ASTNode Simplify(AstFuncNode node, AstEntityBase feature)
        {
            var  o= eval(node, feature);
            var d= SafeConvertAux.ToDouble(o);
            return new AstDoubleNode(d);
        }
    }
    public class RoundFunc : NumberFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            if (node.children.Count != 2)
            {
                throw new Exception("round函数接受2个参数！");
            }
            var d0 =SafeConvertAux.ToDouble(AstEvaluate.Eval(node.children[0], feature));
            var d1 = SafeConvertAux.ToInt32(AstEvaluate.Eval(node.children[1], feature));
            return Math.Round(d0, Math.Max(0, d1));
        }
    }

    public class MulFunc : NumberFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            double d = 1;
            foreach (var n in node.children)
            {
                d *=SafeConvertAux.ToDouble(AstEvaluate.Eval(n, feature));
            }
            return d;
        }
    }

    public class DivFunc : NumberFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            if (node.children.Count > 0)
            {
                double d =SafeConvertAux.ToDouble(AstEvaluate.Eval(node.children[0],feature));
                for (var i = 1; i < node.children.Count;++i )
                {
                    double d1=SafeConvertAux.ToDouble(AstEvaluate.Eval(node.children[i], feature));
                    if (d1 == 0)
                    {
                        return 0;
                    }
                    d /= d1;
                }
                return d;
            }
            return 0;
        }
    }

    public class AddFunc : NumberFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            double d = 0;
            foreach (var n in node.children)
            {
                d +=SafeConvertAux.ToDouble(AstEvaluate.Eval(n, feature));
            }
            return d;
        }
    }

    public class SubFunc : NumberFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            if (node.children.Count > 0)
            {
                double d =SafeConvertAux.ToDouble(AstEvaluate.Eval(node.children[0], feature));
                for (var i = 1; i < node.children.Count; ++i)
                {
                    double d1 =SafeConvertAux.ToDouble(AstEvaluate.Eval(node.children[i], feature));
                    d -= d1;
                }
                return d;
            }
            return 0;
        }
    }
    /// <summary>
    /// 求几何对象的面积
    /// </summary>
    public class ShapeAreaFunc : NumberFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            var geom = feature.GetGeometry();
            if (geom != null)
            {
                return geom.Area;
            }
            return 0;
        }
    }
    #endregion

    #region 字符串相关
    public abstract class StrFunctor : AstFunctor
    {
        public override ASTNode Simplify(AstFuncNode node, AstEntityBase feature)
        {
            var str = eval(node, feature);
            return new AstStringNode(str == null ? "" : str.ToString());
        }
    }
    public class StrMidFunc : StrFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            if (node.children.Count != 3)
            {
                throw new Exception("StrMid函数接受3个参数！");
            }
            var o= AstEvaluate.Eval(node.children[0], feature);
            if(o==null)
                return "";
            var str = o.ToString();

            var i1 =Math.Max(0,SafeConvertAux.ToInt32(AstEvaluate.Eval(node.children[1], feature)));
            var i2 =Math.Max(0,SafeConvertAux.ToInt32(AstEvaluate.Eval(node.children[2], feature)));
            int cnt = str.Length - i1 - i2;
            if (cnt > 0)
            {
                str=str.Substring(i1, cnt);
            }
            return str;
        }
        //public override ASTNode Simplify(AstFuncNode node, AstEntityBase feature)
        //{
        //    var str = eval(node, feature);
        //    return new AstStringNode(str==null?"":str.ToString());
        //}
    }
	public class StrRightFunc : StrFunctor
	{
		public override object eval(AstFuncNode node, AstEntityBase feature)
		{
			if (node.children.Count != 2)
			{
				throw new Exception("Right函数接受2个参数！");
			}
			var o = AstEvaluate.Eval(node.children[0], feature);
			if (o == null)
				return "";
			var str = o.ToString();
			int len = str.Length;
			var i1 = Math.Max(0, SafeConvertAux.ToInt32(AstEvaluate.Eval(node.children[1], feature)));
			int iStart = len - i1;
			if (iStart > 0)
			{
				str = str.Substring(iStart);
			}
			return str;
		}
	}

	/// <summary>
	/// 求字符串长度函数
	/// </summary>
	public class StrLenFunc : StrFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            if (node.children.Count != 1)
            {
                throw new Exception("StrLen函数接受1个参数！");
            }
            int iLen = 0;

            var o = AstEvaluate.Eval(node.children[0], feature);
            if (o != null)
            {
                var str = o.ToString();
                iLen=str.Length;
            }
            return iLen;
        }
    }

    /// <summary>
    /// 求字符串包含某个字符串吗
    /// </summary>
    public class StrContainsFunc : StrFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            if (node.children.Count != 2)
            {
                throw new Exception("StrContains函数接受2个参数！");
            }
            //int iLen = 0;

            var o = AstEvaluate.Eval(node.children[0], feature);
            if (o != null)
            {
                var str = o.ToString();
                //iLen = str.Length;
                var o1= AstEvaluate.Eval(node.children[1], feature);
                if (o1 != null)
                {
                    return str.Contains(o1.ToString());
                }
            }
            return false;
        }
    }
    #endregion

    #region 比较类
    /// <summary>
    /// 等于
    /// </summary>
    public class EqualFunc : AstFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            var ol = AstEvaluate.Eval(node.children[0], feature);
            var or = AstEvaluate.Eval(node.children[1], feature);
			return IsEqual(ol, or);
		}
    }
	public class IsNullFunc : AstFunctor
	{
		public override object eval(AstFuncNode node, AstEntityBase feature)
		{
			var ol = AstEvaluate.Eval(node.children[0], feature);
			return ol == null;
		}
	}
	public class NotFunc : AstFunctor
	{
		private readonly AstFunctor functor;
		public NotFunc(AstFunctor fun = null)
		{
			functor = fun;
		}
		public override object eval(AstFuncNode node, AstEntityBase feature)
		{
			var o =functor!=null?functor.eval(node,feature):AstEvaluate.Eval(node.children[0], feature);
			if (o is bool b)
				return !b;
			return false;
		}
	}
	public class LikeFunc : AstFunctor
	{
		public override object eval(AstFuncNode node, AstEntityBase feature)
		{
			var o1 = AstEvaluate.Eval(node.children[0], feature);
			var o2 = AstEvaluate.Eval(node.children[1], feature);
			if (o1 == null || o2 == null)
			{
				return false;
			}
			var s1 = o1.ToString();
			var s2 = o2.ToString();
			if (s1.IndexOf('%') > 0)
			{
				var t = s1;
				s1 = s2;
				s2 = t;
			}
			return SqlLike(s1, s2);
		}
		private static bool SqlLike(string str, string s)
		{
			if (s.StartsWith("%") && s.EndsWith("%"))
			{
				var s1 = s.Trim('%');
				return str.Contains(s1);
			}
			var sa = s.Split('%');
			if (sa == null || sa.Length == 0)
			{
				return str == s;
			}
			var lst = new List<string>();
			foreach (var s1 in sa)
			{
				if (!string.IsNullOrEmpty(s1))
				{
					lst.Add(s1);
				}
			}
			if (lst.Count == 1)
			{
				if (s.EndsWith("%"))
				{
					return str.StartsWith(lst[0]);
				}
				else
				{
					return str.EndsWith(lst[0]);
				}
			}
			else
			{
				//todo...
			}
			return false;
		}
	}

	public class InFunc : AstFunctor
	{
		public override object eval(AstFuncNode node, AstEntityBase feature)
		{
			var ol = AstEvaluate.Eval(node.children[0], feature);
			for (int i = 1; i < node.children.Count; ++i)
			{
				var o = AstEvaluate.Eval(node.children[i], feature);
				if (IsEqual(ol, o))
					return true;
			}
			return false;
		}
	}

	/// <summary>
	/// 大于
	/// </summary>
	public class GreatFunc : AstFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            var ol = AstEvaluate.Eval(node.children[0], feature);
            var or = AstEvaluate.Eval(node.children[1], feature);
            var dl = SafeConvertAux.ToDouble(ol);
            var dr = SafeConvertAux.ToDouble(or);
			return dl > dr;//?1:0;
        }
    }
    /// <summary>
    /// 大于等于
    /// </summary>
    public class GreatEqualFunc : AstFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            var ol = AstEvaluate.Eval(node.children[0], feature);
            var or = AstEvaluate.Eval(node.children[1], feature);
            var dl = SafeConvertAux.ToDouble(ol);
            var dr = SafeConvertAux.ToDouble(or);
			return dl >= dr;// ? 1 : 0;
        }
    }
    /// <summary>
    /// 小于
    /// </summary>
    public class LessFunc : AstFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            var ol = AstEvaluate.Eval(node.children[0], feature);
            var or = AstEvaluate.Eval(node.children[1], feature);
            var dl = SafeConvertAux.ToDouble(ol);
            var dr = SafeConvertAux.ToDouble(or);
			return dl < dr;// ? 1 : 0;
        }
    }

    /// <summary>
    /// 小于等于
    /// </summary>
    public class LessEqualFunc : AstFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            var ol = AstEvaluate.Eval(node.children[0], feature);
            var or = AstEvaluate.Eval(node.children[1], feature);
            var dl = SafeConvertAux.ToDouble(ol);
            var dr = SafeConvertAux.ToDouble(or);
			return dl <= dr;// ? 1 : 0;
        }
    }
    #endregion

    #region 逻辑类
    /// <summary>
    /// 且
    /// </summary>
    public class AndFunc : AstFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            var ol = AstEvaluate.Eval(node.children[0], feature);
            var or = AstEvaluate.Eval(node.children[1], feature);
            var dl = SafeConvertAux.ToDouble(ol);
            var dr = SafeConvertAux.ToDouble(or);
            return dl !=0 && dr!=0;
        }
    }
    /// <summary>
    /// 或
    /// </summary>
    public class OrFunc : AstFunctor
    {
        public override object eval(AstFuncNode node, AstEntityBase feature)
        {
            var ol = AstEvaluate.Eval(node.children[0], feature);
            var or = AstEvaluate.Eval(node.children[1], feature);
            var dl = SafeConvertAux.ToDouble(ol);
            var dr = SafeConvertAux.ToDouble(or);
            return dl != 0 || dr != 0;
        }
    }
    #endregion
}
