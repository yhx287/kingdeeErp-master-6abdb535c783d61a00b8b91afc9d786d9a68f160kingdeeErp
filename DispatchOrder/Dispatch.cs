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
    public class Dispatch : AbstractBillPlugIn
    {
        [Description("发运单")]
        public override void AfterBindData(EventArgs e)
        {
            this.View.GetControl("F_SJWE_Entity").Enabled = false;
            base.AfterBindData(e);
            if (this.View.OpenParameter.Status.Equals(OperationStatus.ADDNEW))
            {
                
                DynamicObject carrier = this.Model.GetValue("FCarrierID", 0) as DynamicObject;
                if (carrier == null) return;
                string carrierstr = carrier["Number"].ToString();//承运商
                DynamicObject provenance = this.Model.GetValue("FCarrierID1", 0) as DynamicObject;
                if (provenance == null) return;
                string provenancestr = provenance["Number"].ToString();//始发地
                DynamicObject assistant = this.Model.GetValue("F_asdf_Assistant", 0) as DynamicObject;
                if (assistant == null) return;
                string assistantstr = assistant["Id"].ToString();//省份
                DynamicObject city = this.Model.GetValue("F_asdf_Assistant1", 0) as DynamicObject;
                if (city == null) return;
                string citystr = city["Id"].ToString();//市
                DynamicObject county = this.Model.GetValue("F_asdf_Assistant21", 0) as DynamicObject;
                if (county == null) return;
                string countystr = county["Id"].ToString();//县/区
                DateTime dt = DateTime.Parse(this.Model.GetValue("F_KFLH_DATE").ToString());
                string sql = string.Format(@"/*dialect*/ select F_KFLH_QTY1 as aging  from  KFLH_t_Cust100003 
                WHERE F_ASDF_ASSISTANT='" + assistantstr + "' AND F_ASDF_ASSISTANT1='" + citystr + "' AND F_ASDF_ASSISTANT21='" + countystr + "' AND F_KFLH_TEXT='" + provenancestr + "' AND F_KFLH_TEXT1='" + carrierstr + "'");
                DataSet dsX = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sql);
                decimal aging = 0;
                string tdelivery = this.Model.GetValue("FTDeliveryStatus").ToString();

                if (tdelivery != "1" && tdelivery != "2" && tdelivery != "5888")
                {
                    if (dsX == null || dsX.Tables[0].Rows.Count == 0)
                    {
                        this.View.ShowMessage("未找到物流价格资料！或此区域承运商未维护，请维护此区域的承运商！或此区域始发地未维护，请维护此区域的始发地！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                        return;
                    }
                }
                if (dsX != null && dsX.Tables[0].Rows.Count != 0)
                {
                    if (dsX.Tables[0].Rows[0]["aging"].ToString() != "")
                    {
                        aging = decimal.Parse(dsX.Tables[0].Rows[0]["aging"].ToString());
                    }
                }
                int count = Convert.ToInt32(Math.Truncate(aging));
                this.Model.SetValue("F_KFLH_Qty2", count);
                dt = dt.AddDays(count);
                this.Model.SetValue("F_KFLH_Date1", dt);
                decimal allcubic = 0m;//总立方数
                decimal alleight = 0m;//总重量数
                decimal allcount = 0m;// 商品总数量
                decimal allcubicamount = 0m; // 一级承运商运费（立方）合计
                decimal allweightamount = 0m; // 一级承运商运费（重量）合计
                decimal freightsubtotal = 0m;// 运费小计
                #region 单据体逻辑处理
                decimal allvolume = getvolume();
                var entrycount = this.View.Model.GetEntryRowCount("F_SJWE_Entity");
                for (int i = 0; i < entrycount; i++)
                {
                    if (this.Model.GetValue("FMaterialID", i) != null && this.Model.GetValue("FRealQty", i) != null)
                    {
                        DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                        if (fmaterialid == null) break;
                        string materialid = fmaterialid["Id"].ToString();//物料ID
                        decimal realqty = decimal.Parse(this.Model.GetValue("FRealQty", i).ToString());//数量
                        allcount = allcount + realqty;
                        string masql = string.Format(@"/*dialect*/ select a.FMATERIALID, a.F_ASDF_COMBO,b.FGROSSWEIGHT,c.FNAME,b.FVOLUME  from  T_BD_MATERIAL  a
                         left join  t_BD_MaterialBase b  
                          on a.FMASTERID=b.FMATERIALID
                          left join T_BD_MATERIAL_L c on a.FMATERIALID=c.FMATERIALID
                          where  a.FMATERIALID=" + materialid + "");
                        DataSet mads = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, masql);
                        int billing = 0;//计费方式
                        decimal grossweight = 0;//毛重
                        if (mads == null || mads.Tables[0].Rows.Count == 0)
                        {
                            this.View.ShowMessage("物料未维护计费方式和毛重！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                            break;
                        }
                        if (mads.Tables[0].Rows[0]["F_ASDF_COMBO"] != null)
                        {
                            if (mads.Tables[0].Rows[0]["F_ASDF_COMBO"].ToString().Trim() != "")
                            {
                                billing = Convert.ToInt32(Math.Truncate(decimal.Parse(mads.Tables[0].Rows[0]["F_ASDF_COMBO"].ToString())));
                            }
                        }
                        if (billing == 2)
                        {
                            if (mads.Tables[0].Rows[0]["FGROSSWEIGHT"].ToString() != "")
                            {
                                grossweight = decimal.Parse(mads.Tables[0].Rows[0]["FGROSSWEIGHT"].ToString());
                            }
                        }
                        if (billing == 1)
                        {
                            if (mads.Tables[0].Rows[0]["FVOLUME"].ToString() != "")
                            {
                                grossweight = decimal.Parse(mads.Tables[0].Rows[0]["FVOLUME"].ToString());
                            }
                        }

                        #region   标准方数、计费立方数 标准重量、计费重量
                        string JHFS = this.View.Model.GetValue("FTDeliveryStatus").ToString();
                        decimal volume = grossweight * realqty;
                        decimal interval = 0m;
                        if (JHFS == "5888" || JHFS == "1" || JHFS == "2")
                        {

                        }
                        else
                        {
                            this.View.GetControl("F_KFLH_Amount21").Enabled = false;
                            this.View.GetControl("F_KFLH_Amount211").Enabled = false;
                            if (billing == 1)
                            {
                                this.Model.SetValue("Fcubic", volume, i);
                                this.Model.SetValue("Fcubicamount", volume, i);
                                this.Model.SetValue("FWeightQty", 0, i);
                                this.Model.SetValue("Fweightamount", 0, i);
                                //总立方数
                                allcubic = allcubic + volume;

                            }
                            else if (billing == 2)
                            {
                                this.Model.SetValue("Fcubic", 0, i);
                                this.Model.SetValue("Fcubicamount", 0, i);
                                this.Model.SetValue("FWeightQty", volume, i);
                                this.Model.SetValue("Fweightamount", volume, i);
                                alleight = alleight + volume;
                            }
                        }
                        #endregion

                        #region  获取 区间
                        string qtysql = string.Format(@"/*dialect*/  select distinct  F_KFLH_QTY  from  KFLH_t_Cust100003 order by F_KFLH_QTY ");
                        DataSet qtyds = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, qtysql);
                        if (qtysql == null || qtyds.Tables[0].Rows.Count == 0)
                        {
                            break;
                        }
                        for (int j = 0; j < qtyds.Tables[0].Rows.Count; j++)
                        {
                            decimal qty = decimal.Parse(qtyds.Tables[0].Rows[j]["F_KFLH_QTY"].ToString());
                            if (allvolume >= 99999)
                            {
                                interval = 99999;
                                break;
                            }
                            if (qty > allvolume)
                            {
                                interval = qty;
                                break;
                            }

                        }
                        #endregion

                        #region 获取单价
                        string pricesql = string.Format(@"/*dialect*/ select F_KFLH_PRICE  from  KFLH_t_Cust100003 
                        WHERE F_ASDF_ASSISTANT='" + assistantstr + "' AND F_ASDF_ASSISTANT1='" + citystr + "' AND F_ASDF_ASSISTANT21='" + countystr + "' AND F_KFLH_TEXT='" + provenancestr + "' AND F_KFLH_TEXT1='" + carrierstr + "' AND FTRACESTATUS='" + billing + "' AND F_KFLH_QTY='" + interval + "'");
                        DataSet dsprice = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, pricesql);
                        if (tdelivery != "1" && tdelivery != "2" && tdelivery != "5888")
                        {
                            if (dsprice == null || dsprice.Tables[0].Rows.Count == 0)
                            {
                                this.View.ShowMessage(mads.Tables[0].Rows[0]["FNAME"].ToString() + "未找到物流价格资料！", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                                if (!one)
                                {
                                    this.Model.SetValue("FPrice", 0, i);
                                }
                                if (billing == 1)
                                {
                                    if (this.Model.GetValue("Fcubicamount", i) != null)
                                    {
                                        if (!one)
                                        {
                                            this.Model.SetValue("FTransportAmount", 0, i);
                                        }
                                        allcubicamount = allcubicamount + 0;
                                    }
                                }
                                else if (billing == 2)
                                {
                                    if (!one)
                                    {
                                        this.Model.SetValue("FTransportAmount", 0, i);
                                    }
                                    allweightamount = allweightamount + 0;
                                }
                            }
                            else
                            {
                                if (dsprice.Tables[0].Rows[0]["F_KFLH_PRICE"].ToString() != "")
                                {
                                    decimal price = decimal.Parse(dsprice.Tables[0].Rows[0]["F_KFLH_PRICE"].ToString());
                                    if (!one)
                                    {
                                        this.Model.SetValue("FPrice", price, i);
                                    }
                                    if (billing == 1)
                                    {
                                        if (this.Model.GetValue("Fcubicamount", i) != null)
                                        {
                                            decimal cubicamount = decimal.Parse(this.Model.GetValue("Fcubicamount", i).ToString()) * price;//一级承运商运费（立方）金额
                                            if (!one)
                                            {
                                                this.Model.SetValue("FTransportAmount", cubicamount, i);
                                            }
                                            allcubicamount = allcubicamount + cubicamount;
                                        }

                                    }
                                    else if (billing == 2)
                                    {
                                        decimal weightamount = decimal.Parse(this.Model.GetValue("Fweightamount", i).ToString()) * price;//一级承运商运费（重量）金额
                                        if (!one)
                                        {
                                            this.Model.SetValue("FTransportAmount", weightamount, i);
                                        }
                                        allweightamount = allweightamount + weightamount;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion
                freightsubtotal = allcubicamount + allweightamount;//运费小计
                this.Model.SetValue("F_KFLH_QTY", allcubic); //总立方数
                this.Model.SetValue("F_KFLH_Qty1", alleight); //总重量数
                this.Model.SetValue("F_KFLH_Qty11", allcount); //商品总数量
                if (this.Model.GetValue("FTDeliveryStatus") != null)
                {

                    string tdeliverys = this.Model.GetValue("FTDeliveryStatus").ToString();

                    if (tdeliverys != "1" && tdeliverys != "2" && tdeliverys != "5888")
                    {
                        this.Model.SetValue("F_KFLH_Amount21", allcubicamount); //一级承运商运费（立方）合计
                        this.Model.SetValue("F_KFLH_Amount211", allweightamount); //一级承运商运费（重量）合计
                    }
                }

                //this.Model.SetValue("F_KFLH_Amount22", freightsubtotal); //运费小计
                //this.Model.SetValue("F_KFLH_Amount2211", freightsubtotal); //总运费

                //if (this.Model.GetValue("FTDeliveryStatus") != null)
                //{
                //    string tdelivery = this.Model.GetValue("FTDeliveryStatus").ToString();
                //    if (tdelivery == "1" || tdelivery == "2")
                //    {
                //        for (int i = 0; i < entrycount; i++)
                //        {
                //            this.Model.SetValue("FPrice", 0, i);
                //            this.Model.SetValue("FTransportAmount", 0, i);
                //        }

                //    }

                //}
            }
        }
        bool one = false;
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            var entrycount = this.View.Model.GetEntryRowCount("F_SJWE_Entity");
            #region  计费立方数、计费重量、承运商、省份、市、县/区
            if (e.Field.Key == "Fcubicamount" || e.Field.Key == "Fweightamount" || e.Field.Key == "FCarrierID" 
                || e.Field.Key == "FCarrierID1" || e.Field.Key == "F_asdf_Assistant" 
                || e.Field.Key == "F_asdf_Assistant1" || e.Field.Key == "F_asdf_Assistant21" 
                || e.Field.Key == "FTDeliveryStatus" || e.Field.Key == "FPrice")
            {
                decimal totalfreight = 0m;//总运费
                DynamicObject carrier = this.Model.GetValue("FCarrierID", 0) as DynamicObject;
                if (carrier == null) return;
                string carrierstr = carrier["Number"].ToString();//承运商
                DynamicObject provenance = this.Model.GetValue("FCarrierID1", 0) as DynamicObject;
                if (provenance == null) return;
                string provenancestr = provenance["Number"].ToString();//始发地
                DynamicObject assistant = this.Model.GetValue("F_asdf_Assistant", 0) as DynamicObject;
                if (assistant == null) return;
                string assistantstr = assistant["Id"].ToString();//省份
                DynamicObject city = this.Model.GetValue("F_asdf_Assistant1", 0) as DynamicObject;
                if (city == null) return;
                string citystr = city["Id"].ToString();//市
                DynamicObject county = this.Model.GetValue("F_asdf_Assistant21", 0) as DynamicObject;
                if (county == null) return;
                string countystr = county["Id"].ToString();//县/区
                decimal allcubic = 0m;//总立方数
                decimal alleight = 0m;//总重量数
                decimal allcount = 0m;// 商品总数量
                decimal allcubicamount = 0m; // 一级承运商运费（立方）合计
                decimal allweightamount = 0m; // 一级承运商运费（重量）合计
                decimal freightsubtotal = 0m;// 运费小计
                #region 单据体逻辑处理
                decimal allvolume = getvolume();
                for (int i = 0; i < entrycount; i++)
                {
                    if (this.Model.GetValue("FMaterialID", i) != null && this.Model.GetValue("FRealQty", i) != null)
                    {
                        DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                        if (fmaterialid == null) break;
                        string materialid = fmaterialid["Id"].ToString();//物料ID
                        decimal realqty = decimal.Parse(this.Model.GetValue("FRealQty", i).ToString());//数量
                        allcount = allcount + realqty;
                        string masql = string.Format(@"/*dialect*/select a.FMATERIALID, a.F_ASDF_COMBO,b.FGROSSWEIGHT,c.FNAME,b.FVOLUME  from  T_BD_MATERIAL  a
                         left join  t_BD_MaterialBase b  
                          on a.FMASTERID=b.FMATERIALID
                          left join T_BD_MATERIAL_L c on a.FMATERIALID=c.FMATERIALID
                          where  a.FMATERIALID=" + materialid + "");
                        DataSet mads = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, masql);
                        int billing = 0;//计费方式
                        if (mads == null || mads.Tables[0].Rows.Count == 0)
                        {
                            break;
                        }
                        if (mads.Tables[0].Rows[0]["F_ASDF_COMBO"] != null)
                        {
                            if (mads.Tables[0].Rows[0]["F_ASDF_COMBO"].ToString() != "")
                            {
                                billing = Convert.ToInt32(Math.Truncate(decimal.Parse(mads.Tables[0].Rows[0]["F_ASDF_COMBO"].ToString()))); ;
                            }
                        }
                        #region   标准方数、计费立方数 标准重量、计费重量
                        decimal volume = 0m;
                        decimal interval = 0m;
                        if (billing == 1)
                        {
                            volume = decimal.Parse(this.Model.GetValue("Fcubicamount", i).ToString());
                            allcubic = allcubic + volume;

                        }
                        else if (billing == 2)
                        {
                            volume = decimal.Parse(this.Model.GetValue("Fweightamount", i).ToString());
                            alleight = alleight + volume;
                        }

                        #endregion

                        #region  获取 区间
                        string qtysql = string.Format(@"/*dialect*/  select distinct  F_KFLH_QTY  from  KFLH_t_Cust100003 ");
                        DataSet qtyds = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, qtysql);
                        if (qtysql == null || qtyds.Tables[0].Rows.Count == 0)
                        {
                            break;
                        }
                        for (int j = 0; j < qtyds.Tables[0].Rows.Count; j++)
                        {
                            decimal qty = decimal.Parse(qtyds.Tables[0].Rows[j]["F_KFLH_QTY"].ToString());
                            if (allvolume >= 99999)
                            {
                                interval = 99999;
                                break;
                            }
                            if (qty > allvolume)
                            {
                                interval = qty;
                                break;
                            }

                        }
                        #endregion

                        #region 获取单价/一级承运商运费（立方）金额/一级承运商运费（重量）金额
                        string pricesql = string.Format(@"/*dialect*/ select F_KFLH_PRICE   from  KFLH_t_Cust100003 
                        WHERE F_ASDF_ASSISTANT='" + assistantstr + "' AND F_ASDF_ASSISTANT1='" + citystr + "' AND F_ASDF_ASSISTANT21='" + countystr + "' AND F_KFLH_TEXT='" + provenancestr + "' AND F_KFLH_TEXT1='" + carrierstr + "' AND FTRACESTATUS='" + billing + "' AND F_KFLH_QTY='" + interval + "'");
                        DataSet dsprice = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, pricesql);
                        decimal price = 0m;
                        if (dsprice == null || dsprice.Tables[0].Rows.Count == 0)
                        {
                            price = decimal.Parse(this.Model.GetValue("FPrice", i).ToString());
                            if (price <= 0)
                            {
                                if (billing == 1)
                                {
                                    if (this.Model.GetValue("Fcubicamount", i) != null)
                                    {
                                        if (!one)
                                        {
                                            this.Model.SetValue("FTransportAmount", 0, i);
                                        }
                                        allcubicamount = allcubicamount + 0;
                                    }
                                }
                                else if (billing == 2)
                                {
                                    if (!one)
                                    {
                                        this.Model.SetValue("FTransportAmount", 0, i);
                                    }
                                    allweightamount = allweightamount + 0;
                                }
                            }
                            else
                            {
                                if (this.Model.GetValue("FTDeliveryStatus") != null)
                                {
                                    string tdelivery = this.Model.GetValue("FTDeliveryStatus").ToString();
                                    if (tdelivery != "1" || tdelivery != "2")
                                    {
                                        if (billing == 1)
                                        {
                                            if (this.Model.GetValue("Fcubicamount", i) != null)
                                            {
                                                decimal cubicamount = decimal.Parse(this.Model.GetValue("Fcubicamount", i).ToString()) * price;//一级承运商运费（立方）金额
                                                if (!one)
                                                {
                                                    this.Model.SetValue("FTransportAmount", cubicamount, i);
                                                }
                                                allcubicamount = allcubicamount + cubicamount;
                                            }
                                        }
                                        else if (billing == 2)
                                        {
                                            decimal weightamount = decimal.Parse(this.Model.GetValue("Fweightamount", i).ToString()) * price;//一级承运商运费（重量）金额
                                            if (!one)
                                            {
                                                this.Model.SetValue("FTransportAmount", weightamount, i);
                                            }
                                            allweightamount = allweightamount + weightamount;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (this.Model.GetValue("FTDeliveryStatus") != null)
                            {
                                string tdelivery = this.Model.GetValue("FTDeliveryStatus").ToString();
                                if (tdelivery != "1" || tdelivery != "2")
                                {
                                    if (dsprice.Tables[0].Rows[0]["F_KFLH_PRICE"].ToString() != "")
                                    {
                                        price = decimal.Parse(dsprice.Tables[0].Rows[0]["F_KFLH_PRICE"].ToString());
                                        if (!one)
                                        {
                                            this.Model.SetValue("FPrice", price, i);
                                        }
                                        if (billing == 1)
                                        {
                                            if (this.Model.GetValue("Fcubicamount", i) != null)
                                            {
                                                decimal cubicamount = decimal.Parse(this.Model.GetValue("Fcubicamount", i).ToString()) * price;//一级承运商运费（立方）金额
                                                if (!one)
                                                {
                                                    this.Model.SetValue("FTransportAmount", cubicamount, i);
                                                }
                                                allcubicamount = allcubicamount + cubicamount;
                                            }
                                        }
                                        else if (billing == 2)
                                        {
                                            decimal weightamount = decimal.Parse(this.Model.GetValue("Fweightamount", i).ToString()) * price;//一级承运商运费（重量）金额
                                            if (!one)
                                            {
                                                this.Model.SetValue("FTransportAmount", weightamount, i);
                                            }
                                            allweightamount = allweightamount + weightamount;
                                        }
                                    }
                                }
                                else
                                {
                                    price = decimal.Parse(this.Model.GetValue("FPrice", i).ToString());
                                    if (billing == 1)
                                    {
                                        if (this.Model.GetValue("Fcubicamount", i) != null)
                                        {
                                            decimal cubicamount = decimal.Parse(this.Model.GetValue("Fcubicamount", i).ToString()) * price;//一级承运商运费（立方）金额
                                            if (!one)
                                            {
                                                this.Model.SetValue("FTransportAmount", cubicamount, i);
                                            }
                                            allcubicamount = allcubicamount + cubicamount;
                                            //this.Model.SetValue("F_KFLH_Amount21", allcubicamount); //一级承运商运费（立方）合计

                                        }
                                    }
                                    else if (billing == 2)
                                    {
                                        decimal weightamount = decimal.Parse(this.Model.GetValue("Fweightamount", i).ToString()) * price;//一级承运商运费（重量）金额
                                        if (!one)
                                        {
                                            this.Model.SetValue("FTransportAmount", weightamount, i);
                                        }
                                        allweightamount = allweightamount + weightamount;
                                        //this.Model.SetValue("F_KFLH_Amount211", allweightamount); //一级承运商运费（重量）合计
                                    }

                                }
                            }
                        }

                    }
                    #endregion
                }


                #endregion
                freightsubtotal = allcubicamount + allweightamount;//运费小计

                this.Model.SetValue("F_KFLH_QTY", allcubic); //总立方数
                this.Model.SetValue("F_KFLH_Qty1", alleight); //总重量数
                this.Model.SetValue("F_KFLH_Qty11", allcount); //商品总数量
                if (!one)
                {
                    if (this.Model.GetValue("FTDeliveryStatus") != null)
                    {

                        string tdelivery = this.Model.GetValue("FTDeliveryStatus").ToString();

                        if (tdelivery != "1" && tdelivery != "2" && tdelivery != "5888")
                        {
                            this.Model.SetValue("F_KFLH_Amount21", allcubicamount); //一级承运商运费（立方）合计
                            this.Model.SetValue("F_KFLH_Amount211", allweightamount); //一级承运商运费（重量）合计
                        }
                    }
                }
                //this.Model.SetValue("F_KFLH_Amount22", freightsubtotal); //运费小计
                totalfreight = totalfreight + freightsubtotal;
                if (this.Model.GetValue("F_KFLH_Amount") != null)
                {
                    decimal deductions = decimal.Parse(this.Model.GetValue("F_KFLH_Amount").ToString());//异常扣款
                    totalfreight = totalfreight - deductions;
                }
                if (this.Model.GetValue("F_KFLH_Amount1") != null)
                {
                    decimal delivery = decimal.Parse(this.Model.GetValue("F_KFLH_Amount1").ToString());//标准送货费
                    totalfreight = totalfreight + delivery;
                }
                if (this.Model.GetValue("F_KFLH_Amount221") != null)
                {
                    decimal service = decimal.Parse(this.Model.GetValue("F_KFLH_Amount221").ToString());//增值服务费
                    totalfreight = totalfreight + service;
                }
                if (this.Model.GetValue("F_KFLH_Amount2") != null)
                {
                    decimal handling = decimal.Parse(this.Model.GetValue("F_KFLH_Amount2").ToString());//装卸费
                    totalfreight = totalfreight + handling;
                }

                if (totalfreight > 0m)
                {
                    //this.Model.SetValue("F_KFLH_Amount2211", totalfreight); //总运费
                }
                //if (e.Field.Key == "FTDeliveryStatus")
                //{

                //    if (this.Model.GetValue("FTDeliveryStatus") != null)
                //    {
                //        string tdelivery = this.Model.GetValue("FTDeliveryStatus").ToString();
                //        if (tdelivery == "1" || tdelivery == "2")
                //        {
                //            //this.Model.SetValue("F_KFLH_QTY", 0); //总立方数
                //            //this.Model.SetValue("F_KFLH_Qty1", 0); //总重量数
                //            //this.Model.SetValue("F_KFLH_Amount21", 0); //一级承运商运费（立方）合计
                //            //this.Model.SetValue("F_KFLH_Amount211", 0); //一级承运商运费（重量）合计
                //            for (int i = 0; i < entrycount; i++)
                //            {
                //                this.Model.SetValue("FPrice", 0, i);
                //                this.Model.SetValue("FTransportAmount", 0, i);
                //            }

                //        }

                //    }
                //}

            }
            #endregion

            #region //异常扣款、标准送货费、增值服务费、装卸费、始发地
            if (e.Field.Key == "F_KFLH_Amount" || e.Field.Key == "F_KFLH_Amount1" || e.Field.Key == "F_KFLH_Amount211" || e.Field.Key == "F_KFLH_Amount221" || e.Field.Key == "F_KFLH_Amount2" || e.Field.Key == "F_KFLH_Amount21")
            {
                
                decimal totalfreight = 0m;
                if (this.Model.GetValue("F_KFLH_Amount21") != null) //一级承运商运费（立方）合计
                {
                    totalfreight = totalfreight + decimal.Parse(this.Model.GetValue("F_KFLH_Amount21").ToString());//一级承运商运费（重量）合计
                }
                if (this.Model.GetValue("F_KFLH_Amount211") != null) //一级承运商运费（立方）合计
                {
                    totalfreight = totalfreight + decimal.Parse(this.Model.GetValue("F_KFLH_Amount211").ToString());//一级承运商运费（重量）合计
                }
                this.Model.SetValue("F_KFLH_Amount22", totalfreight); //运费小计
                if (this.Model.GetValue("F_KFLH_Amount") != null)
                {
                    decimal deductions = decimal.Parse(this.Model.GetValue("F_KFLH_Amount").ToString());//异常扣款
                    totalfreight = totalfreight - deductions;
                }
                if (this.Model.GetValue("F_KFLH_Amount1") != null)
                {
                    decimal delivery = decimal.Parse(this.Model.GetValue("F_KFLH_Amount1").ToString());//标准送货费
                    totalfreight = totalfreight + delivery;
                }
                if (this.Model.GetValue("F_KFLH_Amount221") != null)
                {
                    decimal service = decimal.Parse(this.Model.GetValue("F_KFLH_Amount221").ToString());//增值服务费
                    totalfreight = totalfreight + service;
                }
                if (this.Model.GetValue("F_KFLH_Amount2") != null)
                {
                    decimal handling = decimal.Parse(this.Model.GetValue("F_KFLH_Amount2").ToString());//装卸费
                    totalfreight = totalfreight + handling;
                }

                if (totalfreight > 0m)
                {
                    this.Model.SetValue("F_KFLH_Amount2211", totalfreight); //总运费
                }
                setpricenumber(totalfreight);
            }
            #endregion


        }
        private void setpricenumber(decimal totalfreight)
        {
            
            if (this.Model.GetValue("FTDeliveryStatus") != null)
            {
                var entrycount = this.View.Model.GetEntryRowCount("F_SJWE_Entity");
                string tdelivery = this.Model.GetValue("FTDeliveryStatus").ToString();
                one = true;
                //if (tdelivery == "1" || tdelivery == "2" || tdelivery == "5888")
                //{
                //    this.Model.SetValue("F_KFLH_Amount21", 0); //一级承运商运费（立方）合计
                //    this.Model.SetValue("F_KFLH_Amount211", 0); //一级承运商运费（重量）合计
                //}
                
               
                string JHFS = this.View.Model.GetValue("FTDeliveryStatus").ToString();
                if (JHFS == "5888" || JHFS == "1" || JHFS == "2")
                {
                    decimal allqty = decimal.Parse(this.Model.GetValue("F_KFLH_QTY11").ToString());//总商品数量
                    decimal LFCYS = decimal.Parse(this.Model.GetValue("F_KFLH_Amount21").ToString());
                    for (int i = 0; i < entrycount; i++)
                    {
                        decimal JJPrice = 0;
                        decimal pricenumber = 0;
                        if (allqty > 0)
                        {
                            JJPrice = totalfreight / allqty;//承运商件数承运单价
                        }
                        decimal count = decimal.Parse(this.Model.GetValue("FRealQty",i).ToString());//数量
                        string strTmp = string.Format("{0:0.##}", JJPrice);
                        
                        pricenumber = decimal.Parse(strTmp);
                        this.Model.SetValue("FPrice", 0, i);
                        this.Model.SetValue("F_PRICE_NUMBER", pricenumber, i);
                        this.Model.SetValue("FTransportAmount", string.Format("{0:0.##}", JJPrice * count), i);
                    }
                }
                else
                {
                    decimal price = 0m;
                    decimal allcubic = decimal.Parse(this.Model.GetValue("F_KFLH_QTY").ToString());//总立方数
                    decimal alleight = decimal.Parse(this.Model.GetValue("F_KFLH_Qty1").ToString());//总重量数

                    if (allcubic > 0)
                    {
                        price = totalfreight / allcubic;
                    }
                    else if (alleight > 0)
                    {
                        price = totalfreight / alleight;
                    }
                    for (int i = 0; i < entrycount; i++)
                    {
                        decimal amount = 0;
                        decimal pricenumber = 0;
                        if (allcubic > 0)
                        {
                            amount = decimal.Parse(this.Model.GetValue("Fcubicamount", i).ToString()) * price;//一级承运商运费（立方）金额
                        }
                        else if (alleight > 0)
                        {
                            amount = decimal.Parse(this.Model.GetValue("Fweightamount", i).ToString()) * price;//一级承运商运费（重量）金额
                        }
                        decimal count = decimal.Parse(this.Model.GetValue("F_KFLH_QTY11").ToString());//数量
                        pricenumber = amount / count;
                        string strTmp = string.Format("{0:0.##}", pricenumber);
                        pricenumber = decimal.Parse(strTmp);
                        this.Model.SetValue("FPrice", price, i);
                        this.Model.SetValue("F_PRICE_NUMBER", pricenumber, i);
                        this.Model.SetValue("FTransportAmount", amount, i);
                    }
                }
                

            }

        }
        private decimal getvolume()
        {
            decimal volume = 0m;
            var entrycount = this.View.Model.GetEntryRowCount("F_SJWE_Entity");
            for (int i = 0; i < entrycount; i++)
            {
                if (this.Model.GetValue("FMaterialID", i) != null && this.Model.GetValue("FRealQty", i) != null)
                {
                    decimal realqty = decimal.Parse(this.Model.GetValue("FRealQty", i).ToString());//数量
                    DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                    if (fmaterialid == null) break;
                    string materialid = fmaterialid["Id"].ToString();//物料ID
                    string masql = string.Format(@"/*dialect*/select a.FMATERIALID, a.F_ASDF_COMBO,b.FGROSSWEIGHT,c.FNAME,b.FVOLUME  from  T_BD_MATERIAL  a
                         left join  t_BD_MaterialBase b  
                          on a.FMASTERID=b.FMATERIALID
                          left join T_BD_MATERIAL_L c on a.FMATERIALID=c.FMATERIALID
                          where  a.FMATERIALID=" + materialid + "");
                    DataSet mads = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, masql);
                    int billing = 0;//计费方式
                    if (mads == null || mads.Tables[0].Rows.Count == 0)
                    {
                        break;
                    }
                    if (mads.Tables[0].Rows[0]["F_ASDF_COMBO"] != null)
                    {
                        if (mads.Tables[0].Rows[0]["F_ASDF_COMBO"].ToString() != "")
                        {
                            billing = Convert.ToInt32(Math.Truncate(decimal.Parse(mads.Tables[0].Rows[0]["F_ASDF_COMBO"].ToString()))); ;
                        }
                    }
                    #region   标准方数、计费立方数 标准重量、计费重量

                    if (billing == 1)
                    {
                        decimal fvolume = decimal.Parse(mads.Tables[0].Rows[0]["FVOLUME"].ToString());

                        volume = volume + realqty * fvolume;

                    }
                    else if (billing == 2)
                    {
                        decimal grossweight = decimal.Parse(mads.Tables[0].Rows[0]["FGROSSWEIGHT"].ToString());
                        volume = volume + realqty * grossweight;
                    }
                    #endregion
                }
            }
            return volume;
        }
    }
}
