/// <summary>
///项目:  
///功能:简单的表达式解析器
/// 用来树高曲线公式
///描述:
///版本:
///日期:2010-12-28
///作者:颜学铭
///更新:
/// </summary>
using System;
using System.Collections.Generic;
using System.Text;
using static Agro.LibCore.AST.SimpleAstLabelExpress;
/*
 * 树高曲线公式
objectID	曲线公式	h_x_transform	d_y_transform	y_h_transform
1	1/h=a+b/sqr(d)	1/[h]	1/sqr([d])	1/[y]
2	1/sqr(h)=a+b/sqr(d)	1/sqr([h])	1/sqr([d])	(1/[y])*(1/[y])
3	h=a+bd*d	[h]	[d]*[d]	[y]
4	h/2=a+b/sqr(d/2)	[h]/2	1/sqr([d]/2)	2*[y]
5	h=a+b*log(d)	[h]	log([d])	[y]
6	h=a+b*d*d*d	[h]	[d]*[d]*[d]	[y]
7	1/(h*h)=a+b/(d*d*d)	1/([h]*[h])	1/([d]*[d]*[d])	sqr(1/[y])
8	sqr(h)=a+b/d	sqr([h])	1/[d]	[y]*[y]
9	h*h=a+b*d*d	[h]*[h]	[d]*[d]	sqr([y])
10	h=a+b*d	[h]	[d]	[y]
11	1/sqr(h)=a+b/d	1/sqr([h])	1/[d]	(1/[y])*(1/[y])
12	h*h=a+b*d*d*d	[h]*[h]	[d]*[d]*[d]	sqr([y])
13	h*h=a+b*d	[h]*[h]	[d]	sqr([y])
14	log(h)=a+b*log(d)	log([h])	log([d])	exp([y])
15	h=a+b*sqr(d)	[h]	sqr([d])	[y]
16	h=a+b*(d^-3)	[h]	[d]^-3	[y]
17	log(h)=a+b/d	log([h])	1/[d]	exp([y])
18	h=e^(1/(a+b/d))	1/log([h])	1/[d]	exp(1/[y])
19	h=1/(a+b/√d)	1/[h]	1/sqr([d])	1/[y]
20	h=(a+b*log(d))^2	sqr([h])	log([d])	[y]*[y]
21	h=e^(a+b√d)	log([h])	sqr([d])	exp([y]) */
namespace Agro.LibCore.AST
{
    /// <summary>
    /// 简单的表达式解析器
    /// 用来树高曲线公式
    /// </summary>
    public class SimpleAstLabelExpress
    {
        /// <summary>
        /// 客户端解析回调函数的参数
        /// </summary>
        public class TokenItem
        {
            /// <summary>
            /// 待解析字符串
            /// </summary>
            public string str;
            /// <summary>
            /// 待解析的起始位置
            /// </summary>
            public int startPos;
            /// <summary>
            /// 返回值
            /// </summary>
            public object retVal;
        }
        ///// <summary>
        ///// 返回true表示由客户端解析
        ///// </summary>
        ///// <param name="ti"></param>
        ///// <returns></returns>
        //public delegate bool NextTokenHandle(TokenItem ti);
        ///// <summary>
        ///// 客户端解析回调函数
        ///// </summary>
        //public event NextTokenHandle OnNextToken=null;

        /// <summary>
        /// 返回true表示由客户端解析
        /// </summary>
        public Func<TokenItem,bool>? OnNextToken;

