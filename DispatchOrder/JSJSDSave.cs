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
using Kingdee.BOS.Core.Metadata;

namespace DispatchOrder
{
    [Description("寄售结算单保存的时候更新折扣额，总体积，总重量，单价等")]
    public class JSJSDSave: AbstractBillPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            if (this.View.OpenParameter.Status.Equals(OperationStatus.ADDNEW))
            {
                //decimal alldiscount = decimal.Parse(this.View.Model.GetValue("alldiscount").ToString());
                //if (alldiscount > 0)
                //{

                //}
            }
        }
    }
}
