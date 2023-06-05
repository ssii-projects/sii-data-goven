using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
yxm created at 2019/1/16 10:31:03
*/
namespace Agro.Module.SketchMap
{
	public class SkecthMapProperty
	{
		#region Const

		#endregion

		#region Propertys

		/// <summary>
		/// 制图者
		/// </summary>
		public string DrawPerson { get; set; }

		/// <summary>
		/// 制图日期
		/// </summary>
		public DateTime? DrawDate { get; set; }

		/// <summary>
		/// 审核者
		/// </summary>
		public string CheckPerson { get; set; }

		/// <summary>
		/// 审核日期
		/// </summary>
		public DateTime? CheckDate { get; set; }

		/// <summary>
		/// 编制单位
		/// </summary>
		public string Company { get; set; }

		/// <summary>
		/// 输出路径
		/// </summary>
		public string OutputPath { get; set; }

		/// <summary>
		/// 是否填写四至
		/// </summary>
		public bool UseNeighbor { get; set; }

		/// <summary>
		/// 保存为Doc格式
		/// </summary>
		public bool SaveDocFormat { get; set; }

		/// <summary>
		/// 保存为Pdf格式
		/// </summary>
		public bool SavePdfFormat { get; set; }

		/// <summary>
		/// 保存为Jpg格式
		/// </summary>
		public bool SaveJpgFormat { get; set; }

		/// <summary>
		/// 是否覆盖
		/// </summary>
		public bool IsConver { get; set; }

		/// <summary>
		/// 是否是承包地块示意图
		/// </summary>
		public int IsSketchMap { get; set; }

		#endregion

		#region Ctor

		public SkecthMapProperty()
		{
			IsConver = true;
			IsSketchMap = 0;
		}

		#endregion

		#region Static

		/// <summary>
		/// 系列化数据
		/// </summary>
		/// <param name="version"></param>
		public static void SerializeXml(SkecthMapProperty mapProperty)
		{
			if (mapProperty == null)
			{
				return;
			}
			string fileName = InitalizeFileName();
			if (string.IsNullOrEmpty(fileName))
			{
				return;
			}
			try
			{
				Serialization.SerializeXml(fileName, mapProperty);
			}
			catch (SystemException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// 反系列化图层数据
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static SkecthMapProperty DeserializeXml()
		{
			string fileName = InitalizeFileName();
			if (!System.IO.File.Exists(fileName))
			{
				return null;
			}
			try
			{
				SkecthMapProperty layers = Serialization.DeserializeXml(fileName, typeof(SkecthMapProperty)) as SkecthMapProperty;
				return layers;
			}
			catch (SystemException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			return null;
		}

		/// <summary>
		/// 初始化图层文件名称
		/// </summary>
		/// <returns></returns>
		public static string InitalizeFileName()
		{
			try
			{
				string filePath = AppDomain.CurrentDomain.BaseDirectory + @"persist\";
				if (!System.IO.Directory.Exists(filePath))
				{
					System.IO.Directory.CreateDirectory(filePath);
				}
				string fileName = filePath + "MapProperty.xml";
				return fileName;
			}
			catch (SystemException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			return "";
		}

		#endregion
	}
}
