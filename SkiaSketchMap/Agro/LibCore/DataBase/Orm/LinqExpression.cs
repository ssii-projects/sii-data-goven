/*
 * (C) 2017 xx公司版权所有，保留所有权利
 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   ImpOrmObjectBase
 * 创 建 人：   颜学铭
 * 创建时间：   2017/5/18 15:36:25
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Agro.LibCore.Database
{
    public class WhereExpression<T> where T :class, new()
    {
		class BoolExpressionValue
		{
			private readonly string Expression;
			public BoolExpressionValue(string v)
			{
				Expression = v;
			}
			public override string ToString()
			{
				return Expression;
			}
		}
		class FieldValue
		{
			internal readonly EntityProperty Value;
			public FieldValue(EntityProperty v)
			{
				Value = v;
			}
			public override string ToString()
			{
				return Value.FieldName;
			}
		}

		/// <summary>
		/// 移除表达式两端多余的括号
		/// 如：
		/// 示例1：输入"(name is null or id>0)" 输出："name is null or id>0"
		/// 示例2：输入"(a=b or c=d) and (d>10 or d<5)" 输出保持不变；
		/// </summary>
		class BracketUtil {
			public static string Inner(string s)
			{
				if (s != null)
				{
					if (CanRemove(s))
					{
						s = s.Substring(1, s.Length - 2);
					}
				}
				return s;
			}
			private static bool CanRemove(string s) {
				var left = '(';
				var right = ')';
				if (s.IndexOf(left) != 0 || s.LastIndexOf(right) != s.Length - 1)
					return false;
				bool fCanRemove =s.IndexOf(left, 1) < 0;
				if (fCanRemove)
					return true;

				var str = s.Replace("''", "_");

				#region 将字符串替换为k以避免字符串中可能出现的括号干扰判断
				var c = '\'';
				int i = str.IndexOf(c);
				while (i>= 0)
				{
					int j = str.IndexOf(c, i + 1);
					if (j < 0)
						break;
					str = str.Substring(0, i) + "k" + str.Substring(j + 1);
					i = str.IndexOf(c,i);
				}
				#endregion

				str = str.Substring(1, str.Length - 2);

				#region  移除内部的括号对
				i = str.LastIndexOf(left);
				while (i >= 0)
				{
					int j = str.IndexOf(right, i + 1);
					if (j < i) break;
					str = str.Substring(0, i) + "[" + str.Substring(i + 1, j - i - 1) + "]" + str.Substring(j + 1);
					i = str.LastIndexOf(left);
				}
				#endregion

				return str.IndexOf(left) < 0;
			}

		}
        public static string Where(Expression<Func<T, bool>> predicate)
        {
			if (predicate == null) return null;
			var o = ProcessExpression(predicate);
            return BracketUtil.Inner(o?.ToString());
        }

		private static object ProcessExpression(Expression expression)
        {
			if (expression is UnaryExpression ue)
			{
				if (ue.NodeType == ExpressionType.Not)
				{
					var op = ProcessExpression(ue.Operand);
					if (op is FieldValue en)
					{
						return new BoolExpressionValue($"{en.ToString()}<>1");
					}
					return new BoolExpressionValue($"not {op.ToString()}");//return $"{op}<>1";
				}
				else if (ue.NodeType == ExpressionType.Convert)
				{
					var op = ProcessExpression(ue.Operand);
					return op;
				}
				throw new NotImplementedException();
			}
			else if (expression is LambdaExpression)
			{
				return ProcessExpression(((LambdaExpression)expression).Body);
			}
			else if (expression is BinaryExpression)
			{
				var be = expression as BinaryExpression;
				var oLeft = ProcessExpression(be.Left);
				var oRight = ProcessExpression(be.Right);
				var sLeft =oLeft .ToString();
				var sRight = oRight.ToString();

				if (expression.NodeType == ExpressionType.OrElse || expression.NodeType == ExpressionType.AndAlso)
				{
					if (oLeft is FieldValue)
					{
						sLeft += "=1";
					}
					if (oRight is FieldValue)
					{
						sRight += "=1";
					}
				}

				var ope = GetOperStr(expression.NodeType);

				if (sRight == "NULL")
				{
					if (ope == "=")
					{
						ope = " is ";
					}
					else
					{
						ope = " is not ";
					}
					return new BoolExpressionValue(sLeft + ope + sRight);
				}
				else if (sLeft == "NULL")
				{
					if (ope == "=")
					{
						ope = " is ";
					}
					else
					{
						ope = " is not ";
					}
					return new BoolExpressionValue(sRight + ope + sLeft);
				}
				var fOr = expression.NodeType == ExpressionType.OrElse
					|| expression.NodeType == ExpressionType.Add
					|| expression.NodeType == ExpressionType.Subtract
					|| expression.NodeType == ExpressionType.And
					|| expression.NodeType == ExpressionType.Or;

				if (fOr)
				{
					sLeft = BracketUtil.Inner(sLeft);
					sRight = BracketUtil.Inner(sRight);
				}

				var s = sLeft + ope + sRight;

				if (fOr)
				{
					s = "(" + s + ")";
				}
				return new BoolExpressionValue(s);
			}
			else if (expression is MethodCallExpression call)
			{
				switch (call.Method.Name)
				{
					case "Contains":
						{
							string sin;
							var first = call.Arguments[0];
							if (call.Object == null)
							{
								if (ParseEnumerable(first, out sin))
								{
									var key = ProcessExpression(call.Arguments[1]);
									return new BoolExpressionValue(key + " in" + sin);
								}
							}
							var s = ProcessExpression(first);
							if (ParseEnumerable(call.Object, out sin))
							{
								return new BoolExpressionValue(s.ToString() + " in" + sin);
							}
							var sLeft = ProcessExpression(call.Object);
							return new BoolExpressionValue(sLeft + $" like '%{ s.ToString().Trim('\'').Replace("'", "''")}%'");
						}
					case "StartsWith":
						{
							var sLeft = ProcessExpression(call.Object).ToString();
							var first = call.Arguments[0];
							var s = ProcessExpression(first).ToString();
							return new BoolExpressionValue(sLeft + " like '" + s.Trim('\'').Replace("'", "''") + "%'");
						}
					case "EndsWith":
						{
							var sLeft = ProcessExpression(call.Object).ToString();
							Expression first = call.Arguments[0];
							var s = ProcessExpression(first).ToString();
							return new BoolExpressionValue(sLeft + " like '%" + s.Trim('\'').Replace("'", "''") + "'");
						}
					default:
						{
							System.Diagnostics.Debug.Assert(false, "未处理的函数：" + call.Method.Name);
							//todo...
						}
						break;
				}
			}
			else if (expression is ConstantExpression ce)
			{
				return ConvertValue(ce.Value);
			}
			else if (expression is MemberExpression tmp)
			{
				if (tmp.Expression.GetType().Name == "TypedParameterExpression")
				{
					return new FieldValue(Entity<T>.GetAttribute(tmp.Member.Name));
				}
				var cast = Expression.Convert(tmp, typeof(object));
				object obj = Expression.Lambda<Func<object>>(cast).Compile().Invoke();
				return ConvertValue(obj);
			}
			System.Diagnostics.Debug.Assert(false);
            return null;
        }
		private static bool ParseEnumerable(Expression expression, out string sin)
		{
			sin = "(";
			if (expression is MemberExpression tmp)
			{
				if (tmp.Expression.GetType().Name != "TypedParameterExpression")
				{
					var cast = Expression.Convert(tmp, typeof(object));
					object obj = Expression.Lambda<Func<object>>(cast).Compile().Invoke();
					if (obj is IEnumerable lst)
					{
						foreach (var o in lst)
						{
							if (sin.Length > 1) sin += ",";
							sin += ConvertValue(o);
						}
						sin += ")";
						return true;
					}
				}
			}
			else if (expression is ListInitExpression lstIE)
			{
				foreach (var it in lstIE.Initializers)
				{
					if (it.Arguments.Count > 0 && it.Arguments[0] is ConstantExpression ce)
					{
						if (sin.Length > 1) sin += ",";
						sin += ConvertValue(ce.Value);
					}
				}
				sin += ")";
				return true;
			}
			else if (expression is NewArrayExpression nae)
			{
				foreach (var it in nae.Expressions)
				{
					if (it is ConstantExpression ce)
					{
						if (sin.Length > 1) sin += ",";
						sin += ConvertValue(ce.Value);
					}
				}
				sin += ")";
				return true;
			}
			return false;
		}

		private static string GetOperStr(ExpressionType e_type)
        {
            switch (e_type)
            {
                case ExpressionType.OrElse: return " OR ";
                case ExpressionType.Or: return "|";
                case ExpressionType.AndAlso: return " AND ";
                case ExpressionType.And: return "&";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.Add: return "+";
                case ExpressionType.Subtract: return "-";
                case ExpressionType.Multiply: return "*";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.Equal: return "=";
            }
            return "";
        }

        private static object ConvertValue(object obj)
        {
			object s = obj;
			if (obj == null)
			{
				s = "NULL";
			}else if (obj is string)
			{
				s = "'" + obj + "'";
			}
			else if (obj is bool b)
			{
				s = new BoolExpressionValue(b ? "1=1" : "1<>1");//.ToString();
			}
			else if (obj is DateTime time)
			{
				s = $"'{time.ToString("yyyy-MM-dd HH:mm:ss")}'";
			}
			else if (obj is Enum)
			{
				s =((int)obj).ToString();
			}
			return s;
        }
    }

	public delegate List<string> ListCreator(params object[] args);
	public sealed class FieldsExpression<T> where T:class, new()
    {
		/// <summary>
		/// 返回字段名集合
		/// </summary>
		/// <param name="expression"></param>
		/// <returns>can be null</returns>
		public static IEnumerable<string> Fields(Expression<Action<ListCreator, T>> expression)
		{
			return Properties(expression).Select(it => it.FieldName);
		}
		public static List<EntityProperty> Properties(Expression<Action<ListCreator, T>> expression)
		{
			List<EntityProperty> fields = null;
			if (expression is LambdaExpression lambdaExpression)
			{
				if (lambdaExpression.Body is InvocationExpression call)
				{
					if (call.Arguments[0] is NewArrayExpression nae)
					{
						fields = new List<EntityProperty>();
						foreach (var it in nae.Expressions)
						{
							fields.Add(ParseField(it));
						}
					}
				}
			}
			return fields;
		}
		public static EntityProperty Field(Expression<Func<T,object>> expression)
		{
			if (expression is LambdaExpression lambdaExpression)
			{
				return ParseField(lambdaExpression.Body);
			}
			return null;
		}
		private static EntityProperty ParseField(Expression expression)
		{
			if (expression is MemberExpression tmp)
			{
				var valueClassName = tmp.Expression.GetType().Name;
				if (valueClassName == "TypedParameterExpression")
				{
					return Entity<T>.GetAttribute(tmp.Member.Name);// Entity<T>.GetFieldName(tmp.Member.Name);
				}
			}
			else if (expression is UnaryExpression ue)
			{
				if (ue.NodeType == ExpressionType.Convert)
				{
					return ParseField(ue.Operand);
				}
			}
			return null;
		}
	}
}
