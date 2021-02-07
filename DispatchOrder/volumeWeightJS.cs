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
    [Description("直接调拨单，分布式调出单，退货通知单 体积重量的计算")]
    public class volumeWeightJS: AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            var entrycount = 0;
            if (e.Field.Key == "FQty")
            {
                //退货通知单
                if (this.View.Model.GetEntryRowCount("FEntity") > 0)
                {
                    entrycount = this.View.Model.GetEntryRowCount("FEntity");
                    var volume = 0m;
                    for (int i = 0; i < entrycount; i++)
                    {
                        if (this.Model.GetValue("FMaterialID", i) != null)
                        {
                            DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                            if (fmaterialid == null) break;
                            string materialid = fmaterialid["Id"].ToString();//物料ID
                            decimal qty = decimal.Parse(this.View.Model.GetValue("FStockQty", i).ToString());//库存数量
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
                ////直接调拨单
                if (this.View.Model.GetEntryRowCount("FBillEntry") > 0)
                {
                    entrycount = this.View.Model.GetEntryRowCount("FBillEntry");
                    var volume = 0m;
                    for (int i = 0; i < entrycount; i++)
                    {
                        if (this.Model.GetValue("FMaterialID", i) != null)
                        {
                            DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                            if (fmaterialid == null) break;
                            string materialid = fmaterialid["Id"].ToString();//物料ID
                            decimal qty = decimal.Parse(this.View.Model.GetValue("FQTY", i).ToString());//调拨数量
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
                            this.Model.SetValue("F_asdf_Qty3", qty * fvolume, i);//行总体积
                            decimal grossweight = decimal.Parse(mads.Tables[0].Rows[0]["FGROSSWEIGHT"].ToString());
                            volume = volume + qty * grossweight;
                            this.Model.SetValue("F_asdf_Qty2", qty * grossweight, i);//行总重量

                            #endregion

                        }
                    }
                }

                //分布式调出单
                if (this.View.Model.GetEntryRowCount("FSTKTRSOUTENTRY") > 0)
                {
                    entrycount = this.View.Model.GetEntryRowCount("FSTKTRSOUTENTRY");
                    var volume = 0m;
                    for (int i = 0; i < entrycount; i++)
                    {
                        if (this.Model.GetValue("FMaterialID", i) != null)
                        {
                            DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                            if (fmaterialid == null) break;
                            string materialid = fmaterialid["Id"].ToString();//物料ID
                            decimal qty = decimal.Parse(this.View.Model.GetValue("FQTY", i).ToString());//调出数量
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
    }
}
