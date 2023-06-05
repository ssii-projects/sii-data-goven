using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.GIS
{
    public class PaperSize
    {
        #region 预定义纸张信息
        public static readonly List<PaperSize> arrForms = new()
        {
    new PaperSize("字母",OkUnits.okInches,8.5,11,OkPageFormID.okPageFormLetter),
    new PaperSize("Legal",OkUnits.okInches,8.5,14,OkPageFormID.okPageFormLegal),
    new PaperSize("Tabloid",OkUnits.okInches,11,17,OkPageFormID.okPageFormTabloid),
    new PaperSize("C",OkUnits.okInches,17,22,OkPageFormID.okPageFormC),
    new PaperSize("D",OkUnits.okInches,22,34,OkPageFormID.okPageFormD),
    new PaperSize("E",OkUnits.okInches,34,44,OkPageFormID.okPageFormE),
    new PaperSize("A5",OkUnits.okCentimeters,14.8,21,OkPageFormID.okPageFormA5),
    new PaperSize("A4",OkUnits.okCentimeters,21,29.7,OkPageFormID.okPageFormA4),
    new PaperSize("A3",OkUnits.okCentimeters,29.7,42,OkPageFormID.okPageFormA3),
    new PaperSize("A2",OkUnits.okCentimeters,42,59.4,OkPageFormID.okPageFormA2),
    new PaperSize("A1",OkUnits.okCentimeters,59.4,84.1,OkPageFormID.okPageFormA1),
    new PaperSize("A0",OkUnits.okCentimeters,84.1,118.9,OkPageFormID.okPageFormA0),
    //new PaperSize("定制",OkUnits.okInches,0,0)
    new PaperSize("定制",OkUnits.okCentimeters,0,0,OkPageFormID.okPageFormCUSTOM)
            };
        #endregion

    //    static PaperSize()
    //    {
    //        #region 定义纸张信息
    //        arrForms = new List<PaperSize> {
    //new PaperSize("字母",OkUnits.okInches,8.5,11),
    //new PaperSize("Legal",OkUnits.okInches,8.5,14),
    //new PaperSize("Tabloid",OkUnits.okInches,11,17),
    //new PaperSize("C",OkUnits.okInches,17,22),
    //new PaperSize("D",OkUnits.okInches,22,34),
    //new PaperSize("E",OkUnits.okInches,34,44),
    //new PaperSize("A5",OkUnits.okCentimeters,14.8,21),
    //new PaperSize("A4",OkUnits.okCentimeters,21,29.7),
    //new PaperSize("A3",OkUnits.okCentimeters,29.7,42),
    //new PaperSize("A2",OkUnits.okCentimeters,42,59.4),
    //new PaperSize("A1",OkUnits.okCentimeters,59.4,84.1),
    //new PaperSize("A0",OkUnits.okCentimeters,84.1,118.9),
    //new PaperSize("定制",OkUnits.okInches,0,0)
    //        };
    //        #endregion
    //    }

        public string name;
        public OkUnits units;
        public double fWidth;
        public double fHeight;
		public OkPageFormID formID;
		public PaperSize(string n, OkUnits u, double w, double h, OkPageFormID fid)
        {
            name = n;
            units = u;
            fWidth = w;
            fHeight = h;
			formID = fid;
        }

        /// <summary>
        /// 获取像素宽度
        /// </summary>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public double GetPixelWidth(double dpi)
        {
            if (units == OkUnits.okCentimeters)
            {
                var w = Centimeter2Dot(dpi, fWidth);// / 2.54 * dpi;
                return w;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 获取像素高
        /// </summary>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public double GetPixelHeight(double dpi)
        {
            if (units == OkUnits.okCentimeters)
            {
                var h = Centimeter2Dot(dpi, fHeight);// / 2.54 * dpi;
                return h;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

		public override string ToString()
		{
			return name;
		}

		#region static

		/// <summary>
		/// 厘米转换点数
		/// </summary>
		/// <param name="dpi"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static double Centimeter2Dot(double dpi, double size)
        {
            return size / 2.54 * dpi;
        }
        /// <summary>
        /// 点数转换为厘米
        /// </summary>
        /// <param name="dpi"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static double Dot2Centimeter(double dpi,double size)
        {
            double c = size / dpi * 2.54;
            return c;
        }
        public static PaperSize GetByName(string name)
        {
            foreach (var f in arrForms)
            {
                if (f.name == name)
                {
                    return f;
                }
            }
            return null;
        }

		/// <summary>
		/// 根据
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static PaperSize FindByFormID(OkPageFormID id)
		{
			//PaperSize custom = null;
			foreach (var ps in arrForms)
			{
				if (ps.formID == id)
				{
					return ps;
				}
				//if (ps.formID == OkPageFormID.okPageFormCUSTOM)
				//{
				//	custom = ps;
				//}
			}
			return null;
		}
		#endregion
	}
}
