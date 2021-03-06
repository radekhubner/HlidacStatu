﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HlidacStatu.Lib.ES
{
    public class SearchData<T> : Lib.Data.Search.ISearchResult
        where T : class 
    {
        public virtual int DefaultPageSize() { return 40; }
        public virtual int MaxResultWindow() { return  Lib.ES.SearchTools.MaxResultWindow; }



        public int Page { get; set; }
        public int PageSize { get; set; } 
        public string Q { get; set; }
        public string OrigQuery { get; set; }
        public bool IsValid { get; set; }
        public long Total { get; set; }
        public virtual bool HasResult { get { return IsValid && Total > 0; } }
        public string DataSource { get; set; }

        private string _order = "0";
        public string Order
        {
            get {
                return _order;
            }

            set {
                this._order = value;
                if (OrderList == null)
                    InitOrderList();
                if (OrderList != null && OrderList.Count > 0)
                {
                    foreach (var item in OrderList)
                    {
                        if (item.Value == this._order.ToString())
                            item.Selected = true;
                        else
                            item.Selected = false;
                    }
                }
                         
               
            }
        }

        public List<System.Web.Mvc.SelectListItem> OrderList { get; set; } = null;
        public Func<T, string> AdditionalRender = null;

        public Nest.ISearchResponse<T> ElasticResults { get; set; }
        public TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero;

        protected Func<List<System.Web.Mvc.SelectListItem>> orderFill = null;
        protected static Func<List<System.Web.Mvc.SelectListItem>> getSmlouvyOrderList = () =>
        {
            return
                Devmasters.Core.Enums.EnumToEnumerable(typeof(HlidacStatu.Lib.ES.SearchTools.OrderResult)).Select(
                    m => new System.Web.Mvc.SelectListItem() { Value = m.Value, Text = "Řadit " + m.Key }
                    ).ToList();
        };

        protected static Func<List<System.Web.Mvc.SelectListItem>> emptyOrderList = () =>
        {
            return new List<System.Web.Mvc.SelectListItem>();
        };

        public SearchData()
                : this(emptyOrderList)
        { }

        public SearchData(Func<List<System.Web.Mvc.SelectListItem>> createdOrderList)
        {
            if (createdOrderList == null)
                createdOrderList = emptyOrderList;
            this.orderFill = createdOrderList;
            InitOrderList();
            this.PageSize = DefaultPageSize();

        }

        public void InitOrderList()
        {
            this.OrderList = this.orderFill();
        }
    }
}
