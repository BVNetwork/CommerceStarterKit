using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using System.Collections.Generic;
using OxxCommerceStarterKit.Core.Objects;

namespace OxxCommerceStarterKit.Core.Services
{
    public interface ICartService
    {
        bool AddToCart(ICart cart, string code, int quantity, out string warningMessage);
        void SetCartCurrency(ICart cart, Currency currency);
        Dictionary<ILineItem, List<ValidationIssue>> ValidateCart(ICart cart);
        Dictionary<ILineItem, List<ValidationIssue>> RequestInventory(ICart cart);
        string DefaultCartName { get; }
        string DefaultWishListName { get; }
        ICart LoadCart(string name);
        ICart LoadOrCreateCart(string name);
        bool AddCouponCode(ICart cart, string couponCode);
        void RemoveCouponCode(ICart cart, string couponCode);
        void MergeShipments(ICart cart);

        CartActionResult AddToCart(LineItem lineItem);
        CartActionResult AddToWishList(LineItem lineItem);
        List<LineItem> GetItems(string cart, string language);
        CartActionResult UpdateCart(string name, LineItem product);
        decimal GetTotal(string name);
        decimal GetTotalAmount(string name);
        decimal GetTotalLineItemsAmount(string name);
        decimal GetTotalDiscount(string name);
        decimal GetTax(string name);
        decimal GetShipping(string name);
        List<DiscountItem> GetAllDiscountCodes(string name);

        CartActionResult RemoveFromCart(string name, LineItem product);
        CartActionResult MoveBetweenCarts(string fromName, string toName, LineItem product);
        CartActionResult EmptyCart(string name);
        CartActionResult ValidateCart(string name);
        CartActionResult AddDiscountCode(string name, string code);
        void UpdateShipping(string name);

    }
}