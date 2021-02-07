using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System.ComponentModel;
using System.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;

namespace RebateChanged
{
    [Description("返利资金池")]
    public class RebateChanged : AbstractBillPlugIn
    {

        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            if (this.View.OpenParameter.Status.Equals(OperationStatus.EDIT) || this.View.OpenParameter.Status.Equals(OperationStatus.ADDNEW))
            {
                string FStatus = this.View.Model.GetValue("FDocumentStatus").ToString();
                if (FStatus == "Z" || FStatus == "A" || FStatus == "D")
                {
                    var entrycount = this.View.Model.GetEntryRowCount("FSaleOrderEntry");
                    this.Model.SetValue("FRPAMOUNT", 0);
                    DynamicObject fcustid = this.Model.GetValue("FCustId", 0) as DynamicObject;
                    if (fcustid == null) return;
                    string custid = fcustid["Id"].ToString();
                    DynamicObject fsaleorgid = this.Model.GetValue("FSaleOrgId", 0) as DynamicObject;
                    if (fsaleorgid == null) return;
                    string saleorgid = fsaleorgid["Id"].ToString();
                    string sql = string.Format(@"/*dialect*/ select sum(rre.FRPAMOUNT)as FRPAMOUNT from T_RPM_RPSTATEMENT rr left join  T_RPM_RPSTATEMENTENTRY 
                rre on rr.FID=rre.FID 
                where rr.FFINANCEORGID='" + saleorgid + "' and rre.FRPOBJECTID ='" + custid + "'");
                    DataSet dsX = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sql);
                    decimal rpamount = 0;
                    if (dsX == null || dsX.Tables[0].Rows.Count == 0)
                    {
                        return;
                    }

                    this.View.GetControl("FCustId").Enabled = false;
                    this.View.GetControl("FSaleOrgId").Enabled = false;
                    if (dsX.Tables[0].Rows[0]["FRPAMOUNT"].ToString() != "")
                    {
                        rpamount = decimal.Parse(dsX.Tables[0].Rows[0]["FRPAMOUNT"].ToString());
                    }
                    this.Model.SetValue("FRPAMOUNT", rpamount);
                    decimal amountL = 0;//最大折扣额
                    decimal allamount = 0;//价税合计总额
                    #region 循环计算单据体最大折扣额
                    for (int i = 0; i < entrycount; i++)
                    {
                        decimal amountrow = decimal.Parse(this.Model.GetValue("FAllAmount", i).ToString());//价税合计
                        decimal decimalcou = decimal.Parse(this.Model.GetValue("FDecimal", i).ToString());//折让比例
                        allamount = allamount + amountrow;
                        decimal discount = amountrow * decimalcou;
                        amountL = amountL + discount;
                    }
                    #endregion
                    decimal alldiscount = decimal.Parse(this.Model.GetValue("FALLdiscount").ToString());//本次折扣使用总金额
                    if (alldiscount == 0)
                    {
                        this.View.GetControl("FSaleOrderEntry").Enabled = true;
                    }
                    else
                    {
                        this.View.GetControl("FSaleOrderEntry").Enabled = false;
                    }
                    if (alldiscount > rpamount)
                    {
                        this.View.GetMainBarItem("tbSplitSave").Visible = false;
                        this.View.GetMainBarItem("tbSplitSubmit").Visible = false;
                        this.View.ShowMessage("本次折扣使用总金额不能大于返利余额!", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                    }
                    if (amountL < alldiscount)
                    {
                        this.View.GetMainBarItem("tbSplitSave").Visible = false;
                        this.View.GetMainBarItem("tbSplitSubmit").Visible = false;
                        this.View.ShowMessage("本次折扣使用总金额不能超过此订单中所有SKU【最大可用折扣额度】的和。", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                    }
                }
            }
        }
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);

