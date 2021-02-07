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
    [Description("物流费用及仓储明细报表")]
    public class WLExpenseReportPlugin : SysReportBaseService
    {
        public override void Initialize()
        {
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            base.ReportProperty.IsGroupSummary = true;            
            base.Initialize();
        }

        //设置单据列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            
            var FCUSTOMERID = header.AddChild();
            FCUSTOMERID.Key = "FCUSTOMERID";
            FCUSTOMERID.FieldName = "FCUSTOMERID";
            FCUSTOMERID.Caption = new LocaleValue("客户代码", this.Context.UserLocale.LCID);
            FCUSTOMERID.ColType = SqlStorageType.Sqlvarchar;

            var FMATERIALName = header.AddChild();
            FMATERIALName.Key = "FMATERIALName";
            FMATERIALName.FieldName = "FMATERIALName";
            FMATERIALName.Caption = new LocaleValue("品种", this.Context.UserLocale.LCID);
            FMATERIALName.ColType = SqlStorageType.Sqlvarchar;

            var FBILLNO = header.AddChild();
            FBILLNO.Key = "FBILLNO";
            FBILLNO.FieldName = "FBILLNO";
            FBILLNO.Caption = new LocaleValue("外向单号", this.Context.UserLocale.LCID);
            FBILLNO.ColType = SqlStorageType.Sqlvarchar;

            var FDate = header.AddChild();
            FDate.Key = "FDate";
            FDate.FieldName = "FDate";
            FDate.Caption = new LocaleValue("下单时间", this.Context.UserLocale.LCID);
            FDate.ColType = SqlStorageType.SqlSmalldatetime;

            var FSStation = header.AddChild();
            FSStation.Key = "FSStation";
            FSStation.FieldName = "FSStation";
            FSStation.Caption = new LocaleValue("始发地", this.Context.UserLocale.LCID);
            FSStation.ColType = SqlStorageType.Sqlvarchar;

            var FSF = header.AddChild();
            FSF.Key = "FSF";
            FSF.FieldName = "FSF";
            FSF.Caption = new LocaleValue("省份", this.Context.UserLocale.LCID);
            FSF.ColType = SqlStorageType.Sqlvarchar;

            var FCITY = header.AddChild();
            FCITY.Key = "FCITY";
            FCITY.FieldName = "FCITY";
            FCITY.Caption = new LocaleValue("目的地", this.Context.UserLocale.LCID);
            FCITY.ColType = SqlStorageType.Sqlvarchar;

            var FCUSTOMERNAME = header.AddChild();
            FCUSTOMERNAME.Key = "FCUSTOMERNAME";
            FCUSTOMERNAME.FieldName = "FCUSTOMERNAME";
            FCUSTOMERNAME.Caption = new LocaleValue("客户名称", this.Context.UserLocale.LCID);
            FCUSTOMERNAME.ColType = SqlStorageType.Sqlvarchar;

            var F_asdf_Remarks = header.AddChild();
            F_asdf_Remarks.Key = "F_asdf_Remarks";
            F_asdf_Remarks.FieldName = "F_asdf_Remarks";
            F_asdf_Remarks.Caption = new LocaleValue("送货地址", this.Context.UserLocale.LCID);
            F_asdf_Remarks.ColType = SqlStorageType.Sqlvarchar;

            var FVALUE = header.AddChild();
            FVALUE.Key = "FVALUE";
            FVALUE.FieldName = "FVALUE";
            FVALUE.Caption = new LocaleValue("运输枚举值", this.Context.UserLocale.LCID);
            FVALUE.ColType = SqlStorageType.Sqlvarchar;

            var FCAPTION = header.AddChild();
            FCAPTION.Key = "FCAPTION";
            FCAPTION.FieldName = "FCAPTION";
            FCAPTION.Caption = new LocaleValue("运输", this.Context.UserLocale.LCID);
            FCAPTION.ColType = SqlStorageType.Sqlvarchar;
            //FCAPTION.Visible = false;


            var FNUMBER = header.AddChild();
            FNUMBER.Key = "FNUMBER";
            FNUMBER.FieldName = "FNUMBER";
            FNUMBER.Caption = new LocaleValue("货物", this.Context.UserLocale.LCID);
            FNUMBER.ColType = SqlStorageType.Sqlvarchar;

            var FREALQTY = header.AddChild();
            FREALQTY.Key = "FREALQTY";
            FREALQTY.FieldName = "FREALQTY";
            FREALQTY.Caption = new LocaleValue("数量", this.Context.UserLocale.LCID);
            FREALQTY.ColType = SqlStorageType.SqlDecimal;

            var FWEIGHTAMOUNT = header.AddChild();
            FWEIGHTAMOUNT.Key = "FWEIGHTAMOUNT";
            FWEIGHTAMOUNT.FieldName = "FWEIGHTAMOUNT";
            FWEIGHTAMOUNT.Caption = new LocaleValue("重量", this.Context.UserLocale.LCID);
            FWEIGHTAMOUNT.ColType = SqlStorageType.SqlDecimal;

            var FCUBICAMOUNT = header.AddChild();
            FCUBICAMOUNT.Key = "FCUBICAMOUNT";
            FCUBICAMOUNT.FieldName = "FCUBICAMOUNT";
            FCUBICAMOUNT.Caption = new LocaleValue("体积", this.Context.UserLocale.LCID);
            FCUBICAMOUNT.ColType = SqlStorageType.SqlDecimal;

            var FPRICE = header.AddChild();
            FPRICE.Key = "FPRICE";
            FPRICE.FieldName = "FPRICE";
            FPRICE.Caption = new LocaleValue("单价", this.Context.UserLocale.LCID);
            FPRICE.ColType = SqlStorageType.SqlDecimal;

            var FTRANSPORTAMOUNT = header.AddChild();
            FTRANSPORTAMOUNT.Key = "FTRANSPORTAMOUNT";
            FTRANSPORTAMOUNT.FieldName = "FTRANSPORTAMOUNT";
            FTRANSPORTAMOUNT.Caption = new LocaleValue("金额", this.Context.UserLocale.LCID);
            FTRANSPORTAMOUNT.ColType = SqlStorageType.SqlDecimal;

            var FENTRYNOTE = header.AddChild();
            FENTRYNOTE.Key = "FENTRYNOTE";
            FENTRYNOTE.FieldName = "FENTRYNOTE";
            FENTRYNOTE.Caption = new LocaleValue("备注", this.Context.UserLocale.LCID);
            FENTRYNOTE.ColType = SqlStorageType.Sqlvarchar;

            var FENTRYNOTE1 = header.AddChild();
            FENTRYNOTE1.Key = "FENTRYNOTE1";
            FENTRYNOTE1.FieldName = "FENTRYNOTE1";
            FENTRYNOTE1.Caption = new LocaleValue("备注1", this.Context.UserLocale.LCID);
            FENTRYNOTE1.ColType = SqlStorageType.Sqlvarchar;

            var FCarrierNAME = header.AddChild();
            FCarrierNAME.Key = "FCarrierNAME";
            FCarrierNAME.FieldName = "FCarrierNAME";
            FCarrierNAME.Caption = new LocaleValue("物流商", this.Context.UserLocale.LCID);
            FCarrierNAME.ColType = SqlStorageType.Sqlvarchar;
            return header;
        }
        
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            string Filter = GetFilterWhere(filter);            
            string seqFld = string.Format(base.KSQL_SEQ, OrderColumn(filter));
            //执行取数存储过程 
            string tableName1 = "wl_expense";//存储过程临时表名
            string sql1 = string.Format(@"/*dialect*/ exec rep_wl_expense_detail '{0}'",tableName1);
            DBUtils.Execute(this.Context, sql1);

            //把过滤后的数据存入到金蝶系统的临时表中
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(@"/*dialect*/
               select *,{0} into {1} from {2}", seqFld, tableName,tableName1);
            sql.AppendFormat(@"/*dialect*/ {0}", Filter);//添加条件过滤
            DBUtils.ExecuteDynamicObject(this.Context, sql.ToString());
            
            //删除存储过程的临时数据表
            string sqldelete = string.Format(@"/*dialect*/
            if exists(select * from sysobjects where name ='{0}') --判断表是否存在
	            drop table {0}", tableName1);
            DBUtils.Execute(this.Context, sqldelete);
        }

        //获取条件过滤的过滤条件
        private string GetFilterWhere(IRptParams filter)
        {
            String ConditionFilter = filter.FilterParameter.FilterString;

            StringBuilder strwhere = new StringBuilder();
            strwhere.AppendLine("Where 1=1 ");
            if (ConditionFilter.IsNullOrEmptyOrWhiteSpace()) {
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
                OrderBy = " FDATE DESC";
            }
            return OrderBy;
        }
        //
        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("FREALQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FTRANSPORTAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            return result;
        }
    }
}
