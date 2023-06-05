using Agro.LibMapServer;
using Agro.Library.Model;
using DataGovenServer.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DataGovenServer.Controllers
{

	[ApiController]
	[Route("[controller]")]
	public class ImportDcdkController: ControllerBase
	{
		public class FeaturePostData
		{
			public string mapName { get; set; }
			/// <summary>
			/// layerID
			/// </summary>
			public int? lid { get; set; }
			/// <summary>
			/// 变更原因
			/// </summary>
			public string bgyy { get; set; }
			public FeatureJO jo { get; set; }
		}

		[HttpPost]
		public IActionResult Post([FromBody] FeaturePostData data)
		{
			Console.WriteLine($"ImportDcdkController.Post mapName is {data.mapName}");
			var p = SoketMaps.Instance.GetMapServer(data.mapName);
			if (p == null) return NotFound();

			if (data.lid == null || !p.dicFeatureLayer.TryGetValue((int)data.lid, out FeatureLayer fl))
			{
				//throw new Exception("数据格式异常");
				return Ok(new CommonResJO("数据格式异常"));
			}
			var db = fl.db;
			var service = ImportDcdkService.Instance(db);

			using var con = db.OpenConnection();
			var ft = FeatureUtil.ParseFeature(p, data.jo, fl,con);
			var en = new DLXX_DK();
			EntityUtil.WriteToEntity(ft, en);
			en.CJSJ = DateTime.Now;
			en.ID = Guid.NewGuid().ToString();
			try
			{
				con.BeginTransaction();
				if (en.BSM <= 0)
				{
					service.Append(con, EBGLX.Xinz, en);
				}
				else
				{
					service.Append(con, EBGLX.Txbg, en, data.bgyy);
				}
				con.Commit();
				UpdateEditVersion(p, fl.layerConfig.FeatureLayer.DBID);
			}
			catch (Exception ex)
			{
				con.Rollback();
				//throw ex;
				return Ok(new CommonResJO(ex.Message));
			}
			return Ok(new { Id = en.BSM });
		}

		[HttpPost("cut")]
		public IActionResult Cut([FromBody] FeatureCutJO jo)
		{
			var p = SoketMaps.Instance.GetMapServer(jo.mapName);
			if (p == null) return NotFound();

			if (!p.dicFeatureLayer.TryGetValue(jo.lid, out FeatureLayer fl))
			{
				throw new Exception("数据格式异常");
			}
			var service = ImportDcdkService.Instance(fl.db);
			var oid = jo.oid;
			var lstOids = new List<int>();

			using var con = fl.db.OpenConnection();
			var oldFeature = fl.GetFeature(con, oid);
			if (oldFeature == null)
			{
				throw new Exception($"BSM={oid}的记录不存在！");
			}
			try
			{
				con.BeginTransaction();
				foreach (var j in jo.jos)
				{
					var ft = FeatureUtil.ParseFeature(p, j, fl,con);
					var flds = FieldsJOUtil.ParseFields(p, j.flds);
					FeatureUtil.OverWrite(oldFeature, ft, flds);
					var en = new DLXX_DK();
					EntityUtil.WriteToEntity(ft, en);
					en.CJSJ = DateTime.Now;
					en.ID = Guid.NewGuid().ToString();
					service.Append(con, EBGLX.Fenge, en);
					lstOids.Add(en.BSM);
				}
				con.Commit();
				UpdateEditVersion(p, fl.layerConfig.FeatureLayer.DBID);
				var res = new FeatureCutResJO()
				{
					Oids = lstOids.ToArray()
				};
				return Ok(res);
			}
			catch (Exception ex)
			{
				con.Rollback();
				return Ok(new CommonResJO(ex.Message));
			}
		}

		[HttpDelete]
		public IActionResult Delete(string map, int dbID, string tbn, int oid)
		{
			var p = SoketMaps.Instance.GetMapServer(map);
			if (p == null) return NotFound();
			if (!p.dicID2DB.TryGetValue(dbID, out IFeatureWorkspace db))
			{
				return NotFound();
			}
			var service = ImportDcdkService.Instance(db);
			using var con = db.OpenConnection();
			try
			{
				if (!db.IsTableExists(con, tbn))
				{
					throw new Exception($"TableName={tbn} not exist!");
				}
				con.BeginTransaction();
				service.Delete(con, oid);
				con.Commit();
				UpdateEditVersion(p, dbID);
			}
			catch (Exception ex)
			{
				con.Rollback();
				return Ok(new CommonResJO(ex.Message));
			}
			return Ok(new { });
		}

		private void UpdateEditVersion(SoketMapServer p,int dbID)
		{
			//p.tableEditVersion.UpdateEditVersion(fl.layerConfig.FeatureLayer.DBID, fl.layerConfig.FeatureLayer.TableName);
			p.tableEditVersion.UpdateEditVersion(dbID, DLXX_DK.GetTableName());
			p.tableEditVersion.UpdateEditVersion(dbID, DLXX_DK_JZD.GetTableName());
			p.tableEditVersion.UpdateEditVersion(dbID, DLXX_DK_JZX.GetTableName());
		}
	}
}
