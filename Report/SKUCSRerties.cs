using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Core.Report.PivotReport;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Core.Metadata.GroupElement;
namespace Report
{
    [Description("分区域要货匹配表")]
    public class SKUCSRerties : SysReportBaseService
    {

        public override void Initialize()
        {
            base.Initialize();
        }
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            Kingdee.BOS.Orm.DataEntity.DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            //快捷过滤
            StringBuilder sqlwhere = new StringBuilder();
            if (filter.FilterParameter.CustomFilter != null)
            {
                sqlwhere.AppendLine(@"/*dialect*/ where ");
                string blnOrgId = "0";
                Kingdee.BOS.Orm.DataEntity.DynamicObject mObj = customFilter["F_JZSL_OrgId"] as Kingdee.BOS.Orm.DataEntity.DynamicObject;
                blnOrgId = mObj["Id"].ToString();
                sqlwhere.AppendFormat(@"/*dialect*/ FORGID = {0} ", blnOrgId);

                string FMaterialID = "";
                if (customFilter["F_JZSL_Base"] != null)
                {
                    mObj = customFilter["F_JZSL_Base"] as Kingdee.BOS.Orm.DataEntity.DynamicObject;
                    FMaterialID = mObj["Id"].ToString();
                    sqlwhere.AppendFormat(@"/*dialect*/ and FMATERIALID = {0} ", FMaterialID);
                }
                
                //获取到需要判断的值
                string DateValue = (customFilter["F_JZSL_Date"] == null) ? string.Empty : Convert.ToDateTime(customFilter["F_JZSL_Date"]).ToString("yyyy-MM-dd HH:mm:ss");
                sqlwhere.AppendFormat(@"/*dialect*/ and FYEAR = YEAR('{0}') ", DateValue);
            }

