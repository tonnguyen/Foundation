using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;
using EPiServer.Filters;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using Foundation.Commerce.Customer.Services;
using Foundation.Commerce.Extensions;
using Foundation.Features.CatalogContent.Package;
using Foundation.Features.CatalogContent.Variation;
using Foundation.Features.Checkout.Services;
using Foundation.Features.Checkout.ViewModels;
using Foundation.Features.MyAccount.AddressBook;
using Foundation.Features.Shared;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Security;
using Microsoft.ReportingServices.RdlExpressions.ExpressionHostObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foundation.Features.MyAccount.OrderConfirmation
{
    public abstract class OrderConfirmationControllerBase<T> : PageController<T> where T : FoundationPageData
    {
        protected readonly ConfirmationService _confirmationService;
        private readonly IAddressBookService _addressBookService;
        private readonly IOrderGroupCalculator _orderGroupCalculator;
        private readonly UrlResolver _urlResolver;
        protected readonly ICustomerService _customerService;
        private readonly FilterPublished _filterPublished;
        private readonly ICurrentMarket _currentMarket;
        private readonly IContentLoader _contentLoader;
        private readonly LanguageResolver _languageResolver;
        private readonly IRelationRepository _relationRepository;

        protected OrderConfirmationControllerBase(ConfirmationService confirmationService,
            IAddressBookService addressBookService,
            IOrderGroupCalculator orderGroupTotalsCalculator,
            UrlResolver urlResolver,
            ICustomerService customerService,
            IContentLoader contentLoader,
            FilterPublished filterPublished,
            ICurrentMarket currentMarket,
            LanguageResolver languageResolver,
            IRelationRepository relationRepository)
        {
            _confirmationService = confirmationService;
            _addressBookService = addressBookService;
            _orderGroupCalculator = orderGroupTotalsCalculator;
            _urlResolver = urlResolver;
            _customerService = customerService;
            _contentLoader = contentLoader;
            _currentMarket = currentMarket;
            _filterPublished = filterPublished;
            _languageResolver = languageResolver;
            _relationRepository = relationRepository;
        }

        protected OrderConfirmationViewModel<T> CreateViewModel(T currentPage, IPurchaseOrder order)
        {
            var hasOrder = order != null;

            if (!hasOrder)
            {
                return new OrderConfirmationViewModel<T>(currentPage);
            }

            var lineItems = order.GetFirstForm().Shipments.SelectMany(x => x.LineItems);
            var totals = _orderGroupCalculator.GetOrderGroupTotals(order);

            var package = new PackageContent();
            List<string> stringNames = new List<string>();
            var variantNames = new List<string>();
            
            foreach (var item in lineItems)
            {
                package = item.GetEntryContent() as PackageContent;

                if (package is PackageContent content)
                {
                    variantNames = _contentLoader
                      .GetItems(content.GetEntries(_relationRepository), _languageResolver.GetPreferredCulture())
                      .OfType<VariationContent>()
                      .Where(v => v.IsAvailableInCurrentMarket(_currentMarket) && !_filterPublished.ShouldFilter(v))
                      .Select(v => v.DisplayName)
                      .ToList();
                }

                StringBuilder displayName = new StringBuilder(package.DisplayName);

                if (variantNames != null)
                {
                    displayName.Append(" (");
                    foreach (var item1 in variantNames)
                    {
                        displayName.Append($"{item1}");
                        if (item1.Equals(variantNames.Last()))
                        {
                            displayName.Append(")");
                        }
                        else
                        {
                            displayName.Append(", ");
                        }
                    }
                }

                stringNames.Add(displayName.ToString());
            }

            var viewModel = new OrderConfirmationViewModel<T>(currentPage)
            {
                DisplayPackageNames = stringNames,
                Currency = order.Currency,
                CurrentContent = currentPage,
                HasOrder = hasOrder,
                OrderId = order.OrderNumber,
                Created = order.Created,
                Items = lineItems,
                BillingAddress = new AddressModel(),
                ShippingAddresses = new List<AddressModel>(),
                ContactId = PrincipalInfo.CurrentPrincipal.GetContactId(),
                Payments = order.GetFirstForm().Payments.Where(c => c.TransactionType == TransactionType.Authorization.ToString() || c.TransactionType == TransactionType.Sale.ToString()),
                OrderGroupId = order.OrderLink.OrderGroupId,
                OrderLevelDiscountTotal = order.GetOrderDiscountTotal(),
                ShippingSubTotal = order.GetShippingSubTotal(),
                ShippingDiscountTotal = order.GetShippingDiscountTotal(),
                ShippingTotal = totals.ShippingTotal,
                HandlingTotal = totals.HandlingTotal,
                TaxTotal = totals.TaxTotal,
                CartTotal = totals.Total,
                SubTotal = order.GetSubTotal(),
                FileUrls = new List<Dictionary<string, string>>(),
                Keys = new List<Dictionary<string, string>>()
            };

            foreach (var lineItem in lineItems)
            {
                var entry = lineItem.GetEntryContent<EntryContentBase>();
                var variant = entry as GenericVariant;
                if (entry == null || variant == null || variant.VirtualProductMode == null || variant.VirtualProductMode.Equals("None"))
                {
                    continue;
                }

                if (variant.VirtualProductMode.Equals("File"))
                {
                    var url = "";// _urlResolver.GetUrl(((FileVariant)lineItem.GetEntryContentBase()).File);
                    viewModel.FileUrls.Add(new Dictionary<string, string>() { { lineItem.DisplayName, url } });
                }
                else if (variant.VirtualProductMode.Equals("Key"))
                {
                    var key = Guid.NewGuid().ToString();
                    viewModel.Keys.Add(new Dictionary<string, string>() { { lineItem.DisplayName, key } });
                }
                else if (variant.VirtualProductMode.Equals("ElevatedRole"))
                {
                    viewModel.ElevatedRole = variant.VirtualProductRole;
                    var currentContact = _customerService.GetCurrentContact();
                    if (currentContact != null)
                    {
                        currentContact.ElevatedRole = "Reader";
                        currentContact.SaveChanges();
                    }
                }
            }

            var billingAddress = order.GetFirstForm().Payments.First().BillingAddress;

            // Map the billing address using the billing id of the order form.
            _addressBookService.MapToModel(billingAddress, viewModel.BillingAddress);

            // Map the remaining addresses as shipping addresses.
            foreach (var orderAddress in order.Forms.SelectMany(x => x.Shipments).Select(s => s.ShippingAddress))
            {
                var shippingAddress = new AddressModel();
                _addressBookService.MapToModel(orderAddress, shippingAddress);
                viewModel.ShippingAddresses.Add(shippingAddress);
            }

            return viewModel;
        }
    }
}
