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


namespace RebateChanged
{
    public class RebateCheck : AbstractOperationServicePlugIn
    {

        //OnPreparePropertys 数据加载前，确保需要的属性被加载
        //因为需要读取采购员信息,先必须加载

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);

        }
        //OnAddValidators操作执行前，加载操作校验器
        public override void OnAddValidators(AddValidatorsEventArgs e)
        {
            base.OnAddValidators(e);
        }



        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            decimal allamount = 0;
            foreach (ExtendedDataEntity extended in e.SelectedRows)
            {
                DynamicObject dy = extended.DataEntity;

                DynamicObjectCollection docPriceEntity = dy["SaleOrderEntry"] as DynamicObjectCollection;

                foreach (DynamicObject HHentity in docPriceEntity)
                {
                    if (HHentity["FAllAmount_ZR"] != null)
                    {
                        decimal amount = decimal.Parse(Convert.ToString(HHentity["FAllAmount_ZR"]));
                        allamount = allamount + amount;
                    }
                }

                if (dy["FALLdiscount"] != null)
                {
                    decimal discount = decimal.Parse(dy["FALLdiscount"].ToString());
                    decimal rpamount = decimal.Parse(dy["FRPAMOUNT"].ToString());
                    if (rpamount < discount)
                    {
                        //e.CancelMessage = "本次折扣使用总金额不能大于返利余额,终止操作执行!";
                        throw new KDBusinessException("", "本次折扣使用总金额不能大于返利余额,终止操作执行!");
                        //e.Cancel = true;
                    }
                    else
                    {
                        //decimal amountL = allamount * (decimal)0.4;

                        //if (amountL < discount)
                        //{
                        //    throw new KDBusinessException("", "本次折扣使用总金额不能超过此订单总金额百分之40,终止操作执行!");
                        //}
                    }

                }

            }

        }

    }
}




