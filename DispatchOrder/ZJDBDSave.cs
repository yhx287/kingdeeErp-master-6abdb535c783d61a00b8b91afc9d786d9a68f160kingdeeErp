using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using System.ComponentModel;
//引用Kingdee.BOS.App.Data,才能执行sql
using Kingdee.BOS.App.Data;
using System.Data;
using Kingdee.BOS.Orm.DataEntity;

namespace DispatchOrder
{
    [Description("直接调拨单保存的时候更新折扣额，总体积，总重量，单价等")]
    public class ZJDBDSave: AbstractBillPlugIn
    {
        public override void AfterSave(Kingdee.BOS.Core.Bill.PlugIn.Args.AfterSaveEventArgs e)
        {
            base.AfterSave(e);

            //如果保存成功,则触发
            if (e.OperationResult.IsSuccess)
            {
                decimal alldiscount = decimal.Parse(this.View.Model.GetValue("F_zzz_ZKXD").ToString());//本次折扣使用金额
                if (alldiscount > 0)
                {
                    string FID = this.View.Model.DataObject["Id"].ToString();
                    string sql = "/*dialect*/ select F_ZZZ_ZKL,F_zzz_ZQHSPrice,FMATERIALID,FENTRYID,FID from T_STK_STKTRANSFERINENTRY where FID=" + FID;
                    DataSet ds = DBUtils.ExecuteDataSet(this.Context, sql);
                    decimal allamountrow = 0;
                    decimal decimalzr = 0;
                    decimal dis = 0;
                    decimal amountL = 0;
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        decimal ZQHSPrice = decimal.Parse(ds.Tables[0].Rows[i]["F_zzz_ZQHSPrice"].ToString());
                        decimal qty = decimal.Parse(this.View.Model.GetValue("FPRICEQTY", i).ToString());//计价数量
                        decimal amountrow = ZQHSPrice * qty;//价税合计
                        decimal decimalcou = decimal.Parse(ds.Tables[0].Rows[i]["F_ZZZ_ZKL"].ToString());//折扣率
                        if (decimalcou > 0)
                        {
                            decimal discount = amountrow * decimalcou;
                            amountL = amountL + discount;
                        }

                    }

                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        decimal ZQHSPrice = decimal.Parse(ds.Tables[0].Rows[i]["F_zzz_ZQHSPrice"].ToString());
                        decimal qty = decimal.Parse(this.View.Model.GetValue("FPRICEQTY", i).ToString());
                        allamountrow = ZQHSPrice * qty;
                        decimalzr = decimal.Parse(ds.Tables[0].Rows[i]["F_ZZZ_ZKL"].ToString());
                        decimal discount = allamountrow * decimalzr;
                        dis = alldiscount / amountL * discount;

                        string strdis = string.Format("{0:0.##}", dis);
                        decimal entrytaxrate = decimal.Parse(this.View.Model.GetValue("FTaxRate", i).ToString());
                        allamountrow = allamountrow - dis;//价税合计
                        string strallamountrow = string.Format("{0:0.##}", allamountrow);
                        decimal famount = allamountrow / ((entrytaxrate + 100) / 100);//金额
                        string strfamount = string.Format("{0:0.##}", famount);
                        decimal taxprice = allamountrow / qty;//含税单价
                        string strtaxprice = string.Format("{0:0.######}", taxprice);
                        decimal price = famount / qty;//单价
                        string strprice = string.Format("{0:0.######}", price);
                        decimal taxamount = allamountrow - famount;//税额
                        string strtaxamount = string.Format("{0:0.##}", taxamount);
                        decimal ZKL = dis / (allamountrow + dis);
                        string strZKL = string.Format("{0:0.##}", ZKL);

                        decimal volume = 0m;
                        decimal weight = 0m;
                        if (this.Model.GetValue("FMaterialID", i) != null)
                        {
                            DynamicObject fmaterialid = this.Model.GetValue("FMaterialID", i) as DynamicObject;
                            if (fmaterialid == null) break;
                            string materialid = fmaterialid["Id"].ToString();//物料ID
                                                                             //decimal qty = decimal.Parse(this.View.Model.GetValue("FSTOCKQTY", i).ToString());//计价数量
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
                            decimal qtyST = decimal.Parse(this.View.Model.GetValue("FQTY", i).ToString());
                            decimal fvolume = decimal.Parse(mads.Tables[0].Rows[0]["FVOLUME"].ToString());
                            volume = qtyST * fvolume;
                            //this.Model.SetValue("F_asdf_Qty1", qty * fvolume, i);//行总体积
                            decimal grossweight = decimal.Parse(mads.Tables[0].Rows[0]["FGROSSWEIGHT"].ToString());
                            weight = qtyST * grossweight;
                            //this.Model.SetValue("F_asdf_Qty", qty * grossweight, i);//行总重量

                            #endregion

                        }
                        string sqlDetail = "/*dialect*/ update T_STK_STKTRANSFERINENTRY set F_ASDF_QTY3=" + volume
                            + ",F_ASDF_QTY2=" + weight + " where FID=" + FID + " and FEntryID=" + ds.Tables[0].Rows[i]["FEntryId"].ToString();

                        string sqlDetailF = "/*dialect*/ update T_STK_STKTRANSFERINENTRY_T set FDISCOUNT=" +decimal.Parse(strdis) + ",FALLAMOUNT=" + decimal.Parse(strallamountrow) + ",FTAXAMOUNT=" + decimal.Parse(strtaxamount)
                            + ",FCONSIGNPRICE=" + decimal.Parse(strprice) + ",FTAXPRICE=" + decimal.Parse(strtaxprice) + ",FCONSIGNAMOUNT=" + decimal.Parse(strfamount) + ",FDISCOUNTRATE=" + decimal.Parse(strZKL) + " where FID=" + FID + " and FEntryID=" + ds.Tables[0].Rows[i]["FEntryId"].ToString();
                        DBUtils.Execute(this.Context, sqlDetail);
                        DBUtils.Execute(this.Context, sqlDetailF);

                    }
                }
            }
        }
    }
}
