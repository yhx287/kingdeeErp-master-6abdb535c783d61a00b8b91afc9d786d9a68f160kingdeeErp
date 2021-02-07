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
using Kingdee.BOS.JSON;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.WebApi.Client;
using Newtonsoft.Json.Linq;
using System.Web;

namespace RebateChanged
{
    public class RebateAuditReturn : AbstractOperationServicePlugIn
    {
        K3CloudApiClient client;
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            string billno = string.Empty;
            List<string> sqlArray = new List<string>();
            foreach (DynamicObject entity in e.DataEntitys)
            {
                if (entity != null)
                {
                    //单据头
                    billno = Convert.ToString(entity["BillNo"]);

                    if (billno != "")
                    {
                        if (LogIn() == 1)
                        {
                            var ViewResult = client.View("RPM_RPStatements", "{\"CreateOrgId\":0,\"Number\":\"" + billno + "\",\"Id\":\"\"}");
                            JObject ViResult = JObject.Parse(ViewResult);
                            JObject viresult = ViResult["Result"]["ResponseStatus"] as JObject;
                            if (viresult["IsSuccess"].ToString() == "True")
                            {
                                var UnAuditResult = client.UnAudit("RPM_RPStatements", "{\"CreateOrgId\":0,\"Numbers\":[\"" + billno + "\"],\"Ids\":\"\",\"NetworkCtrl\":\"\"}");
                                JObject UnResult = JObject.Parse(UnAuditResult);
                                JObject unresult = UnResult["Result"]["ResponseStatus"] as JObject;
                                if (unresult["IsSuccess"].ToString() == "True")
                                {
                                    var DeleteResult = client.Delete("RPM_RPStatements", "{\"CreateOrgId\":0,\"Numbers\":[\"" + billno + "\"],\"Ids\":\"\",\"NetworkCtrl\":\"\"}");
                                    JObject DeResult = JObject.Parse(DeleteResult);
                                    JObject deresult = DeResult["Result"]["ResponseStatus"] as JObject;
                                    if (deresult["IsSuccess"].ToString() == "True")
                                    {

                                    }
                                    else
                                    {
                                        var SubmitResult = client.Submit("RPM_RPStatements", "{\"CreateOrgId\":0,\"Numbers\":[\"" + billno + "\"],\"Ids\":\"\",\"SelectedPostId\":0,\"NetworkCtrl\":\"\"}");
                                        var AuditResult = client.Audit("RPM_RPStatements", "{\"CreateOrgId\":0,\"Numbers\":[\"" + billno + "\"],\"Ids\":\"\",\"SelectedPostId\":0,\"NetworkCtrl\":\"\"}");
                                        throw new KDBusinessException("", "反审核失败,终止操作执行!" + deresult["Errors"].ToString() + "");
                                    }
                                }
                                else
                                {
                                    throw new KDBusinessException("", "反审核失败,终止操作执行!" + unresult["Errors"].ToString() + "");
                                }
                            }

                        }
                        //获取FID
                        //string ssql = string.Format(@"/*dialect*/ select FID from T_RPM_RPSTATEMENT  where FBILLNO='" + billno + "'");
                        //DataSet dsX = Kingdee.BOS.ServiceHelper.DBServiceHelper.ExecuteDataSet(this.Context, ssql);
                        //if (dsX == null || dsX.Tables[0].Rows.Count == 0)
                        //    return;
                        //int fid = int.Parse(dsX.Tables[0].Rows[0]["FID"].ToString());
                        //string sql = string.Format(@"/*dialect*/  DELETE FROM T_RPM_RPSTATEMENT WHERE FID = {0}", fid);
                        //sqlArray.Add(sql);
                        //string sqll = string.Format(@" DELETE FROM   T_RPM_RPSTATEMENTENTRY where FID={0}"
                        //    , fid);
                        //sqlArray.Add(sqll);

                    }
                }
            }
            //if (sqlArray.Count > 0)
            //{
            //    DBUtils.ExecuteBatch(this.Context, sqlArray, 100);
            //}
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
    }
}