            if (e.Field.Key == "FALLdiscount")
            {
                ALLdiscount();
                #region  本次折扣使用总金额
                //this.View.GetMainBarItem("tbSplitSave").Visible = true;
                //this.View.GetMainBarItem("tbSplitSubmit").Visible = true;
                //if (this.View.Model.GetValue("FRPAMOUNT") == null) return;
                //decimal rpamount = decimal.Parse(this.Model.GetValue("FRPAMOUNT").ToString());
                //if (rpamount < 0) return;
                //if (this.View.Model.GetValue("FALLdiscount") == null) return;
                //decimal alldiscount = decimal.Parse(this.Model.GetValue("FALLdiscount").ToString());//本次折扣使用总金额
                //var entrycount = this.View.Model.GetEntryRowCount("FSaleOrderEntry");
                //decimal fallamountzr = decimal.Parse(this.Model.GetValue("FAllAmount_ZR", 0).ToString());//价税合计
                //decimal amountL = 0;//最大折扣额
                //decimal allamount = 0;//价税合计总额
                //#region 循环计算单据体最大折扣额
                //for (int i = 0; i < entrycount; i++)
                //{
                //    decimal amountrow = decimal.Parse(this.Model.GetValue("FAllAmount", i).ToString());//价税合计
                //    decimal decimalcou = decimal.Parse(this.Model.GetValue("FDecimal", i).ToString());//折让比例
                //    allamount = allamount + amountrow;
                //    decimal discount = amountrow * decimalcou;
                //    amountL = amountL + discount;
                //}
                //#endregion
                //if (fallamountzr>0) 
                //{
                //    this.View.ShowMessage("请将本次折扣使用总金额复位为0!", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                //    return;
                //}

                //if (alldiscount > rpamount)
                //{
                //    this.View.ShowMessage("本次折扣使用总金额不能大于返利余额!", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                //    this.View.GetMainBarItem("tbSplitSave").Visible = false;
                //    this.View.GetMainBarItem("tbSplitSubmit").Visible = false;
                //}
                //if (!this.View.GetControl("FSaleOrderEntry").Enabled) 
                //{
                //    this.View.ShowMessage("本次折扣使用总金额大于0!,如需修改本次折扣使用总金额,请将本次折扣使用总金额复位为0", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                //    return;
                //}
                //if (alldiscount == 0)
                //{
                //    this.View.GetControl("FSaleOrderEntry").Enabled = true;
                //    this.View.GetBarItem("ToolBar", "tbDeleteLine").Visible = true;
                //}
                //if (amountL < alldiscount)
                //{
                //    this.View.ShowMessage("本次折扣使用总金额不能超过此订单中所有SKU【最大可用折扣额度】的和。", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                //    this.View.GetMainBarItem("tbSplitSave").Visible = false;
                //    this.View.GetMainBarItem("tbSplitSubmit").Visible = false;
                //}
                //for (int i = 0; i < entrycount; i++)
                //{
                //    decimal taxprice = decimal.Parse(this.View.Model.GetValue("FTaxPrice", i).ToString());//含税单价
                //    decimal price = decimal.Parse(this.View.Model.GetValue("FPrice", i).ToString());//单价
                //    decimal taxamount = decimal.Parse(this.View.Model.GetValue("FEntryTaxAmount", i).ToString());//税额
                //    decimal famount = decimal.Parse(this.View.Model.GetValue("FAmount", i).ToString());//金额 
                //    decimal allamountrow = decimal.Parse(this.Model.GetValue("FAllAmount", i).ToString());//价税合计
                //    decimal decimalzr = decimal.Parse(this.Model.GetValue("FDecimal", i).ToString());//折让比例
                //    decimal qty = decimal.Parse(this.View.Model.GetValue("FQty", i).ToString());//销售数量
                //    if (qty <= 0) break;
                //    decimal entrytaxrate = decimal.Parse(this.View.Model.GetValue("FEntryTaxRate", i).ToString());  //税率

                //    decimal ftaxprice_zr = decimal.Parse(this.View.Model.GetValue("FTaxPrice_ZR", i).ToString());//含税单价
                //    decimal fprice_zr = decimal.Parse(this.View.Model.GetValue("FPrice_ZR", i).ToString());//单价
                //    decimal ftaxamount_zr = decimal.Parse(this.View.Model.GetValue("FTaxAmount_ZR", i).ToString());//税额
                //    decimal famount_zr = decimal.Parse(this.View.Model.GetValue("FAmount_ZR", i).ToString());//金额 
                //    decimal fallamount_zr = decimal.Parse(this.Model.GetValue("FAllAmount_ZR", i).ToString());//价税合计
                //    if (alldiscount <= 0 && ftaxprice_zr > 0 && fprice_zr > 0 && ftaxamount_zr > 0 && famount_zr > 0 && fallamount_zr > 0)
                //    {
                //        this.Model.SetValue("FTaxPrice", ftaxprice_zr, i);//折让含税单价
                //        this.Model.SetValue("FAllAmount", fallamount_zr, i);  //折让前价税合计
                //        this.Model.SetValue("FAmount", famount_zr, i);//折让前金额
                //        this.Model.SetValue("FEntryTaxAmount", ftaxamount_zr, i);//折让前税额
                //        this.Model.SetValue("FPrice", fprice_zr, i);//折让前单价 
                //        this.Model.SetValue("FDiscount",0, i);//折扣额

                //        this.Model.SetValue("FTaxPrice_ZR", 0, i);//折让含税单价
                //        this.Model.SetValue("FTaxAmount_ZR", 0, i);  //折让前价税合计
                //        this.Model.SetValue("FAmount_ZR", 0, i);//折让前金额
                //        this.Model.SetValue("FTaxAmount_ZR", 0, i);//折让前税额
                //        this.Model.SetValue("FPrice_ZR", 0, i);//折让前单价 
                //    }
                //    if (alldiscount > 0)
                //    {
                //        this.Model.SetValue("FTaxPrice_ZR", taxprice, i);//折让含税单价
                //        this.Model.SetValue("FAllAmount_ZR", allamountrow, i);  //折让前价税合计
                //        this.Model.SetValue("FAmount_ZR", famount, i);//折让前金额
                //        this.Model.SetValue("FTaxAmount_ZR", taxamount, i);//折让前税额
                //        this.Model.SetValue("FPrice_ZR", price, i);//折让前单价 
                //        decimal dis = (decimal.Parse(this.Model.GetValue("FAllAmount", i).ToString()) / allamount) * alldiscount;
                //        string strTmp = dis.ToString("#0.00#");
                //        dis = decimal.Parse(strTmp);
                //        this.Model.SetValue("FDiscount", dis, i);//折扣额
                //        allamountrow = allamountrow - dis;//价税合计
                //        taxprice = allamountrow / qty;//含税单价
                //        price = taxprice - (taxprice * (entrytaxrate / 100));
                //        famount = price * qty;
                //        taxamount = allamountrow - famount;
                //        this.Model.SetValue("FTaxPrice", taxprice, i);//含税单价
                //        this.Model.SetValue("FAllAmount", allamountrow, i);  //价税合计
                //        this.Model.SetValue("FAmount", famount, i);//金额
                //        this.Model.SetValue("FEntryTaxAmount", taxamount, i);//税额
                //        this.Model.SetValue("FPrice", price, i);//单价 

                //    }
                //    this.View.GetControl("FSaleOrderEntry").Enabled = false;
                //    this.View.GetBarItem("ToolBar", "tbDeleteLine").Visible = false;
                //}
                #endregion
            }
            if (e.Field.Key == "FPriceUnitQty")
            {
                cargo();
            }
            var entrycount = this.View.Model.GetEntryRowCount("FSaleOrderEntry");
            if (entrycount > 0)
            {
                DynamicObject fcustida = this.Model.GetValue("FCustId") as DynamicObject;
                if (fcustida == null) return;
                string custida = fcustida["Id"].ToString();
                DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", 0) as DynamicObject;
                if (fmaterialid != null && custida != "")
                {
                    this.View.GetControl("FCustId").Enabled = false;
                    this.View.GetControl("FSaleOrgId").Enabled = false;
                }
                else
                {
                    this.View.GetControl("FCustId").Enabled = true;
                    this.View.GetControl("FSaleOrgId").Enabled = true;
                }
            }
            //获取返利余额
            if (e.Field.Key == "FCustId" || e.Field.Key == "FSaleOrgId")
            {

                DynamicObject fcustid = this.Model.GetValue("FCustId", e.Row) as DynamicObject;
                if (fcustid == null) return;
                string custid = fcustid["Id"].ToString();
                DynamicObject fsaleorgid = this.Model.GetValue("FSaleOrgId", e.Row) as DynamicObject;
                if (fsaleorgid == null) return;
                string saleorgid = fsaleorgid["Id"].ToString();
                string sql = string.Format(@"/*dialect*/ select sum(rre.FRPAMOUNT)as FRPAMOUNT from T_RPM_RPSTATEMENT rr left join  T_RPM_RPSTATEMENTENTRY 
                rre on rr.FID=rre.FID 
                where rr.FFINANCEORGID='" + saleorgid + "' and rre.FRPOBJECTID ='" + custid + "'");
                DataSet dsX = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sql);
                decimal rpamount = 0;
                if (dsX == null || dsX.Tables[0].Rows.Count == 0)
                {
                    return;
                }
                if (dsX.Tables[0].Rows[0]["FRPAMOUNT"].ToString() != "")
                {
                    rpamount = decimal.Parse(dsX.Tables[0].Rows[0]["FRPAMOUNT"].ToString());
                }
                this.Model.SetValue("FRPAMOUNT", rpamount);
            }
        }

