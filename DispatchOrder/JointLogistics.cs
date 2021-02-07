using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
namespace DispatchOrder
{
    public class JointLogistics : AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            if (this.View.OpenParameter.Status.Equals(OperationStatus.ADDNEW))
            {

                DynamicObject assistant = this.Model.GetValue("F_asdf_Assistant") as DynamicObject;
                if (assistant == null) return;
                string assistantstr = assistant["Id"].ToString();//省份
                DynamicObject city = this.Model.GetValue("F_asdf_Assistant1") as DynamicObject;
                if (city == null) return;
                string citystr = city["Id"].ToString();//市
                DynamicObject county = this.Model.GetValue("F_asdf_Assistant2") as DynamicObject;
                if (county == null) return;
                string countystr = county["Id"].ToString();//县/区

                string sql = string.Format(@"/*dialect*/ select b.FID as BFID,c.FID as  CFID  from  KFLH_t_Cust100003 a
                            left join  KFLH_t_Cust100006 b on a.F_KFLH_TEXT1=b.FNUMBER 
                            left join KFLH_t_Cust100005 c on a.F_KFLH_TEXT=c.FNUMBER
                        WHERE a.F_ASDF_ASSISTANT='" + assistantstr + "' AND a.F_ASDF_ASSISTANT1='" + citystr + "' AND a.F_ASDF_ASSISTANT21='" + countystr + "'");
                DataSet dswl = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sql);
                string JHFS = this.View.Model.GetValue("FTDeliveryStatus").ToString();
                if (JHFS == "5888" || JHFS == "1" || JHFS == "2")
                {

                }
                else
                {
                   
                    if (dswl == null || dswl.Tables[0].Rows.Count == 0)
                    {
                        this.View.ShowMessage("未找到物流价格资料！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                        return;
                    }
                }
                if (dswl.Tables[0].Rows[0]["CFID"].ToString() != "")
                {
                   // this.Model.SetValue("F_asdf_Base1", dswl.Tables[0].Rows[0]["CFID"].ToString());
                }
                else
                {
                    this.View.ShowMessage("此区域始发地未维护，请维护此区域的始发地！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }
                if (dswl.Tables[0].Rows[0]["BFID"].ToString() != "")
                {
                    this.Model.SetValue("FCarrierID", dswl.Tables[0].Rows[0]["BFID"].ToString());
                }
                else
                {
                    this.View.ShowMessage("此区域承运商未维护，请维护此区域的承运商！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }
                cargo();
            }
        }
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key == "F_asdf_Assistant" || e.Field.Key == "F_asdf_Assistant1" || e.Field.Key == "F_asdf_Assistant2")
            {
                DynamicObject assistant = this.Model.GetValue("F_asdf_Assistant") as DynamicObject;
                if (assistant == null) return;
                string assistantstr = assistant["Id"].ToString();//省份
                DynamicObject city = this.Model.GetValue("F_asdf_Assistant1") as DynamicObject;
                if (city == null) return;
                string citystr = city["Id"].ToString();//市
                DynamicObject county = this.Model.GetValue("F_asdf_Assistant2") as DynamicObject;
                if (county == null) return;
                string countystr = county["Id"].ToString();//县/区

                string sql = string.Format(@"/*dialect*/ select b.FID as BFID,c.FID as  CFID  from  KFLH_t_Cust100003 a
                            left join  KFLH_t_Cust100006 b on a.F_KFLH_TEXT1=b.FNUMBER 
                            left join KFLH_t_Cust100005 c on a.F_KFLH_TEXT=c.FNUMBER
                        WHERE F_ASDF_ASSISTANT='" + assistantstr + "' AND F_ASDF_ASSISTANT1='" + citystr + "' AND F_ASDF_ASSISTANT21='" + countystr + "'");
                DataSet dswl = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sql);
                string JHFS = this.View.Model.GetValue("FTDeliveryStatus").ToString();
                if (JHFS == "5888" || JHFS == "1" || JHFS == "2")
                {

                }
                else
                {
                    if (dswl == null || dswl.Tables[0].Rows.Count == 0)
                    {
                        this.View.ShowMessage("未找到物流价格资料！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                        return;
                    }
                }
                if (dswl.Tables[0].Rows[0]["CFID"].ToString() != "")
                {
                    //this.Model.SetValue("F_asdf_Base1", dswl.Tables[0].Rows[0]["CFID"].ToString());
                }
                else
                {
                    this.View.ShowMessage("此区域始发地未维护，请维护此区域的始发地！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }
                if (dswl.Tables[0].Rows[0]["BFID"].ToString() != "")
                {
                    this.Model.SetValue("FCarrierID", dswl.Tables[0].Rows[0]["BFID"].ToString());
                }
                else
                {
                    this.View.ShowMessage("此区域承运商未维护，请维护此区域的承运商！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                }

            }
            if (e.Field.Key == "FQty") { cargo(); }
        }
        private void cargo()
        {
            var entrycount = 0;
            if (this.View.Model.GetEntryRowCount("FEntity") > 0)
            {
                entrycount = this.View.Model.GetEntryRowCount("FEntity");
            }
            if (this.View.Model.GetEntryRowCount("FSTKTRSOUTENTRY") > 0)
            {
                entrycount = this.View.Model.GetEntryRowCount("FSTKTRSOUTENTRY");
            }

            var volume = 0m;
            for (int i = 0; i < entrycount; i++)
            {
                if (this.Model.GetValue("FMaterialID", i) != null)
                {
                    DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                    if (fmaterialid == null) break;
                    string materialid = fmaterialid["Id"].ToString();//物料ID
                    decimal qty = decimal.Parse(this.View.Model.GetValue("FQty", i).ToString());//销售数量
                    string masql = string.Format(@"/*dialect*/select a.FMATERIALID, a.F_ASDF_COMBO,b.FGROSSWEIGHT,c.FNAME,b.FVOLUME  from  T_BD_MATERIAL  a
                         left join  t_BD_MaterialBase b  
                          on a.FMASTERID=b.FMATERIALID
                          left join T_BD_MATERIAL_L c on a.FMATERIALID=c.FMATERIALID
                          where  a.FMATERIALID=" + materialid + "");
                    DataSet mads = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, masql);
                    if (mads == null || mads.Tables[0].Rows.Count == 0)
                    {
                        return;
                    }
                    #region   计费立方数 计费重量

                    decimal fvolume = decimal.Parse(mads.Tables[0].Rows[0]["FVOLUME"].ToString());
                    volume = volume + qty * fvolume;
                    this.Model.SetValue("F_asdf_Qty1", qty * fvolume, i);//行总体积
                    decimal grossweight = decimal.Parse(mads.Tables[0].Rows[0]["FGROSSWEIGHT"].ToString());
                    volume = volume + qty * grossweight;
                    this.Model.SetValue("F_asdf_Qty", qty * grossweight, i);//行总重量

                    #endregion

                }
            }
        }

    }
}
