using GeoAPI.Geometries;

namespace Agro.LibCore.AST
{
    public abstract class AstEntityBase
    {
        public virtual object GetFieldValue(int iField)
        {
            return null;
        }
        public virtual object GetPropertyValue(string propertyName)
        {
            return null;
        }
        public abstract int FindField(string fieldName);
        public virtual IGeometry GetGeometry()
        {
            return null;
        }
    }
    public static class AstEvaluate
    {
        public static object Eval(ASTNode node, AstEntityBase feature)
        {
            if (node is AstFieldNode fieldNode)
            {
                var fieldIndex = fieldNode.fieldIndex;
                if (fieldIndex < 0)
                {
                    fieldIndex = feature.FindField(fieldNode.fieldName);
                    fieldNode.fieldIndex = fieldIndex;
                }
                return feature.GetFieldValue(fieldIndex);
            }
            else if (node is AstOprNode oprNode)
            {
                var fn = AstFuncHelper.Convert(oprNode);
                System.Diagnostics.Debug.Assert(fn != null);
                return AstFunctorFactory.GetFunctor(fn.funcName).eval(fn, feature);
            }
            else if (node is AstFuncNode funNode)
            {
                return AstFunctorFactory.GetFunctor(funNode.funcName).eval(funNode, feature);
            }
            return node.GetValue();
        }
    }
}
