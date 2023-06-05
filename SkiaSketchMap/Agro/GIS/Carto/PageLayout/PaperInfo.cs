using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.GIS
{
    public class PaperInfo
    {
        /// <summary>
        /// 纸张大小
        /// </summary>
        public PaperSize Size;

        /// <summary>
        /// 纸张方向
        /// </summary>
        public PaperOrientation Orientation = PaperOrientation.Landscape;

        /// <summary>
        /// 输出dpi
        /// </summary>
        public double Dpi = 300;//96;

        public PaperInfo(PaperSize size = null)
        {
            Size = size;
        }

        public void Override(PaperInfo rhs)
        {
            this.Size = rhs.Size;
            this.Orientation = rhs.Orientation;
            this.Dpi = rhs.Dpi;
        }

        /// <summary>
        /// 获取水平方向点数（对应dpi的dot）
        /// </summary>
        /// <returns></returns>
        public double GetDotWidth(double dpi=96)
        {
            dpi = this.Dpi;
            if (Orientation == PaperOrientation.Portrait)
            {
                return Size.GetPixelWidth(dpi);
            }
            else
            {
                return Size.GetPixelHeight(dpi);
            }
        }

        /// <summary>
        /// 获取垂直方向点数（对应dpi的dot）
        /// </summary>
        /// <returns></returns>
        public double GetDotHeight(double dpi = 96)
        {
            dpi = this.Dpi;// 96;
            if (Orientation == PaperOrientation.Portrait)
            {
                return Size.GetPixelHeight(dpi);
            }
            else
            {
                return Size.GetPixelWidth(dpi);
            }
        }

        public double Dot2Units(double nDots,OkUnits u = OkUnits.okCentimeters)
        {
            if (u == OkUnits.okCentimeters)
            {

            }
            return nDots;
        }
    }
    public enum OkUnits
    {
        okUnknownUnits,
        okInches,
        okPoints,
        okFeet,
        okYards,
        okMiles,
        okNauticalMiles,
        okMillimeters,
        okCentimeters,
        okMeters,
        okKilometers,
        okDecimalDegrees,
        okDecimeters
    }
    public enum PaperOrientation
    {
        /// <summary>
        /// 纵向
        /// </summary>
        Portrait=1,
        /// <summary>
        /// 横向
        /// </summary>
        Landscape=2
    }
    public enum eImageFormat { JPG, BMP, PNG, GIF, TIF }
}
