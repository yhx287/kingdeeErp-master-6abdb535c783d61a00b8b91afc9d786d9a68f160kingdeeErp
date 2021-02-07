using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.BusinessEntity;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.DataEntity;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Validation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;
using System.Data;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Bill.PlugIn;
using System.ComponentModel;
using System;
namespace DispatchOrder
{
    public class XSDDJoin : AbstractBillPlugIn
    {

        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            if (this.View.OpenParameter.Status.Equals(OperationStatus.ADDNEW))
            {

                DynamicObject assistant = this.Model.GetValue("F_asdf_Assistant2", 0) as DynamicObject;
                if (assistant == null) return;
                string assistantstr = assistant["Id"].ToString();//省份
                DynamicObject city = this.Model.GetValue("F_asdf_Assistant3", 0) as DynamicObject;
                if (city == null) return;
                string citystr = city["Id"].ToString();//市
                DynamicObject county = this.Model.GetValue("F_asdf_Assistant4", 0) as DynamicObject;
                if (county == null) return;
                string countystr = county["Id"].ToString();//县/区

                string sql = string.Format(@"/*dialect*/  select b.FID as BFID,c.FID as  CFID  from  KFLH_t_Cust100003 a
                            left join  KFLH_t_Cust100006 b on a.F_KFLH_TEXT1=b.FNUMBER 
                            left join KFLH_t_Cust100005 c on a.F_KFLH_TEXT=c.FNUMBER
                        WHERE F_ASDF_ASSISTANT='" + assistantstr + "' AND F_ASDF_ASSISTANT1='" + citystr + "' AND F_ASDF_ASSISTANT21='" + countystr + "'");
                DataSet dswl = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sql);
                if (dswl == null || dswl.Tables[0].Rows.Count == 0)
                {
                    this.View.ShowMessage("未找到物流价格资料！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                    return;
                }
                if (dswl.Tables[0].Rows[0]["CFID"].ToString() != "")
                {
                    this.Model.SetValue("F_asdf_Base1", dswl.Tables[0].Rows[0]["CFID"].ToString());
                }
                else
                {
                    this.View.ShowMessage("此区域始发地未维护，请维护此区域的始发地！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }

            }
        }
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key == "F_asdf_Assistant2" || e.Field.Key == "F_asdf_Assistant3" || e.Field.Key == "F_asdf_Assistant4")
            {
                DynamicObject assistant = this.Model.GetValue("F_asdf_Assistant2", 0) as DynamicObject;
                if (assistant == null) return;
                string assistantstr = assistant["Id"].ToString();//省份
                DynamicObject city = this.Model.GetValue("F_asdf_Assistant3", 0) as DynamicObject;
                if (city == null) return;
                string citystr = city["Id"].ToString();//市
                DynamicObject county = this.Model.GetValue("F_asdf_Assistant4", 0) as DynamicObject;
                if (county == null) return;
                string countystr = county["Id"].ToString();//县/区

                string sql = string.Format(@"/*dialect*/ select b.FID as BFID,c.FID as  CFID  from  KFLH_t_Cust100003 a
                            left join  KFLH_t_Cust100006 b on a.F_KFLH_TEXT1=b.FNUMBER 
                            left join KFLH_t_Cust100005 c on a.F_KFLH_TEXT=c.FNUMBER
                        WHERE F_ASDF_ASSISTANT='" + assistantstr + "' AND F_ASDF_ASSISTANT1='" + citystr + "' AND F_ASDF_ASSISTANT21='" + countystr + "'");
                DataSet dswl = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sql);
              
                if (dswl == null || dswl.Tables[0].Rows.Count == 0)
                {
                    this.View.ShowMessage("未找到物流价格资料！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                    return;
                }
                if (dswl.Tables[0].Rows[0]["CFID"].ToString() != "")
                {
                    this.Model.SetValue("F_asdf_Base1", dswl.Tables[0].Rows[0]["CFID"].ToString());
                }
                else
                {
                    this.View.ShowMessage("此区域始发地未维护，请维护此区域的始发地！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }

            }
        }
    }
}