        private readonly Scanner _scan;
        private OpStack _stack = new();
        public SimpleAstLabelExpress()
        {
            _scan = new Scanner(_stack);
        }
        /// <summary>
        /// 传入一个表达式返回一个double
        /// 如果解析失败抛出异常
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="dblVal">[h]或[d]或[y]的值</param>
        /// <returns></returns>
        public ASTNode? BuildAST(string expr)//, double dblVal)
        {
            _stack.Clear();
            _scan.Reset(expr);
            return Parse();
        }
        private ASTNode? Parse()
        {
            var token = _scan.NextToken(OnNextToken);
            if (token == null)
            {
                for (var op = _stack.GetPreOperator(); op != null; op = _stack.GetPreOperator())
                {
                    op.DoOp(_stack);
                }
                var o = _stack.Pop();
                return o as ASTNode;// Convert.ToDouble(o);
            }
            else if (token is ASTNode)
            {
                _stack.Push(token);
                return Parse();
            }
            else if (token is RightBracket rbOp)
            {
                //Operator op = token as RightBracket;
                for (var preOp = _stack.GetPreOperator(); preOp != null && preOp >= rbOp; preOp = _stack.GetPreOperator())
                {
                    preOp.DoOp(_stack);
                    if (preOp is LeftBracket)
                    {
                        return Parse();
                    }
                }
                return Parse();
            }
            //else if (token is FunctionOp)
            //{
            //    _stack.Push(token);
            //    return Parse();
            //}
            else if (token is Operator op)
            {
                //Operator op = token as Operator;
                for (var preOp = _stack.GetPreOperator(); preOp != null && preOp >= op; preOp = _stack.GetPreOperator())
                {
                    preOp.DoOp(_stack);
                }
                _stack.Push(token);
                return Parse();
            }
            return null;
        }
    }
    /// <summary>
    /// 堆栈机
    /// </summary>
    internal class OpStack
    {
        private List<object> _s = new();
        internal void Clear()
        {
            _s.Clear();
        }
        /// <summary>
        /// 取栈顶元素的值
        /// </summary>
        /// <returns></returns>
        internal object? Top()
        {
            return _s.Count > 0 ? _s[_s.Count - 1] : null;
        }
        /// <summary>
        /// 弹出栈顶的元素
        /// </summary>
        /// <returns></returns>
        internal object? Pop()
        {
            var o = Top();
            if (o != null)
            {
                _s.RemoveAt(_s.Count - 1);
            }
            return o;
        }
        /// <summary>
        /// 进栈
        /// </summary>
        /// <param name="o"></param>
        internal void Push(object o)
        {
            _s.Add(o);
        }
        /// <summary>
        /// 取先前压入堆栈的操作符
        /// </summary>
        /// <returns></returns>
        internal Operator GetPreOperator()
        {
            for (int i = _s.Count - 1; i >= 0; --i)
            {
                if ((_s[i] as Operator) != null)
                    return _s[i] as Operator;
            }
            return null;
        }

    }
    internal class Parser
    {
        //internal virtual bool Parse(Scanner s);
    }
    /// <summary>
    /// 操作符
    /// </summary>
    internal class Operator : Parser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        internal virtual void DoOp(OpStack s)
        {
        }
        /// <summary>
        /// 判读操作符的优先级
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator >=(Operator lhs, Operator rhs)
        {
            return lhs.GreaterEqual(rhs);
        }
        public static bool operator <=(Operator lhs, Operator rhs)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 优先级是否>=右边的操作符的优先级
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected virtual bool GreaterEqual(Operator rhs)
        {
            return false;
        }
    }
    /// <summary>
    /// 左括号
    /// </summary>
    internal class LeftBracket : Operator
    {
        /// <summary>
        /// 从消除堆栈顶端的左括号
        /// </summary>
        /// <param name="s"></param>
        internal override void DoOp(OpStack s)
        {
            var p = s.Pop();
            if (!(p is double))
            {
                throw new Exception("");
            }
            s.Pop();
            s.Push(p);
        }

        /// <summary>
        /// 左括号的优先级除右括号外始终小于它右边的操作符的优先级
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected override bool GreaterEqual(Operator rhs)
        {
            if (rhs is RightBracket)
                return true;
            return false;
        }
    }
    /// <summary>
    /// 右括号
    /// </summary>
    internal class RightBracket : Operator
    {
        //不需要做任何处理
        internal override void DoOp(OpStack s)
        {
        }
        /// <summary>
        /// 有括号的优先级始终>它右边的操作符的优先级
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected override bool GreaterEqual(Operator rhs)
        {
            return true;
        }
    }
    internal class FunctionOp : Operator, IComparable
    {
        protected string _funcName;//函数名
        internal FunctionOp(string funcName)
        {
            _funcName = funcName;
        }
        internal string FuncName
        {
            get { return _funcName; }
        }

        #region IComparable 成员

        public int CompareTo(object obj)
        {
            FunctionOp fo = obj as FunctionOp;
            return fo.FuncName.Length - _funcName.Length;
        }

        #endregion
    }

    /// <summary>
    /// 一元函数
    /// </summary>
    internal class UnaryFunctionOp : FunctionOp
    {
        internal UnaryFunctionOp(string funcName)
            : base(funcName)
        {
        }
        /// <summary>
        /// 函数右边的操作符始终是左括号
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected override bool GreaterEqual(Operator rhs)
        {
            if(rhs is LeftBracket)
                return false;
            return true;
        }
    }
    /// <summary>
    /// 乘操作符（*）
    /// </summary>
    internal class MulOp : Operator
    {
        internal override void DoOp(OpStack s)
        {
            double r = Convert.ToDouble(s.Pop());
            s.Pop();//弹出操作数*
            double l = Convert.ToDouble(s.Pop());
            s.Push(l * r);
        }
        /// <summary>
        /// *的优先级只比右边的左括号或函数的优先级低
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected override bool GreaterEqual(Operator rhs)
        {
            if (rhs is LeftBracket || rhs is FunctionOp || rhs is PowFuncOp)
                return false;
            return true;
        }
    }
    /// <summary>
    /// 除操作符(/)
    /// </summary>
    internal class DivOp : Operator
    {
        private readonly char sOpr;
        internal DivOp(char sOpr) {
            this.sOpr = sOpr;
        }
        internal override void DoOp(OpStack s)
        {
            /*
            double r = Convert.ToDouble(s.Pop());
            s.Pop();//弹出操作数*
            double l = Convert.ToDouble(s.Pop());
            s.Push(l / r);
            */

            var right = s.Pop() as ASTNode;
            //divNode.children.Add(s.Pop() as ASTNode);
            s.Pop();//弹出操作数*
            //divNode.children.Add(s.Pop() as ASTNode);
            var left = s.Pop() as ASTNode;

            var divNode = new AstOprNode()
            {
                oper =sOpr=='/'? OperatorType.OPR_DIVIDE:OperatorType.OPR_HLINE,
                operName = sOpr.ToString()
            };
            divNode.children.Add(left);
            divNode.children.Add(right);
            s.Push(divNode);
            //divNode.operName = opr.ToString();
            //if (opr == "/")
            //{
            //    divNode.opr = OperatorType.OPR_DIVIDE;
            //}else if (opr == "\\")
            //{
            //    divNode.opr = OperatorType.OPR_HLINE;,
            //}
        }
        /// <summary>
        /// /的优先级只比右边的左括号或函数的优先级低
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected override bool GreaterEqual(Operator rhs)
        {
            if (rhs is LeftBracket || rhs is FunctionOp || rhs is PowFuncOp)
                return false;
            return true;
        }
    }
    /// <summary>
    /// 加操作符(+)
    /// </summary>
    internal class PlusOp : Operator
    {
        internal override void DoOp(OpStack s)
        {
            double r = Convert.ToDouble(s.Pop());
            s.Pop();//弹出操作数*
            double l = Convert.ToDouble(s.Pop());
            s.Push(l + r);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        protected override bool GreaterEqual(Operator rhs)
        {
            if (rhs is LeftBracket || rhs is FunctionOp || rhs is MulOp || rhs is DivOp || rhs is PowFuncOp)
                return false;
            return true;
        }
    }
    /// <summary>
    /// 减操作符(-)
    /// </summary>
    internal class MinusOp : Operator
    {
        internal override void DoOp(OpStack s)
        {
            double r = Convert.ToDouble(s.Pop());
            s.Pop();//弹出操作数
            double l = Convert.ToDouble(s.Pop());
            s.Push(l - r);
        }
        protected override bool GreaterEqual(Operator rhs)
        {
            if (rhs is LeftBracket || rhs is FunctionOp || rhs is MulOp || rhs is DivOp || rhs is PowFuncOp)
                return false;
            return true;
        }
    }
    /*internal class DoubleParser : Parser
    {
        internal double _d = 0;
        internal DoubleParser(double d)
        {
            _d = d;
        }
        internal double Value
        {
            get { return _d; }
        }
    }*/

    internal class PowFuncOp : Operator
    {
        internal override void DoOp(OpStack s)
        {
            double y = Convert.ToDouble(s.Pop());
            s.Pop();
            double x = Convert.ToDouble(s.Pop());
            s.Push(System.Math.Pow(x, y));
        }
        protected override bool GreaterEqual(Operator rhs)
        {
            if (rhs is LeftBracket || rhs is FunctionOp)
                return false;
            return true;
        }
    }
    #region 函数
    /// <summary>
    /// log函数
    /// </summary>
    internal class LogFuncOp : UnaryFunctionOp
    {
        internal LogFuncOp()
            : base("log")
        {
        }
        internal override void DoOp(OpStack s)
        {
            double d = Convert.ToDouble(s.Pop());
            s.Pop();//弹出操作符
            s.Push(System.Math.Log(d));
        }
    }
    internal class SqrFuncOp : UnaryFunctionOp
    {
        internal SqrFuncOp()
            : base("sqr")
        {
        }
        internal override void DoOp(OpStack s)
        {
            double d = Convert.ToDouble(s.Pop());
            s.Pop();//弹出操作符
            s.Push(System.Math.Sqrt(d));
        }
    }
    internal class ExpFuncOp : UnaryFunctionOp
    {
        internal ExpFuncOp()
            : base("exp")
        {
        }
        internal override void DoOp(OpStack s)
        {
            double d = Convert.ToDouble(s.Pop());
            s.Pop();//弹出操作符
            s.Push(System.Math.Exp(d));
        }
    }

    #endregion 函数
    /*internal class DoubleOp:Parser
    {
        private double _d;
        internal DoubleOp(double d)
        {
            _d = d;
        }
        internal double Value
        {
            get { return _d; }
        }
    }*/
    /// <summary>
    /// 扫描器
    /// </summary>
    internal class Scanner
    {
        private readonly OpStack _stack;// = null;
        private static List<FunctionOp> _lstFunc = new ();
        private int _startPos = 0;
        private string _s="";
        //private double _dblVal = 0;
        private readonly TokenItem _ti = new();
        internal Scanner(OpStack stack)
        {
            _stack = stack;
            if (_lstFunc.Count == 0)
            {

                _lstFunc.Add(new LogFuncOp());
                _lstFunc.Add(new SqrFuncOp());
                _lstFunc.Add(new ExpFuncOp());
                _lstFunc.Sort();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="dblVal"></param>
        internal void Reset(string s)//, double dblVal)
        {
            //_dblVal = dblVal;
            _startPos = 0;
            _s = s;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        internal object? NextToken(Func<TokenItem, bool>? callback =null)
        {
            SkipSpace();
            if (_startPos == _s.Length)
                return null;
            if (callback != null)
            {
                _ti.str = _s;
                _ti.startPos = _startPos;
                if (callback(_ti))
                {
                    _startPos = _ti.startPos;
                    return _ti.retVal;
                }
            }
            int n = 0, len = 0;
            object? o = null;
            var chr = _s[_startPos];
            switch (chr)
            {
                case '(': o = new LeftBracket(); ++_startPos; break;
                case ')': o = new RightBracket(); ++_startPos; break;
                case '+': o = new PlusOp(); ++_startPos; break;
                case '-':
                    {
                        ++_startPos;
                        if (_stack.Top() is double)
                        {
                            o = new MinusOp();
                        }
                        else
                        {
                            if (StartWithUint(out n, out len))
                            {
                                o = (double)(-n);
                                _startPos += len;
                            }
                            else
                            {
                                throw new Exception("语法错误，位置：" + _startPos);
                            }
                        }
                    } break;
                case '*': o = new MulOp(); ++_startPos; break;
                case '\\':
                case '/': o = new DivOp(chr); ++_startPos; break;
                case '^': o = new PowFuncOp(); ++_startPos; break;
                //case 'd': ++_startPos; return _dblVal;z
                case '[':
                    {
                        var fok = false;
                        string fieldName = "";
                        for(var i = _startPos + 1; i < _s.Length; i++)
                        {
                            var ch=_s[i];
                            if (_s[i] == ']')
                            {
                                _startPos = i + 1;
                                fok = true;
                                break;
                            }
                            fieldName += ch;
                        }
                        if (!fok) throw new Exception($"error at {_startPos} find '[' but not find ']'");
                        return new AstFieldNode(fieldName);
                    }
            }
            if (o != null)
                return o;
            if (StartWithUint(out n, out len))
            {
                _startPos += len;
                return (double)n;
            }
            int nRemainChar = _s.Length - _startPos;
            //if (nRemainChar >= 3)
            //{
            //    if (_s[_startPos] == '[' && _s[_startPos + 2] == ']')
            //    {
            //        _startPos += 3;
            //        return _dblVal;
            //    }
            //}
            foreach (FunctionOp fo in _lstFunc)
            {
                if (fo.FuncName.Length <= nRemainChar)
                {
                    if (StartsWidth(fo.FuncName))
                    {
                        _startPos += fo.FuncName.Length;
                        return fo;
                    }
                }
            }
            throw new Exception("无效的输入！");
        }
        /// <summary>
        /// 判断_s从_startPos开始是否是s开头
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool StartsWidth(string s)
        {
            System.Diagnostics.Debug.Assert(_s.Length - _startPos >= s.Length);
            for (int i = 0; i < s.Length; ++i)
            {
                if (_s[_startPos + i] != s[i])
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 跳过空白
        /// </summary>
        private void SkipSpace()
        {
            while (_startPos < _s.Length && IsSpace(_s[_startPos]))
            {
                ++_startPos;
            }
        }
        private bool IsSpace(char ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n';
        }
        private bool IsDigit(char ch)
        {
            return ch >= '0' && ch <= '9';
        }
        /// <summary>
        /// 假定表达式中的整数只有一位
        /// </summary>
        /// <param name="n"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private bool StartWithUint(out int n, out int len)
        {
            n = 0;
            len = 0;
            if (_startPos < _s.Length && IsDigit(_s[_startPos]))
            {
                n = _s[_startPos];
                len = 1;
                return true;
            }

            return false;
        }
    }
}
