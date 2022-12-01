using EPiServer.Commerce.Bolt;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using Foundation.Infrastructure.Commerce.Markets;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;

namespace Foundation.Features.Checkout.Payments
{
    public class GenericCreditCardPaymentOption : PaymentOptionBase
    {
        public override string SystemKeyword => "Bolt";

        public string CardType { get; set; }

        public GenericCreditCardPaymentOption()
            : this(LocalizationService.Current, 
                  ServiceLocator.Current.GetInstance<IOrderGroupFactory>(),
                  ServiceLocator.Current.GetInstance<ICurrentMarket>(), 
                  ServiceLocator.Current.GetInstance<LanguageService>(),
                  ServiceLocator.Current.GetInstance<IPaymentService>())
        {
        }

        public GenericCreditCardPaymentOption(LocalizationService localizationService,
            IOrderGroupFactory orderGroupFactory,
            ICurrentMarket currentMarket,
            LanguageService languageService,
            IPaymentService paymentService)
            : base(localizationService, orderGroupFactory, currentMarket, languageService, paymentService)
        {
         
        }

        public override IPayment CreatePayment(decimal amount, IOrderGroup orderGroup)
        {
            return new SerializablePayment
            {
                PaymentMethodId = PaymentMethodId,
                PaymentMethodName = SystemKeyword,
                Amount = amount,
                Status = PaymentStatus.Pending.ToString(),
                ImplementationClass = typeof(BoltPayment).AssemblyQualifiedName,
                TransactionType = TransactionType.Authorization.ToString()
            };
        }

        public override bool ValidateData() => IsValid;

        private bool IsValid => true;
    }
}