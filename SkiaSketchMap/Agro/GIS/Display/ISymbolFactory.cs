namespace Agro.GIS
{
    public interface ISymbolFactory
    {
        ISymbol? CreateSymbol(string typeName);
        ILineSymbol CreateSimpleLineSymbol();
        IFillSymbol CreateSimpleFillSymbol();
        ITextSymbol CreateTextSymbol();
        ICharacterMarkerSymbol CreateCharacterMarkerSymbol();
        IMarkerSymbol CreateSimpleMarkerSymbol();
    }
}
