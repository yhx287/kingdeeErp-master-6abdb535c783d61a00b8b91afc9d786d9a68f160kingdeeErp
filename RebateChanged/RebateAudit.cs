using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

namespace RebateChanged
{
    public class RebateAudit : AbstractOperationServicePlugIn
    {
        K3CloudApiClient client;
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            e.FieldKeys.Add("FALLdiscount");
            e.FieldKeys.Add("SaleOrgId");//结算单位
            e.FieldKeys.Add("CustId");
            e.FieldKeys.Add("SaleDeptId");
            e.FieldKeys.Add("SettleCurrId");
            e.FieldKeys.Add("SalerId");


        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {

            string billno = string.Empty;
            string saleorgnumber = "";
            string custnumber = "";
            string auserid = "";
            string settlecurrnumber = "";
            string salernumber = "";
            decimal alldiscount = 0;
            int saleorgid = 0;
            int custid = 0;
            List<string> sqlArray = new List<string>();
            foreach (DynamicObject entity in e.DataEntitys)
            {
                if (entity != null)
                {
                    //单据头
                    billno = Convert.ToString(entity["BillNo"]);
                    DynamicObject CustId = entity["CustId"] as DynamicObject;
                    if (CustId != null)
                    {
                        if (CustId["Id"] != null)
                        {
                            custid = Convert.ToInt32(CustId["Id"]);
                        }
                        DynamicObject TRADINGCURRID = CustId["TRADINGCURRID"] as DynamicObject;
                        if (TRADINGCURRID != null)
                        {
                            if (TRADINGCURRID["Number"] != null)
                            {
                                settlecurrnumber = Convert.ToString(TRADINGCURRID["Number"]);
                            }
                        }
                        if (CustId["Number"] != null)
                        {
                            custnumber = Convert.ToString(CustId["Number"]);
                        }
                    }

                    DynamicObject SaleOrgId = entity["SaleOrgId"] as DynamicObject;
                    if (SaleOrgId != null)
                    {
                        if (SaleOrgId["Number"] != null)
                        {
                            saleorgnumber = Convert.ToString(SaleOrgId["Number"]);
                        }
                        if (SaleOrgId["Id"] != null)
                        {
                            saleorgid = Convert.ToInt32(SaleOrgId["Id"]);
                        }
                    }
                    DynamicObject SalerId = entity["SalerId"] as DynamicObject;
                    if (SalerId != null)
                    {
                        if (SalerId["Number"] != null)
                        {
                            salernumber = Convert.ToString(SalerId["Number"]);
                        }
                    }
                    DynamicObject ApproverId = entity["ApproverId"] as DynamicObject;
                    if (ApproverId != null)
                    {
                        if (ApproverId["Id"] != null)
                        {
                            auserid = Convert.ToString(ApproverId["Id"]);
                        }
                    }
                    alldiscount = Convert.ToDecimal(entity["FALLdiscount"]);
                    if (alldiscount > 0)
                    {
                        string sqlc = string.Format(@"/*dialect*/ select sum(rre.FRPAMOUNT)as FRPAMOUNT from T_RPM_RPSTATEMENT rr left join  T_RPM_RPSTATEMENTENTRY 
                                        rre on rr.FID=rre.FID 
                                        where rr.FFINANCEORGID='" + saleorgid + "' and rre.FRPOBJECTID ='" + custid + "'");
                        DataSet dsC = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sqlc);
                        if (dsC == null || dsC.Tables[0].Rows.Count == 0)
                            return;
                        decimal rpamount = decimal.Parse(dsC.Tables[0].Rows[0]["FRPAMOUNT"].ToString());
                        if (rpamount < alldiscount)
                        {
                            throw new KDBusinessException("", "本次折扣使用总金额不能大于返利余额,终止操作执行!");
                        }
                        else
                        {
                            Save(billno, saleorgnumber, salernumber, settlecurrnumber, auserid, custnumber, alldiscount);
                        }
                    }
                    #region 手写sql
                    //                    string sqlc = string.Format(@"/*dialect*/ select sum(rre.FRPAMOUNT)as FRPAMOUNT from T_RPM_RPSTATEMENT rr left join  T_RPM_RPSTATEMENTENTRY 
                    //                    rre on rr.FID=rre.FID 
                    //                    where rr.FFINANCEORGID='" + saleorgid + "' and rre.FRPOBJECTID ='" + custid + "'");
                    //                    DataSet dsC = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, sqlc);
                    //                    if (dsC == null || dsC.Tables[0].Rows.Count == 0)
                    //                        return;
                    //                    decimal rpamount = decimal.Parse(dsC.Tables[0].Rows[0]["FRPAMOUNT"].ToString());
                    //                    if (rpamount < alldiscount)
                    //                    {
                    //                        throw new KDBusinessException("", "本次折扣使用总金额不能大于返利余额,终止操作执行!");
                    //                    }
                    //                    else
                    //                    {
                    //                        if (billno != "" && alldiscount > 0 && custid != "" && saleorgid != "")
                    //                        {
                    //                            //Save(billno, saleorgid, salerid, settlecurrid);
                    //                            //获取FID
                    //                            string ssql = string.Format(@"/*dialect*/ select top 1 FID from T_RPM_RPSTATEMENT order by FID desc");
                    //                            DataSet dsX = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, ssql);
                    //                            if (dsX == null || dsX.Tables[0].Rows.Count == 0)
                    //                                return;
                    //                            int fid = int.Parse(dsX.Tables[0].Rows[0]["FID"].ToString()) + 1;
                    //                            string sql = string.Format(@"INSERT INTO T_RPM_RPSTATEMENT
                    //                          (FID,FBILLNO,FDOCUMENTSTATUS, FFINANCEORGID,FDEPTID,FAPPUSERID,FSETTLECURRID,FBILLDATE,FCREATEDATE) 
                    //                          VALUES 
                    //                          ({0},'{1}','{2}',{3},{4},{5},{6},'{7}', '{8}') "
                    //                             , fid, billno, "C", saleorgid, saledeptid, salerid, settlecurrid,
                    //                             DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    //                            sqlArray.Add(sql);

                    //                            ssql = string.Format(@"/*dialect*/ select top 1 FENTRYID from T_RPM_RPSTATEMENTENTRY order by FENTRYID desc");
                    //                            DataSet dsF = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, ssql);
                    //                            if (dsF == null || dsF.Tables[0].Rows.Count == 0)
                    //                                return;
                    //                            int fentryid = int.Parse(dsF.Tables[0].Rows[0]["FENTRYID"].ToString()) + 1;
                    //                            string sqll = string.Format(@"INSERT INTO T_RPM_RPSTATEMENTENTRY 
                    //                          (FENTRYID,FID,FSEQ,FRPAMOUNT,FRPLOCALAMOUNT,FRPOBJECTID,FRPOBJECTTYPEID,FCUSTOMERID,FENTRYCLOSESTATUS,FORDERCUSTOMERID) 
                    //                           VALUES 
                    //                           ({0},{1},{2},{3},{4},{5},'{6}',{7},'{8}',{9})"
                    //                                , fentryid, fid, 1, -alldiscount, -alldiscount, custid, "BD_Customer", 0, "A", custid);
                    //                            sqlArray.Add(sqll);

                    //                        }
                    //                    }
                    //                }
                    //                if (sqlArray.Count > 0)
                    //                {
                    //                    DBUtils.ExecuteBatch(this.Context, sqlArray, 100);
                    //                }
                    #endregion
                }
            }
        }