            string sql = "/*dialect*/ exec rep_xs_fqy_matching";
            DBUtils.Execute(Context, sql);
            string sSQL = @"select *, {0} into {1} from fqy {2}";
            KSQL_SEQ = string.Format(KSQL_SEQ, "FAreaNAME asc");
            sSQL = string.Format(sSQL, this.KSQL_SEQ, tableName,sqlwhere);
            DBUtils.Execute(Context, sSQL);
            DBUtils.Execute(Context, "drop table fqy");
            InitSettingInfo();
        }
        private void InitSettingInfo()
        {
            SettingInfo = new PivotReportSettingInfo();
           
            SettingInfo.IsShowGrandTotal = true;
            SettingField AreaFNAME = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FAreaNAME",
                FieldName = "FAreaNAME",
                Name = new LocaleValue("大区")
            }, 0);
            SettingInfo.RowTitleFields.Add(AreaFNAME);
            SettingInfo.SelectedFields.Add(AreaFNAME);
            
            SettingField CityFNAME = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FCityNAME",
                FieldName = "FCityNAME",
                Name = new LocaleValue("市场")
            }, 1);
            CityFNAME.IsShowTotal = false;
            SettingInfo.RowTitleFields.Add(CityFNAME);
            SettingInfo.SelectedFields.Add(CityFNAME);

            SettingField FCUSTOMERNAME = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FCUSTOMERNAME",
                FieldName = "FCUSTOMERNAME",
                Name = new LocaleValue("客户")
            }, 1);
            FCUSTOMERNAME.IsShowTotal = false;
            SettingInfo.RowTitleFields.Add(FCUSTOMERNAME);
            SettingInfo.SelectedFields.Add(FCUSTOMERNAME);

            SettingField FMATERIALGROUP = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FMATERIALGROUP",
                FieldName = "FMATERIALGROUP",
                Name = new LocaleValue("产品类别")
            }, 1);
            FMATERIALGROUP.IsShowTotal = false;
            SettingInfo.RowTitleFields.Add(FMATERIALGROUP);
            SettingInfo.SelectedFields.Add(FMATERIALGROUP);

            SettingField FMATERIALNUMBER = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FMATERIALNUMBER",
                FieldName = "FMATERIALNUMBER",
                Name = new LocaleValue("物料编码")
            }, 1);
            FMATERIALNUMBER.IsShowTotal = false;
            SettingInfo.RowTitleFields.Add(FMATERIALNUMBER);
            SettingInfo.SelectedFields.Add(FMATERIALNUMBER);

            SettingField FMATERIALNAME = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FMATERIALNAME",
                FieldName = "FMATERIALNAME",
                Name = new LocaleValue("物料名称")
            }, 1);
            FMATERIALNAME.IsShowTotal = false;
            SettingInfo.RowTitleFields.Add(FMATERIALNAME);
            SettingInfo.SelectedFields.Add(FMATERIALNAME);

            SettingField FTAXPRICE = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FTAXPRICE",
                FieldName = "FTAXPRICE",
                Name = new LocaleValue("标准出厂价")
            }, 1);
            FTAXPRICE.IsShowTotal = false;
            SettingInfo.RowTitleFields.Add(FTAXPRICE);
            SettingInfo.SelectedFields.Add(FTAXPRICE);

            SettingField YMDATE = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FYMDATE",
                FieldName = "FYMDATE",
                Name = new LocaleValue("年月")
            }, 1);
            YMDATE.IsShowTotal = false;
            SettingInfo.ColTitleFields.Add(YMDATE);
            SettingInfo.SelectedFields.Add(YMDATE);

            SettingField FQTY = PivotReportSettingInfo.CreateDataSettingField(new DecimalField()
            {
                Key = "FQTY",
                FieldName = "FQTY",
                Name = new LocaleValue("要货数量")
            }, 0,GroupSumType.Sum,"N10");
            SettingInfo.AggregateFields.Add(FQTY);
            SettingInfo.SelectedFields.Add(FQTY);

            SettingField FAMOUNT = PivotReportSettingInfo.CreateDataSettingField(new DecimalField()
            {
                Key = "FAMOUNT",
                FieldName = "FAMOUNT",
                Name = new LocaleValue("要货数值")
            }, 0, GroupSumType.Sum, "N10");
            SettingInfo.AggregateFields.Add(FAMOUNT);
            SettingInfo.SelectedFields.Add(FAMOUNT);

            SettingField FREALQTY = PivotReportSettingInfo.CreateDataSettingField(new DecimalField()
            {
                Key = "FREALQTY",
                FieldName = "FREALQTY",
                Name = new LocaleValue("出货数量")
            }, 0, GroupSumType.Sum, "N10");
            SettingInfo.AggregateFields.Add(FREALQTY);
            SettingInfo.SelectedFields.Add(FREALQTY);

            SettingField FREALAMOUNT = PivotReportSettingInfo.CreateDataSettingField(new DecimalField()
            {
                Key = "FREALAMOUNT",
                FieldName = "FREALAMOUNT",
                Name = new LocaleValue("出货数值")
            }, 0, GroupSumType.Sum, "N10");
            SettingInfo.AggregateFields.Add(FREALAMOUNT);
            SettingInfo.SelectedFields.Add(FREALAMOUNT);

            SettingField FDeviation = PivotReportSettingInfo.CreateDataSettingField(new DecimalField()
            {
                Key = "FDeviation",
                FieldName = "FDeviation",
                Name = new LocaleValue("偏差")
            }, 0, GroupSumType.Sum, "N10");
            SettingInfo.AggregateFields.Add(FDeviation);
            SettingInfo.SelectedFields.Add(FDeviation);

            SettingField FDeviationrate = PivotReportSettingInfo.CreateDataSettingField(new DecimalField()
            {
                Key = "FDeviationrate",
                FieldName = "FDeviationrate",
                Name = new LocaleValue("偏差率")
            }, 0,GroupSumType.Sum,"N10");
            FDeviationrate.IsShowTotal = false;
            SettingInfo.AggregateFields.Add(FDeviationrate);
            SettingInfo.SelectedFields.Add(FDeviationrate);

        }
    }
}
