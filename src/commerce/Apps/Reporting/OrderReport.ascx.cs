using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Manager.Apps.Reporting;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Search;
using Mediachase.Commerce.Storage;
using Mediachase.Data.Provider;
using Mediachase.Web.Console.Common;
using Mediachase.Web.Console.Reporting.DataSources;
using Microsoft.Reporting.WebForms;
using OxxCommerceStarterKit.Commerce.Apps.Reporting.Datasets;
using Resources;

namespace OxxCommerceStarterKit.Commerce.Apps.Reporting
{
    public partial class OrderReport : ReportingBaseClass
    {
        protected override void OnInit(EventArgs e)
        {
            this.MarketFilter.SelectedIndexChanged += new EventHandler(this.MarketFilter_SelectedIndexChanged);
            this.CurrencyFilter.SelectedIndexChanged += new EventHandler(this.CurrencyFilter_SelectedIndexChanged);
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;
            this.StartDate.Value = DateTime.Now.AddMonths(-1);
            this.EndDate.Value = DateTime.Now;
            this.EndDate.Value = this.EndDate.Value.Date;
            this.BindMarketFilter();
            this.BindCurrencyFilter();
            this.BindReport();
        }

        private void BindReport()
        {
            DataCommand tranDataCommand = OrderDataHelper.CreateTranDataCommand();
            tranDataCommand.CommandText = "[reporting_OrderReport]";
            tranDataCommand.CommandType = CommandType.StoredProcedure;
            tranDataCommand.Parameters = new DataParameters();
            tranDataCommand.Parameters.Add(new DataParameter("ApplicationId", (object)Mediachase.Commerce.Core.AppContext.Current.ApplicationId, DataParameterType.UniqueIdentifier));
            tranDataCommand.Parameters.Add(new DataParameter("MarketId", (object)this.MarketFilter.SelectedValue, DataParameterType.NVarChar));
            tranDataCommand.Parameters.Add(new DataParameter("CurrencyCode", (object)this.CurrencyFilter.SelectedValue, DataParameterType.NVarChar));
            tranDataCommand.Parameters.Add(new DataParameter("CommerceManagerUrl", (object)ConfigurationManager.AppSettings["SiteUrl"], DataParameterType.NVarChar));            
            tranDataCommand.Parameters.Add(new DataParameter("startdate", (object)this.StartDate.Value.Date.ToUniversalTime(), DataParameterType.DateTime));
            tranDataCommand.Parameters.Add(new DataParameter("enddate", (object)this.EndDate.Value.Date.AddDays(1.0).AddSeconds(-1.0).ToUniversalTime(), DataParameterType.DateTime));           
            tranDataCommand.TableMapping = DataHelper.MapTables(new string[1]
      {
        "reporting_OrderReport"
      });            
            tranDataCommand.DataSet = (DataSet)new Reports();
            DataResult dataResult = DataService.LoadDataSet(tranDataCommand);
            ReportDataSource reportDataSource = new ReportDataSource();
            reportDataSource.Name = "OrdersReport";
            reportDataSource.Value = (object) ((Reports) dataResult.DataSet).reporting_OrderReport;


            this.MyReportViewer.LocalReport.DataSources.Clear();
            this.MyReportViewer.LocalReport.DataSources.Add(reportDataSource);
            this.MyReportViewer.LocalReport.ReportPath = this.MapPath("Reports\\OrdersByMarket.rdlc");
            this.MyReportViewer.DataBind();
        }

      

        private void MarketFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.BindCurrencyFilter();
            this.BindReport();
        }

        private void CurrencyFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.BindReport();
        }

        private void BindMarketFilter()
        {
            this.MarketFilter.Items.Clear();
            this.MarketFilter.Items.Add(new ListItem(SharedStrings.Markets_All, string.Empty));
            foreach (IMarket market in ServiceLocator.Current.GetInstance<IMarketService>().GetAllMarkets())
                this.MarketFilter.Items.Add(new ListItem(market.MarketName, market.MarketId.Value));
            this.MarketFilter.DataBind();
        }

        private void BindCurrencyFilter()
        {
            CurrencyDto.CurrencyDataTable currencyDataTable = new CurrencyDto.CurrencyDataTable();
            CurrencyDto currencyDto = CatalogContext.Current.GetCurrencyDto();
            if (string.IsNullOrEmpty(this.MarketFilter.SelectedValue))
            {
                this.CurrencyFilter.DataSource = (object)currencyDto.Currency;
            }
            else
            {
                foreach (Mediachase.Commerce.Currency currency in ServiceLocator.Current.GetInstance<IMarketService>().GetMarket(new MarketId(this.MarketFilter.SelectedValue)).Currencies)
                {
                    foreach (DataRow dataRow in (InternalDataCollectionBase)currencyDto.Currency.Rows)
                    {
                        if (currency.CurrencyCode.Equals(dataRow["CurrencyCode"].ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            DataRow row = currencyDataTable.NewRow();
                            row.ItemArray = dataRow.ItemArray;
                            currencyDataTable.Rows.Add(row);
                        }
                    }
                }
                this.CurrencyFilter.DataSource = (object)currencyDataTable;
            }
            this.CurrencyFilter.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            this.BindReport();
        }

        protected void MyReportViewer_BookmarkNavigation(object sender, BookmarkNavigationEventArgs e)
        {
            string sqlMetaWhereClause = string.Format(@"META.TrackingNumber = '{0}'",
                                                   e.BookmarkId);

            var purchaseOrder = GetOrdersByMetaField(sqlMetaWhereClause, 1).FirstOrDefault();

            if (purchaseOrder != null)
            {

                e.Cancel = true;
                Response.Redirect("/Apps/Shell/Pages/ContentFrame.aspx?_a=Order&_v=PurchaseOrder-ObjectView&id=" +
                                       purchaseOrder.OrderGroupId);
            }
            e.Cancel = true;
        }

        public static List<PurchaseOrder> GetOrdersByMetaField(string sqlMetaWhereClause, int recordCount = int.MaxValue)
        {
            return GetOrders(string.Empty, sqlMetaWhereClause, recordCount);
        }

        public static List<PurchaseOrder> GetOrders(string sqlWhereClause, string sqlMetaWhereClause, int recordCount = int.MaxValue)
        {
            var orderSearchParameters = new OrderSearchParameters();
            if (!string.IsNullOrEmpty(sqlWhereClause))
            {
                orderSearchParameters.SqlWhereClause = sqlWhereClause;
            }

            if (!string.IsNullOrEmpty(sqlMetaWhereClause))
            {
                orderSearchParameters.SqlMetaWhereClause = sqlMetaWhereClause;
            }

            var orderSearchOptions = new OrderSearchOptions();
            orderSearchOptions.Namespace = "Mediachase.Commerce.Orders";
            orderSearchOptions.Classes.Add("PurchaseOrder");
            orderSearchOptions.Classes.Add("Shipment");
            orderSearchOptions.CacheResults = false;
            orderSearchOptions.RecordsToRetrieve = recordCount;

            return OrderContext.Current.FindPurchaseOrders(orderSearchParameters, orderSearchOptions).ToList();
        }

    }
}