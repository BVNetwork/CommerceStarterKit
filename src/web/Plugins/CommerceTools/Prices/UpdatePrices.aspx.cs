/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Dto;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Pricing;
using OxxCommerceStarterKit.Web.Helpers;

namespace OxxCommerceStarterKit.Web.Plugins.CommerceTools.Prices
{    
    public partial class UpdatePrices : WebFormsBase
    {
        private IMarketService _marketRepository;

        private IPriceService _priceService;
        private IContentRepository _repo;
        private ReferenceConverter _referenceConverter;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            this.MasterPageFile = EPiServer.UriSupport.ResolveUrlFromUIBySettings("MasterPages/Frameworks/Framework.Master");
            if (EPiServer.Security.PrincipalInfo.CurrentPrincipal.IsInRole("Administrators") == false
                && EPiServer.Security.PrincipalInfo.CurrentPrincipal.IsInRole("WebAdmins") == false)
            {
                throw new AccessDeniedException();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this._marketRepository = ServiceLocator.Current.GetInstance<IMarketService>();
            this._priceService = ServiceLocator.Current.GetInstance<IPriceService>();

            ErrorPanel.Visible = false;

            if (!IsPostBack)
            {
                BindMarkets();
                BindPriceTypes();
            }
        }

        private void BindPriceTypes()
        {
            Array itemValues = System.Enum.GetValues(typeof(CustomerPricing.PriceType));
            PriceType.DataSource = itemValues;
            PriceType.DataBind();
        }

        private void BindMarkets()
        {
            var markets = this._marketRepository.GetAllMarkets();
            var marketList = markets.Select(m => new { m.MarketId, DisplayText = m.MarketDescription.ToString() + " (" + m.MarketName + ")" });

            Market.DataSource = marketList;
            Market.DataBind();
        }

        protected void SetPrice_Click(object sender, EventArgs e)
        {
            try
            {
                if (SkuList.Items.Count <= 0)
                {
                    throw new Exception("Search for a valid style/variant");
                }

                DateTime from = DateTime.Parse(StartDate.Value + " " + startTime.Value);
                DateTime until = DateTime.Parse(EndDate.Value + " " + endTime.Value);

                string currency = SelectedCurrency.Text;

                if (string.IsNullOrEmpty(currency))
                    throw new Exception("Select Market to define currency.");

                decimal value = 0;

                if (!Decimal.TryParse(Value.Text, out value))
                {
                    throw new Exception("Not a valid price value");
                }

                var market = _marketRepository.GetMarket(Market.SelectedValue);

                foreach (var item in this.SkuList.Items)
                {
                    this.UpdatePriceOnSku(item.ToString(), from, until, market, currency, value);
                }

                CatalogCache.Clear();

               // IndexEntry();

                this.BindPrices();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
                ErrorPanel.Visible = true;
            }

        }

    
        private void UpdatePriceOnSku(string code, DateTime from, DateTime until, IMarket market, string currency, decimal value)
        {
            var catalogKey = new CatalogKey(Mediachase.Commerce.Core.AppContext.Current.ApplicationId, code);



            var priceServiceDatabase =ServiceLocator.Current.GetInstance<IPriceService>();

            CustomerPricing.PriceType type =
                (CustomerPricing.PriceType)Enum.Parse(typeof(CustomerPricing.PriceType), PriceType.SelectedValue);
            var priceCode = PriceCode.Text;


            var price = new PriceValue()
            {
                CatalogKey = catalogKey,
                CustomerPricing = new CustomerPricing(type, priceCode),
                MarketId = market.MarketId,
                MinQuantity = 0,
                UnitPrice = new Money(value, new Currency(currency)),
                ValidFrom = from,
                ValidUntil = until
            };


            priceServiceDatabase.SetCatalogEntryPrices(catalogKey,new[]{price});
        
        }

        protected void GetEntry_Click(object sender, EventArgs e)
        {
            try
            {
                this.ResetPanels();

                _repo = ServiceLocator.Current.GetInstance<IContentRepository>();
                _referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();

                var content = _repo.Get<CatalogContentBase>(_referenceConverter.GetContentLink(Code.Value));

                ErrorMessage.Text = (content is PackageContent).ToString();

                string code = Code.Value;
                var entry = CatalogContext.Current.GetCatalogEntry(
                    code, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Children));

                if (entry == null)
                    throw new Exception(string.Format("No style or variant found with code {0}", code));

                DisplayVariant(entry);

            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
                ErrorPanel.Visible = true;
            }
        }

  
        public List<KeyValuePair<int, decimal>> GetPackageChildEntries(int id)
        {
            List<KeyValuePair<int, decimal>> childEntryQuanities = new List<KeyValuePair<int, decimal>>();

            CatalogRelationResponseGroup group =
              new CatalogRelationResponseGroup(CatalogRelationResponseGroup.ResponseGroup.CatalogEntry);
            CatalogRelationDto relations = CatalogContext.Current.GetCatalogRelationDto(0, 0, id, string.Empty, group);

            if (relations != null && relations.CatalogEntryRelation != null && relations.CatalogEntryRelation.Count > 0)
            {
                foreach (CatalogRelationDto.CatalogEntryRelationRow row in relations.CatalogEntryRelation.Rows)
                {
                    childEntryQuanities.Add(new KeyValuePair<int, decimal>(row.ChildEntryId, row.Quantity));
                }
            }

            return childEntryQuanities;
        }

