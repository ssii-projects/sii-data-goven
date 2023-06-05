using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Model;
using System;
using System.Collections.Generic;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// 重庆市根据当地实际情况添加了表及部分字段，该任务用于更新字段：2010年确权颁证总面积（QQZMJM2010）
	/// 将mdb中表DKDC中的QQMJM2010字段中面积导入到DLXX_DK表中的JDDMJ列中
	/// </summary>
	public class UpdateQQMJ2010Task : Task
	{
		private int iUpdateCount = 0;

		/// <summary>
		/// 重庆市特殊字段数据更新
		/// </summary>
		public UpdateQQMJ2010Task()
		{
			Name = "更新2010年确权面积";
			Description = "重庆市特殊字段数据更新";
			PropertyPage = new UpdateQQMJ2010PropertyPage();
			base.OnStart += s => base.ReportInfomation("开始" + Name);
			base.OnFinish += (s, e) => base.ReportInfomation($"结束{Name}，共更新记录{iUpdateCount}条，耗时：" + base.Elapsed);

		}
		private static int ToInt(double d)
		{
			return (int)(d * 10000);
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			iUpdateCount = 0;
			var prm = PropertyPage as UpdateQQMJ2010PropertyPage;
			var dic = new Dictionary<string, double>();
			using (var mdb = DBAccess.Open(prm.FileName))
			{
				mdb.QueryCallback("select DKBM,QQMJM2010 from DKDC", r =>
				 {
					 if (r.IsDBNull(0)) {
						 ReportError("DKDC表中DKBM存在空值");
						 return false;
					 }
					 var dkbm = r.GetString(0);
					 dic[dkbm] = r.IsDBNull(1) ? 0 : SafeConvertAux.ToDouble(r.GetValue(1));
					 return true;
				 }, cancel);
			}
			//Progress.Reset(dic.Count);
			if (dic.Count > 0)
			{
				var db = MyGlobal.Workspace;
				var tgtTableName = DLXX_DK.GetTableName();
				var tgtFieldName = DLXX_DK.GetFieldName(nameof(DLXX_DK.JDDMJ));
				var dkbmField = DLXX_DK.GetFieldName(nameof(DLXX_DK.DKBM));
				try
				{
					var cnt = db.QueryOneInt($"select count(*) from {tgtTableName}");
					Progress.Reset(cnt,"载入数据...");
					var dicDkbm2Oid = new Dictionary<string,Tuple<int,int>>();
					db.QueryCallback($"select DKBM,BSM,JDDMJ from {tgtTableName} where DKBM is not null", r =>
					 {
						 var mj = ToInt(SafeConvertAux.ToDouble(r.GetValue(2)));
						 dicDkbm2Oid[r.GetString(0)] =new Tuple<int, int>(SafeConvertAux.ToInt32(r.GetValue(1)),mj);
						 Progress.Step();
					 }, cancel);
					if (cancel.Cancel()) return;
					Progress.Reset(dic.Count,"更新数据...");
					int i = 0;
					db.BeginTransaction();
					foreach (var kv in dic)
					{
						if (cancel.Cancel())
						{
							db.Rollback();
							return ;
						}
						var dkbm = kv.Key;
						if (dicDkbm2Oid.TryGetValue(dkbm, out var tuple))
						{
							var oid = tuple.Item1;
							var yMj = tuple.Item2;
							var dQQMJM2010 = kv.Value;
							if (yMj != ToInt(dQQMJM2010))
							{
								var sql = $"update {tgtTableName} set {tgtFieldName}={dQQMJM2010} where BSM={oid}";
								db.ExecuteNonQuery(sql);
								if (++i > 10000)
								{
									i = 0;
									db.Commit();
									db.BeginTransaction();
								}
							}
							++iUpdateCount;
						}
						Progress.Step();
					}
					db.Commit();
				}
				catch (Exception ex)
				{
					db.Rollback();
					ReportException(ex);
				}
			}
		}
	}
}
