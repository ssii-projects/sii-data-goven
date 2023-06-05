
namespace Agro.LibCore.AST
{
    public enum NodeType 
    {
        typeUnknown,
        typeOpr=1, typeTag, typeInt, typeStr, typeField, typeReal,typeFunc,typeStatement,typeKeyword
    }

    public enum OperatorType
    {
        OPR_NULL = 258,
        OPR_PLUS = 259,
        OPR_MINUS,
        OPR_MULT,
        OPR_DIVIDE,
        OPR_CONCAT,
        OPR_EQUAL,
        OPR_AND,
        OPR_OR,
        OPR_GREAT,
        OPR_LESS,
        OPR_GREATTHAN,
        OPR_LESSTHAN,
        OPR_NOTEQUAL,
        OPR_HLINE,//水平分隔线
        OPR_QUESTION,//?
        OPR_STATEMENT,//;
        OPR_LIKE,
		OPR_ISNULL,
		OPR_ISNOTNULL,
		OPR_NOT,
		OPR_IN,
		OPR_NOTIN,
		OPR_NOTLIKE,
	};
    public class ASTNode
    {
        //public NodeType type;
        //public object value;
        public readonly MyRect rc = new MyRect();

        public virtual ASTNode Clone()
        {
            throw new NotImplementedException();
        }
        public virtual object GetValue()
        {
            return null;
        }
    }

    public class AstIntNode : ASTNode
    {
        public int Value;
        public AstIntNode(int d)
        {
            Value = d;
        }
        public override ASTNode Clone()
        {
            var n= new AstIntNode(Value);
            n.rc.SetRect(rc);
            return n;
        }
        public override object GetValue()
        {
            return Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    public class AstDoubleNode : ASTNode
    {
        public double Value;
        public AstDoubleNode(double d)
        {
            Value = d;
        }
        public override ASTNode Clone()
        {
            var n = new AstDoubleNode(Value);
            n.rc.SetRect(rc);
            return n;
        }
        public override object GetValue()
        {
            return Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    public class AstStringNode : ASTNode
    {
        public string Value;
        public AstStringNode(string str)
        {
            Value = str;
        }
        public override ASTNode Clone()
        {
            var n = new AstStringNode(Value);
            n.rc.SetRect(rc);
            return n;
        }
        public override object GetValue()
        {
            return Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }
    public class AstFieldNode : ASTNode
    {
        public string fieldName { get; private set; }
        public object fieldValue;
        public int fieldIndex=-1;
        public AstFieldNode(string fieldName_,object fieldValue_=null)
        {
            fieldName = fieldName_;
            fieldValue = fieldValue_;
        }
        public override ASTNode Clone()
        {
            var n= new AstFieldNode(fieldName,fieldValue);
            n.rc.SetRect(rc);
            return n;
        }
    }
    public class AstParentNode : ASTNode
    {
        public readonly List<ASTNode?> children = new();
    }
    /// <summary>
    /// 操作节点
    /// </summary>
    public class AstOprNode : AstParentNode
    {
        public OperatorType oper;
        public string operName;
        public readonly MyRect rcOperator=new MyRect();
        public override ASTNode Clone()
        {
			var n = new AstOprNode
			{
				oper = oper,
				operName = operName
			};
			n.rcOperator.SetRect(rcOperator);
            for (var i = 0; i < children.Count; ++i)
            {
                n.children.Add(children[i].Clone());
            }
            return n;
        }
    }
    public class AstFuncNode : AstParentNode
    {
        public string funcName;
        public readonly MyRect rcLeft = new MyRect();//左区域 funcName(
        public readonly MyRect rcRight = new MyRect();//右区域 )
        public readonly MyRect rcComma = new MyRect();//逗号区域 
		public AstFuncNode(string funName)
		{
			funcName = funName;
		}
		public override ASTNode Clone()
        {
			var n = new AstFuncNode(funcName);
			n.rcLeft.SetRect(rcLeft);
            n.rcRight.SetRect(rcRight);
            n.rcComma.SetRect(rcComma);
            for (var i = 0; i < children.Count; ++i)
            {
                n.children.Add(children[i].Clone());
            }
            return n;
        }
    }
    public class AstStatementNode : AstParentNode
    {
        public override ASTNode Clone()
        {
            var n = new AstStatementNode();
            for (var i = 0; i < children.Count; ++i)
            {
                n.children.Add(children[i].Clone());
            }
            return n;
        }
    }
    public static class ASTNodeUtil
    {
        public static string[] GetFields(ASTNode t)
        {
            List<string> fields = new List<string>();
            getFields(t, fields);
            return fields.ToArray();
        }
        private static void getFields(ASTNode t, List<string> fields)
        {
            if (t is AstFieldNode)
            {
                var n = t as AstFieldNode;
                var fieldName = n.fieldName;
                if (!fields.Contains(fieldName))
                {
                    fields.Add(fieldName);
                }
            }
            else if (t is AstParentNode)
            {
                var n = t as AstParentNode;
                foreach (var cn in n.children)
                {
                    getFields(cn, fields);
                }
            }
        }
    }
}