        private int LogIn()
        {
            client = new K3CloudApiClient(GetCurApplicationUrl() + "/");
            var loginResult = client.LoginByAppSecret(this.Context.DBId, "Administrator",
                "210740_6+5NTbGL5OH5QaSu767O66UEQMTaRAmI",
                "86b093564ef4489095a24de6300fcb46", 2052);
            //var loginResult = client.ValidateLogin(this.Context.DBId, "Administrator", "888888", 2052);
            var resultType = JObject.Parse(loginResult)["LoginResultType"].Value<int>();
            return resultType;

        }
        public static String GetCurApplicationUrl()
        {
            String url = HttpContext.Current.Request.Url.IsDefaultPort
                ? HttpContext.Current.Request.Url.Host
                : string.Format("{0}:{1}", HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port.ToString());
            if (HttpContext.Current.Request.ApplicationPath != "/")
                return "http://" + url + HttpContext.Current.Request.ApplicationPath;
            else return "http://" + url;
        }

        private void Save(string billno, string saleorgid, string salerid, string settlecurrnumber, string auserid, string custnumber, decimal alldiscount)
        {
            bool PD = true;
            string Errors = String.Empty;
            if (LogIn() == 1)
            {
                //var a = "{\"IsDeleteEntry\":\"true\",\"IsVerifyBaseDataField\":\"false\",\"IsEntryBatchFill\":\"true\",\"ValidateFlag\":\"true\",\"NumberSearch\":\"true\",\"Model\":{\"FID\":0,\"FBillNo\":\"" + billno + "\",\"FBillDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FFinanceOrgId\":{\"FNumber\":\"" + saleorgid + "\"},\"FSettleCurrId\":{\"FNUMBER\":\"" + settlecurrnumber + "\"},\"FDeptId\":{\"FNUMBER\":\"\"},\"FAppUserId\":{\"FNUMBER\":\"\"},\"FCreatorId\":{\"FUserID\":\"" + auserid + "\"},\"FCreateDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FModifierId\":{\"FUserID\":\"" + auserid + "\"},\"FModifyDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FApproverId\":{\"FUserID\":\"" + auserid + "\"},\"FApproveDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FBaseSettleCurrID\":{\"FNUMBER\":\"0\"},\"FRateTypeID\":{\"FNUMBER\":\"0\"},\"FExchangeRate\":0,\"FBillSource\":0,\"FAccountID\":{\"FNUMBER\":\"\"},\"FIsSettlement\":\"false\",\"FIsExceptRpt\":\"false\",\"FCloserId\":{\"FUserID\":\"0\"},\"FCloseDate\":\"\",\"FEntity\":[{\"FEntryID\":0,\"FRPObjectTypeID\":\"BD_Customer\",\"FRPObjectID\":{\"FNumber\":\"" + custnumber + "\"},\"FOrderCustomerId\":{\"FNUMBER\":\"" + custnumber + "\"},\"FIsGroupRebate\":\"false\",\"FRPLocalAmount\":" + alldiscount + ",\"FRPAmount\":" + alldiscount + ",\"FGroupNumber\":0,\"FRPCycleID\":{\"FNUMBER\":\"\"},\"FAssistPropertyID\":{\"FASSISTPROPERTYID__FF100001\":{\"FNumber\":\"\"}},\"FRPBaseQty\":0}]}}";

                alldiscount = -alldiscount;
                var ViewResult = client.View("RPM_RPStatements", "{\"CreateOrgId\":0,\"Number\":\"" + billno + "\",\"Id\":\"\"}");
                JObject ViResult = JObject.Parse(ViewResult);
                JObject viresult = ViResult["Result"]["ResponseStatus"] as JObject;
                if (viresult["IsSuccess"].ToString() == "True")
                {
                    throw new KDBusinessException("", "返利结算单据编码已存在，保存失败,终止操作执行!");
                }
                else
                {
                    var SaveResult = client.Save("RPM_RPStatements", "{\"IsDeleteEntry\":\"true\",\"IsVerifyBaseDataField\":\"false\",\"IsEntryBatchFill\":\"true\",\"ValidateFlag\":\"true\",\"NumberSearch\":\"true\",\"Model\":{\"FID\":0,\"FBillNo\":\"" + billno + "\",\"FBillDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FFinanceOrgId\":{\"FNumber\":\"" + saleorgid + "\"},\"FSettleCurrId\":{\"FNUMBER\":\"" + settlecurrnumber + "\"},\"FDeptId\":{\"FNUMBER\":\"\"},\"FAppUserId\":{\"FNUMBER\":\"\"},\"FCreatorId\":{\"FUserID\":\"" + auserid + "\"},\"FCreateDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FModifierId\":{\"FUserID\":\"" + auserid + "\"},\"FModifyDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FApproverId\":{\"FUserID\":\"" + auserid + "\"},\"FApproveDate\":\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",\"FBaseSettleCurrID\":{\"FNUMBER\":\"0\"},\"FRateTypeID\":{\"FNUMBER\":\"0\"},\"FExchangeRate\":0,\"FBillSource\":0,\"FAccountID\":{\"FNUMBER\":\"\"},\"FIsSettlement\":\"false\",\"FIsExceptRpt\":\"false\",\"FCloserId\":{\"FUserID\":\"0\"},\"FCloseDate\":\"\",\"FEntity\":[{\"FEntryID\":0,\"FRPObjectTypeID\":\"BD_Customer\",\"FRPObjectID\":{\"FNumber\":\"" + custnumber + "\"},\"FOrderCustomerId\":{\"FNUMBER\":\"" + custnumber + "\"},\"FIsGroupRebate\":\"false\",\"FRPLocalAmount\":" + alldiscount + ",\"FRPAmount\":" + alldiscount + ",\"FGroupNumber\":0,\"FRPCycleID\":{\"FNUMBER\":\"\"},\"FAssistPropertyID\":{\"FASSISTPROPERTYID__FF100001\":{\"FNumber\":\"\"}},\"FRPBaseQty\":0}]}}");
                    JObject SResult = JObject.Parse(SaveResult);
                    JObject sresult = SResult["Result"]["ResponseStatus"] as JObject;
                    if (sresult["IsSuccess"].ToString() == "True")
                    {
                        var SubmitResult = client.Submit("RPM_RPStatements", "{\"CreateOrgId\":0,\"Numbers\":[\"" + billno + "\"],\"Ids\":\"\",\"SelectedPostId\":0,\"NetworkCtrl\":\"\"}");
                        JObject SuResult = JObject.Parse(SubmitResult);
                        JObject suresult = SuResult["Result"]["ResponseStatus"] as JObject;
                        if (suresult["IsSuccess"].ToString() == "True")
                        {
                            var AuditResult = client.Audit("RPM_RPStatements", "{\"CreateOrgId\":0,\"Numbers\":[\"" + billno + "\"],\"Ids\":\"\",\"SelectedPostId\":0,\"NetworkCtrl\":\"\"}");
                            JObject AuResult = JObject.Parse(AuditResult);
                            JObject auresult = AuResult["Result"]["ResponseStatus"] as JObject;
                            if (auresult["IsSuccess"].ToString() == "True")
                            {

                            }
                            else
                            {
                                PD = false;
                                Errors = suresult["Errors"].ToString();
                            }
                        }
                        else
                        {
                            PD = false;
                            Errors = suresult["Errors"].ToString();
                        }
                    }
                    else
                    {
                        Errors = sresult["Errors"].ToString();
                        throw new KDBusinessException("", "保存失败,终止操作执行!" + Errors + "");
                    }
                    if (!PD)
                    {
                        var DeleteResult = client.Delete("RPM_RPStatements", "{\"CreateOrgId\":0,\"Numbers\":[\"" + billno + "\"],\"Ids\":\"\",\"NetworkCtrl\":\"\"}");
                        JObject DeResult = JObject.Parse(DeleteResult);
                        JObject deresult = DeResult["Result"]["ResponseStatus"] as JObject;
                        if (deresult["IsSuccess"].ToString() == "True")
                        {

                        }
                        throw new KDBusinessException("", "保存失败,终止操作执行!" + Errors + "");
                    }
                }
            }
        }
    }
}

