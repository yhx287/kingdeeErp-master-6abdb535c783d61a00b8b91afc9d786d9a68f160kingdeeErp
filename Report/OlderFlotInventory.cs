using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using System.ComponentModel;
using Kingdee.BOS.Util;

namespace Report
{
    [Description("老批次库存报表")]
    public class OlderFlotInventory : SysReportBaseService
    {
        //初始化
        public override void Initialize()
        {
            //简单帐表类型，支持分组统计
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            base.ReportProperty.IsGroupSummary = true;
            base.Initialize();
        }

        //构建报表列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();

            //设置列的key值，字段名，标题，数据类型
            var FMATERIALNUMBER = header.AddChild();
            FMATERIALNUMBER.Key = "FMATERIALNUMBER";
            FMATERIALNUMBER.FieldName = "FMATERIALNUMBER";
            FMATERIALNUMBER.Caption = new LocaleValue("物料号", this.Context.UserLocale.LCID);
            FMATERIALNUMBER.ColType = SqlStorageType.Sqlnvarchar;

            var FLOTNUMBER = header.AddChild();
            FLOTNUMBER.Key = "FLOTNUMBER";
            FLOTNUMBER.FieldName = "FLOTNUMBER";
            FLOTNUMBER.Caption = new LocaleValue("批号", this.Context.UserLocale.LCID);
            FLOTNUMBER.ColType = SqlStorageType.Sqlnvarchar;

            var FBASEQTY = header.AddChild();
            FBASEQTY.Key = "FBASEQTY";
            FBASEQTY.FieldName = "FBASEQTY";
            FBASEQTY.Caption = new LocaleValue("库存量", this.Context.UserLocale.LCID);
            FBASEQTY.ColType = SqlStorageType.SqlDecimal;

            var FDAYS = header.AddChild();
            FDAYS.Key = "FDAYS";
            FDAYS.FieldName = "FDAYS";
            FDAYS.Caption = new LocaleValue("天数", this.Context.UserLocale.LCID);
            FDAYS.ColType = SqlStorageType.SqlInt;

            return header;
        }


        //
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            string Filter = GetFilterWhere(filter);
            string seqFld = string.Format(base.KSQL_SEQ, OrderColumn(filter));
            //取数
            string sqltemp = @"/*dialect*/
                if exists(select * from sysobjects where name ='tabletemp') 
                    drop table tabletemp;
                select FID,FMATERIALNUMBER,FLOTNUMBER,FBASEQTY,FDAYS into tabletemp
                from(
                select t0.FID,
                    --物料编码
                    t1.FNUMBER FMATERIALNUMBER,
                    --批号编码
                    t2.FNUMBER FLOTNUMBER,
                    --库存量（基本单位）
                    t0.FBaseQty FBASEQTY,
                    --物料分组
                    t1GL.FNAME FMATERIALGROUPNAME,
                    --批号编码总长度
                    len(t2.FNUMBER) int1,
                    --天数=批号中的日期-当前日期
                    DateDiff(DAY,GETDATE(),convert(datetime,substring(t2.FNUMBER,1,8))) as FDAYS
                    --库存                    
                    from  T_STK_INVENTORY t0 
                    --物料
                    left join T_BD_MATERIAL t1 on t0.FMATERIALID = t1.FMATERIALID
                    --物料分组
                    left join T_BD_MATERIALGROUP_L t1GL on t1.FMATERIALGROUP = t1GL.FID	
                    --批号主档                    
                    left join T_BD_LOTMASTER t2 on t0.FLOT =t2.FLOTID
                    --只取成品物料
                    where t1GL.FNAME in ('成品','成品-米稀','成品-饮料','成品-饼干','成品-饼稀','成品-其他') and 
                    --只支持 批号编码规则为 1.总长度为8且格式为‘yyyymmdd’，2.总长度大于等于10且格式为‘yyyymmdd’+‘其他’
                    (CASE 
                    when len(t2.FNUMBER)=8 and substring(t2.FNUMBER,len(t2.FNUMBER),len(t2.FNUMBER)) in ('0','1','2','3','4','5','6','7','8','9') then 1
                    when len(t2.FNUMBER)>=10 then 1
                    else 0 end) =1
                ) table1";
            DBUtils.Execute(this.Context, sqltemp);
            //把过滤后的数据存入到金蝶系统的临时表中
            String sql = string.Format(@"/*dialect*/
               select FID,FMATERIALNUMBER,FLOTNUMBER,FBASEQTY,FDAYS,{0} into {1} from 
                 tabletemp {2}", seqFld, tableName, Filter);
            DBUtils.ExecuteDynamicObject(this.Context, sql);
            //删除临时表
            string sqldrop = @"drop table tabletemp";
            DBUtils.Execute(this.Context, sqldrop);
        }

        //获取条件过滤的过滤条件
        private string GetFilterWhere(IRptParams filter)
        {
            String ConditionFilter = filter.FilterParameter.FilterString;

            StringBuilder strwhere = new StringBuilder();
            strwhere.AppendLine("Where 1=1 ");
            if (ConditionFilter.IsNullOrEmptyOrWhiteSpace())
            {
                return strwhere.ToString();
            }
            else
            {
                strwhere.AppendFormat("and {0}", ConditionFilter);
                return strwhere.ToString();
            }
        }

        //获取过滤条件-排序
        private string OrderColumn(IRptParams filter)
        {
            string OrderBy = "";
            string datasort = Convert.ToString(filter.FilterParameter.SortString);//排序
            if (datasort != "")
            {
                OrderBy = " " + datasort + " ";
            }
            else
            {
                OrderBy = " FID";
            }
            return OrderBy;
        }
        //
        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("FBASEQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));            
            return result;
        }
    }
}