        private void DisplayVariant(Entry entry)
        {
            if (entry.Entries.Entry != null)
            {
                var variants = new Entry[] { entry }.Select(e => new { DisplayText = e.Name + " (" + e.ID + ")" });
                VariantsList.DataSource = variants;
                VariantPanel.Visible = true;
                BindSkus(new Entry[] { entry });
                BindPrices();
            }
            VariantsList.DataBind();
        }

        private void DisplayStyle(Entry entry)
        {
            StyleNameValue.Text = entry.Name;
            StylePanel.Visible = true;

            if (entry.Entries.Entry != null)
            {
                var variants = entry.Entries.Entry.Select(e => new { DisplayText = e.Name + " (" + e.ID + ")" });
                VariantsList.DataSource = variants;
                VariantPanel.Visible = true;
                BindSkus(entry.Entries.Entry);
                BindPrices();
            }
            VariantsList.DataBind();
        }

        private void BindSkus(Entry[] variants)
        {
            List<Entry> skus = new List<Entry>();
            foreach (var variant in variants)
            {
                var entry = CatalogContext.Current.GetCatalogEntry(
                    variant.CatalogEntryId,
                    new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.Children));

                if (entry.Entries.Entry != null)
                {
                    foreach (var sku in entry.Entries.Entry)
                    {
                        skus.Add(sku);
                    }
                }

            }
            SkuList.DataSource = skus;
            SkuList.DataBind();
        }

        private void ResetPanels()
        {
            StylePanel.Visible = false;
            VariantPanel.Visible = false;
        }


        private void BindPrices()
        {
            var skuEntry = CatalogContext.Current.GetCatalogEntry(this.SkuList.Items[0].Value);

            var prices =
                PriceHelper.BuildValidPrices(skuEntry.ID)
                                    .Select(
                                        p =>
                                        new
                                        {
                                            From = p.ValidFrom,
                                            To = p.ValidUntil,
                                            Value = p.UnitPrice.Amount.ToString("# ##0.00"),
                                            Currency = p.UnitPrice.Currency.CurrencyCode,
                                            Type = ((CustomerPricing.PriceType)p.CustomerPricing.PriceTypeId).ToString(),
                                            PriceCode = p.CustomerPricing.PriceCode
                                        });
          
            this.SkuPrices.DataSource = prices;
            this.SkuPrices.DataBind();
        }

        protected void Market_SelectedChanged(object sender, EventArgs e)
        {
            var currency = this._marketRepository.GetMarket(Market.SelectedItem.Value).DefaultCurrency;
            SelectedCurrency.Text = currency;
        }

        protected void SkuPrice_OnDeleting(object sender, GridViewDeleteEventArgs e)
        {
            ResetRowColors((GridView)sender);
            ((GridView)sender).Rows[e.RowIndex].BackColor = Color.CornflowerBlue;
            Price.Value = string.Join(";", e.Values);
            DeleteFromSkuId.Value = SkuList.Items[0].Value;
            DeleteValidFrom.Value = e.Values["From"].ToString();
            DeleteValidTo.Value = e.Values["To"].ToString();
            DeleteType.Value = e.Values["Type"].ToString();
            DeleteCurrency.Value = e.Values["Currency"].ToString();
            DeleteValue.Value = e.Values["Value"].ToString();
            DeletePricePanel.Visible = true;
        }

        private void ResetRowColors(GridView sender)
        {
            foreach (GridViewRow row in sender.Rows)
            {
                row.BackColor = Color.Transparent;
            }
        }

        protected void ConfirmDelete_Click(object sender, EventArgs e)
        {
            var priceService = ServiceLocator.Current.GetInstance<IPriceService>();

            var skuId = DeleteFromSkuId.Value;
            var validFrom = DateTime.Parse(DeleteValidFrom.Value);
            var validTo = DateTime.Parse(DeleteValidTo.Value);
            var currency = DeleteCurrency.Value;
            var amount = Decimal.Parse(DeleteValue.Value.Replace(" ", ""));
            var type = DeleteType.Value;

            List<string> skus = (from ListItem item in this.SkuList.Items select item.Value).ToList();

            //IEnumerable<IPriceValue> prices = VarnerPricingManager.GetAllSKUPrices(skuId).Where(p => p.ValidFrom != validFrom || p.ValidUntil != validTo || p.UnitPrice.Currency.CurrencyCode != currency || ((VarnerPricingTypeEnum)p.CustomerPricing.PriceTypeId).ToString() != type || p.UnitPrice.Amount != amount);

            //foreach (var sku in skus)
            //{
            //    VarnerPricingManager.DeleteEntryPrices(sku, false);
            //    var key = new CatalogKey(AppContext.Current.ApplicationId, sku);
            //    List<SalePriceValue> spv = prices.Select(priceValue => new SalePriceValue(key, priceValue.MarketId, priceValue.CustomerPricing, priceValue.ValidFrom, priceValue.ValidUntil, 1, priceValue.UnitPrice)).ToList();
            //    priceService.SetCatalogEntryPrices(key, spv);
            //}

            //CatalogCache.Clear();

            //IndexEntry();


            this.BindPrices();
            DeletePricePanel.Visible = false;
        }

        protected void Cancel_Click(object sender, EventArgs e)
        {
            DeletePricePanel.Visible = false;
            this.BindPrices();
        }
    }
}