        public override void EntryBarItemClick(BarItemClickEventArgs e)
        {
            base.EntryBarItemClick(e);
            int rowIndex = this.Model.GetEntryCurrentRowIndex("FSaleOrderEntry");
            decimal alldiscount = decimal.Parse(this.Model.GetValue("FALLdiscount").ToString());//本次折扣使用总金额
            if (e.BarItemKey.Equals("tbDeleteLine"))
            {
                if (alldiscount == 0)
                {
                    //逐行,删除单据体分录,0代表第1行
                    this.View.Model.DeleteEntryRow("FSaleOrderEntry", rowIndex);
                }
                DynamicObject fcustid = this.Model.GetValue("FCustId") as DynamicObject;
                if (fcustid == null) return;
                string custid = fcustid["Id"].ToString();
                DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", 0) as DynamicObject;
                if (fmaterialid != null && custid != "")
                {
                    this.View.GetControl("FCustId").Enabled = false;
                    this.View.GetControl("FSaleOrgId").Enabled = false;
                }
                else
                {
                    this.View.GetControl("FCustId").Enabled = true;
                    this.View.GetControl("FSaleOrgId").Enabled = true;
                }
                //刷新,单据体界面
                this.View.UpdateView("FSaleOrderEntry");
            }
        }
        private void ALLdiscount()
        {
            var entrycount = this.View.Model.GetEntryRowCount("FSaleOrderEntry");

            this.View.GetMainBarItem("tbSplitSave").Visible = true;
            this.View.GetMainBarItem("tbSplitSubmit").Visible = true;
            if (this.View.Model.GetValue("FRPAMOUNT") == null) return;
            decimal rpamount = decimal.Parse(this.Model.GetValue("FRPAMOUNT").ToString());
            if (rpamount < 0) return;
            if (this.View.Model.GetValue("FALLdiscount") == null) return;
            decimal alldiscount = decimal.Parse(this.Model.GetValue("FALLdiscount").ToString());//本次折扣使用总金额
            decimal fallamountzr = decimal.Parse(this.Model.GetValue("FAllAmount_ZR", 0).ToString());//价税合计
            decimal amountL = 0;//最大折扣额
            decimal allamount = 0;//价税合计总额

            #region 循环计算单据体最大折扣额
            for (int i = 0; i < entrycount; i++)
            {
                decimal amountrow = decimal.Parse(this.Model.GetValue("FAllAmount", i).ToString());//价税合计
                decimal decimalcou = decimal.Parse(this.Model.GetValue("FDecimal", i).ToString());//折扣率
                if (decimalcou > 0)
                {
                    allamount = allamount + amountrow;
                    decimal discount = amountrow * decimalcou;
                    amountL = amountL + discount;
                }

            }
            #endregion
            if (alldiscount > rpamount)
            {

                this.View.GetMainBarItem("tbSplitSave").Visible = false;
                this.View.GetMainBarItem("tbSplitSubmit").Visible = false;
                this.View.ShowMessage("本次折扣使用总金额不能大于返利余额!", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
            }
            if (fallamountzr > 0 && alldiscount > 0)
            {
                this.View.GetMainBarItem("tbSplitSave").Visible = false;
                this.View.GetMainBarItem("tbSplitSubmit").Visible = false;
                this.View.ShowMessage("如需修改请将本次折扣使用总金额,请将本次折扣使用总金额复位为0", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                return;
            }
            if (this.View.GetControl("FSaleOrderEntry").Enabled == false)//单据体被封住时
            {
                this.View.ShowMessage("本次折扣使用总金额大于0!,如需修改请将本次折扣使用总金额,请将本次折扣使用总金额复位为0", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
                return;
            }
            if (alldiscount == 0)
            {
                this.View.GetControl("FSaleOrderEntry").Enabled = true;
            }
            if (amountL < alldiscount)
            {
                this.View.GetMainBarItem("tbSplitSave").Visible = false;
                this.View.GetMainBarItem("tbSplitSubmit").Visible = false;
                this.View.ShowMessage("本次折扣使用总金额不能超过此订单中所有SKU【最大可用折扣额度】的和。", Kingdee.BOS.Core.DynamicForm.MessageBoxType.Error);
            }
            for (int i = 0; i < entrycount; i++)
            {
                decimal taxprice = decimal.Parse(this.View.Model.GetValue("FTaxPrice", i).ToString());//含税单价
                decimal price = decimal.Parse(this.View.Model.GetValue("FPrice", i).ToString());//单价
                decimal taxamount = decimal.Parse(this.View.Model.GetValue("FEntryTaxAmount", i).ToString());//税额
                decimal famount = decimal.Parse(this.View.Model.GetValue("FAmount", i).ToString());//金额 
                decimal allamountrow = decimal.Parse(this.Model.GetValue("FAllAmount", i).ToString());//价税合计
                decimal decimalzr = decimal.Parse(this.Model.GetValue("FDecimal", i).ToString());//折让比例
                decimal qty = decimal.Parse(this.View.Model.GetValue("FPriceUnitQty", i).ToString());//计价数量
                if (qty <= 0) break;
                decimal entrytaxrate = decimal.Parse(this.View.Model.GetValue("FEntryTaxRate", i).ToString());  //税率
                decimal ftaxprice_zr = decimal.Parse(this.View.Model.GetValue("FTaxPrice_ZR", i).ToString());//含税单价
                decimal fprice_zr = decimal.Parse(this.View.Model.GetValue("FPrice_ZR", i).ToString());//单价
                decimal ftaxamount_zr = decimal.Parse(this.View.Model.GetValue("FTaxAmount_ZR", i).ToString());//税额
                decimal famount_zr = decimal.Parse(this.View.Model.GetValue("FAmount_ZR", i).ToString());//金额 
                decimal fallamount_zr = decimal.Parse(this.Model.GetValue("FAllAmount_ZR", i).ToString());//价税合计
                if (alldiscount <= 0 && ftaxprice_zr > 0 && fprice_zr > 0 && ftaxamount_zr > 0 && famount_zr > 0 && fallamount_zr > 0)
                {
                    this.Model.SetValue("FTaxPrice", ftaxprice_zr, i);//折让含税单价
                    this.Model.SetValue("FAllAmount", fallamount_zr, i);  //折让前价税合计
                    this.Model.SetValue("FAmount", famount_zr, i);//折让前金额
                    this.Model.SetValue("FEntryTaxAmount", ftaxamount_zr, i);//折让前税额
                    this.Model.SetValue("FPrice", fprice_zr, i);//折让前单价 
                    this.Model.SetValue("FDiscount", 0, i);//折扣额

                    this.Model.SetValue("FTaxPrice_ZR", 0, i);//折让含税单价
                    this.Model.SetValue("FAllAmount_ZR", 0, i);  //折让前价税合计
                    this.Model.SetValue("FAmount_ZR", 0, i);//折让前金额
                    this.Model.SetValue("FTaxAmount_ZR", 0, i);//折让前税额
                    this.Model.SetValue("FPrice_ZR", 0, i);//折让前单价 

                }

                if (alldiscount > 0)
                {
                    decimal dis = 0;
                    this.Model.SetValue("FTaxPrice_ZR", taxprice, i);//折让含税单价
                    this.Model.SetValue("FAllAmount_ZR", allamountrow, i);  //折让前价税合计
                    this.Model.SetValue("FAmount_ZR", famount, i);//折让前金额
                    this.Model.SetValue("FTaxAmount_ZR", taxamount, i);//折让前税额
                    this.Model.SetValue("FPrice_ZR", price, i);//折让前单价 
                    if (decimalzr > 0)
                    {
                        //allamountrow 价税合计
                        //decimalzr 折扣率
                        //alldiscount 本次折扣使用总金额
                        //amountL 最大折扣额
                        decimal discount = allamountrow * decimalzr; //当前行最大折扣额
                        dis = alldiscount / amountL * discount;
                        //dis = (decimal.Parse(this.Model.GetValue("FAllAmount", i).ToString()) / allamount) * alldiscount;
                        string strTmp = string.Format("{0:0.##}", dis);
                        dis = decimal.Parse(strTmp);
                    }
                    this.Model.SetValue("FDiscount", dis, i);//折扣额

                    allamountrow = allamountrow - dis;//价税合计
                    famount = allamountrow / ((entrytaxrate+100)/100);//金额
                    taxprice = allamountrow / qty;//含税单价
                    price = famount / qty;//单价
                    taxamount = allamountrow - famount;//税额
                    this.Model.SetValue("FTaxPrice", taxprice, i);//含税单价
                    this.Model.SetValue("FAllAmount", allamountrow, i);  //价税合计
                    this.Model.SetValue("FAmount", famount, i);//金额
                    this.Model.SetValue("FEntryTaxAmount", taxamount, i);//税额
                    this.Model.SetValue("FPrice", price, i);//单价 
                    this.Model.SetValue("FDiscountRate", dis / (allamountrow+dis), i);//折扣率
                    this.View.GetControl("FSaleOrderEntry").Enabled = false;
                }

            }
        }

        private void cargo()
        {
            var entrycount = this.View.Model.GetEntryRowCount("FSaleOrderEntry");
            var volume = 0m;
            for (int i = 0; i < entrycount; i++)
            {
                if (this.Model.GetValue("FMaterialID", i) != null)
                {
                    DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                    if (fmaterialid == null) break;
                    string materialid = fmaterialid["Id"].ToString();//物料ID
                    decimal qty = decimal.Parse(this.View.Model.GetValue("FSTOCKQTY", i).ToString());//计价数量
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
