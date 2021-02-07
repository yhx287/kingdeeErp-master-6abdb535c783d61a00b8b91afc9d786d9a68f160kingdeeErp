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


namespace Report
{
    [Description("请使用")]
    public class SKUReport : SysReportBaseService
    {
        public override void Initialize()
        {
            base.ReportProperty.IsGroupSummary = true;
            InitSettingInfo();
            base.Initialize();
        }

        string tmpTbName;
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            List<SqlParam> listParam = new List<SqlParam>();
            Kingdee.BOS.Orm.DataEntity.DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            tmpTbName = tableName.Substring(0, 20);

            //快捷过滤
            if (filter.FilterParameter.CustomFilter != null)
            {

                string blnOrgId = "0";
                Kingdee.BOS.Orm.DataEntity.DynamicObject mObj = customFilter["F_JZSL_OrgId"] as Kingdee.BOS.Orm.DataEntity.DynamicObject;
                blnOrgId = mObj["Id"].ToString();
                if (blnOrgId != "0")
                {
                    SqlParam sqlParamstartdate = new SqlParam("@in_useOrgId", KDDbType.String, blnOrgId);
                    listParam.Add(sqlParamstartdate);
                }
                string FMaterialID = "";
                if (customFilter["F_JZSL_Base"] != null)
                {
                    mObj = customFilter["F_JZSL_Base"] as Kingdee.BOS.Orm.DataEntity.DynamicObject;
                    FMaterialID = mObj["Id"].ToString();
                }
                if (FMaterialID != "")
                {
                    SqlParam sqlParamstartdate = new SqlParam("@in_material", KDDbType.String, FMaterialID);
                    listParam.Add(sqlParamstartdate);
                }
                //获取到需要判断的值
                string DateValue = (customFilter["F_JZSL_Date"] == null) ? string.Empty : Convert.ToDateTime(customFilter["F_JZSL_Date"]).ToString("yyyy-MM-dd HH:mm:ss");

                if (DateValue != string.Empty)
                {
                    SqlParam sqlParamstartdate = new SqlParam("@in_datetime", KDDbType.DateTime, DateValue);
                    listParam.Add(sqlParamstartdate);
                }
            

            }
            SqlParam tablename = new SqlParam("@in_tbName", KDDbType.String, tmpTbName);
            listParam.Add(tablename);

            string sql = "/*dialect*/ exec rep_xs_sku_matching @in_tbName";
            DBUtils.Execute(Context, sql, listParam);
            string sSQL = @"select *, {0} into {1} from {2}";
            KSQL_SEQ = string.Format(KSQL_SEQ, "AreaFNAME asc");
            sSQL = string.Format(sSQL, this.KSQL_SEQ, tableName, tmpTbName);
            DBUtils.Execute(Context, sSQL);
            DBUtils.Execute(Context, "drop table " + tmpTbName);
            
        }
        private void InitSettingInfo()
        {
            SettingInfo = new PivotReportSettingInfo();
            SettingField customer = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "AreaFNAME",
                FieldName = "AreaFNAME",
                Name = new LocaleValue("大区")
            }, 0);
            customer.IsShowTotal = false;
            SettingInfo.RowTitleFields.Add(customer);
            
            SettingInfo.SelectedFields.Add(customer);

            SettingField materia = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "YMDATE",
                FieldName = "YMDATE",
                Name = new LocaleValue("年月")
            }, 0);
            SettingInfo.ColTitleFields.Add(materia);
            SettingInfo.SelectedFields.Add(materia);
            SettingField material = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FQTY",
                FieldName = "FQTY",
                Name = new LocaleValue("要货")
            }, 1);
            SettingInfo.AggregateFields.Add(material);
            SettingInfo.SelectedFields.Add(material);
            SettingField materia2 = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "FREALQTY",
                FieldName = "FREALQTY",
                Name = new LocaleValue("出货")
            }, 2);
            SettingInfo.AggregateFields.Add(materia2);
            SettingInfo.SelectedFields.Add(materia2);
            SettingField materia3 = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "Deviation",
                FieldName = "Deviation",
                Name = new LocaleValue("偏差")
            }, 3);
            SettingInfo.AggregateFields.Add(materia3);
            SettingInfo.SelectedFields.Add(materia3);
            SettingField materia4 = PivotReportSettingInfo.CreateColumnSettingField(new TextField()
            {
                Key = "Deviationrate",
                FieldName = "Deviationrate",
                Name = new LocaleValue("偏差率")
            }, 4);
            SettingInfo.AggregateFields.Add(materia4);
            SettingInfo.SelectedFields.Add(materia4);

        }
    }
}

