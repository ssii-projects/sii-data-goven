using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.GIS
{
    public class UnitsUtil
    {
        public static double ConvertUnits(OkUnits from, OkUnits to, double value)
        {
            if (from == to)
                return value;
            value=Convert2Meter(from, value);
            value = ConvertFromMeter(to, value);
            return value;
        }

        public static double Convert2Meter(OkUnits from, double value)
        {
            switch (from)
            {
                case OkUnits.okUnknownUnits:
                case OkUnits.okMeters:
                    break;
                case OkUnits.okInches:
                    value = value * 25.4 / 1000;
                    break;
                case OkUnits.okCentimeters:
                    value = value / 100;
                    break;
                case OkUnits.okPoints:
                    value = value / 72 * 25.4 / 1000;
                    break;
                case OkUnits.okMillimeters:
                    value = value / 1000;
                    break;
                case OkUnits.okDecimalDegrees:
                    value = value * 111195;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
            return value;
        }

        public static double ConvertFromMeter(OkUnits to, double value)
        {
            switch (to)
            {
                case OkUnits.okUnknownUnits:
                case OkUnits.okMeters:
                    break;
                case OkUnits.okInches:
                    value = value / 25.4 * 1000;
                    break;
                case OkUnits.okCentimeters:
                    value = value * 100;
                    break;
                case OkUnits.okKilometers:
                    value = value / 1000;
                    break;
                case OkUnits.okFeet:
                    value = value / 0.3048;
                    break;
                case OkUnits.okMiles:
                    value = value / 1609;
                    break;
                case OkUnits.okYards:
                    value = value / 0.9144;
                    break;
                case OkUnits.okPoints:
                    value = value / 25.4 * 1000 * 72;
                    break;
                case OkUnits.okMillimeters:
                    value = value * 1000;
                    break;
                case OkUnits.okDecimalDegrees:
                    value = value / 111195;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
            return value;
        }

        public static string UnitString(OkUnits u)
        {
            switch (u)
            {
                case OkUnits.okCentimeters:return "厘米";
                case OkUnits.okInches:return "英寸";
                case OkUnits.okPoints:return "磅";
                case OkUnits.okMillimeters:return "毫米";
                default:
                    return u.ToString();
            }
        }
    }
}
