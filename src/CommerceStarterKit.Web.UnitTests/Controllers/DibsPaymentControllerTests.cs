using System;
using System.Globalization;
using System.Security.Principal;
using System.Threading;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Web;
using EPiServer.Web.Routing;
using Mediachase.Commerce.Engine.Caching;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Shared;
using Mediachase.MetaDataPlus;
using Moq;
using NUnit.Framework;
using OxxCommerceStarterKit.Core.PaymentProviders.DIBS;
using OxxCommerceStarterKit.Web;
using OxxCommerceStarterKit.Web.Business.ClientTracking;
using OxxCommerceStarterKit.Web.Business.Payment;
using OxxCommerceStarterKit.Web.Controllers;
using OxxCommerceStarterKit.Web.Models.PageTypes.Payment;
using OxxCommerceStarterKit.Web.Models.PageTypes.System;
using OxxCommerceStarterKit.Web.Models.ViewModels;
using Ploeh.AutoFixture;
using OxxCommerceStarterKit.Web.Models.PageTypes;
using Should;
using OxxCommerceStarterKit.Core.Objects.SharedViewModels;

namespace CommerceStarterKit.Web.Controllers
{
    public class DibsPaymentControllerTests
    {
        private static readonly Fixture Fixture = new Fixture();
        private DibsPaymentController _sut;
        private Mock<IContentRepository> _contentRepositoryMock;
        private Mock<IDibsPaymentProcessor> _dibsPaymentProcessorMock;
        private Mock<IReceiptViewModelBuilder> _receiptViewModelBuilderMock;
        private Mock<IIdentityProvider> _identityProvider;
        private SettingsBlock _settingsBlock;
        private Mock<IGoogleAnalyticsTracker> _googleAnalyticsTracker;
        private Mock<ILogger> _logger;
        private Mock<IPermanentLinkMapper> _permanentLinkMapper;

        static DibsPaymentControllerTests()
        {
            var receiptPage = new PageReference(Fixture.Create<int>());
            Fixture.Register(() => new SettingsBlock()
            {
                ReceiptPage = receiptPage
            });

            RegisterPurchaseOrderCreationRules();
            Fixture.Register(
                () => new DibsPaymentProcessingResult(Fixture.Create<PurchaseOrderModel>(), Fixture.Create<string>()));
            Fixture.Register<IFormatProvider>(() => CultureInfo.CurrentCulture.NumberFormat);

        }

        private static void RegisterPurchaseOrderCreationRules()
        {
            Fixture.Register<PurchaseOrder>(
                () => Fixture.Build<PurchaseOrderStub>()
                    .OmitAutoProperties()
                    .With(x => x.TrackingNumber)
                    .With(x => x.Created)
                    .With(x => x.Status)
                    .With(x => x.Total)
                    .With(x => x.ShippingTotal)
                    .With(x => x.TaxTotal)
                    .Create());
        }

        [SetUp]
        public virtual void SetUp()
        {
            _dibsPaymentProcessorMock = new Mock<IDibsPaymentProcessor>();
            _settingsBlock = Fixture.Create<SettingsBlock>();
            SetUpContentRepository();
            _identityProvider = new Mock<IIdentityProvider>();
            _receiptViewModelBuilderMock = new Mock<IReceiptViewModelBuilder>();
            _googleAnalyticsTracker = new Mock<IGoogleAnalyticsTracker>();
            _permanentLinkMapper = new Mock<IPermanentLinkMapper>();

            _sut = new DibsPaymentController(_identityProvider.Object, _contentRepositoryMock.Object, _dibsPaymentProcessorMock.Object, _receiptViewModelBuilderMock.Object, 
                _googleAnalyticsTracker.Object, 
                null
                );
        }

        private void SetUpContentRepository()
        {
            _contentRepositoryMock = new Mock<IContentRepository>();
            _contentRepositoryMock.Setup(c => c.Get<ReceiptPage>(_settingsBlock.ReceiptPage))
                .Returns(new Mock<ReceiptPage>().Object);
        }

        public class When_processing_a_completed_payment : DibsPaymentControllerTests
        {
            private DibsPaymentResult _paymentResponse;
            private DibsPaymentProcessingResult _processingResult;
            private ReceiptViewModel _expectedModel;
            public override void SetUp()
            {
                base.SetUp();

                _paymentResponse = Fixture.Create<DibsPaymentResult>();
                _processingResult = new DibsPaymentProcessingResult(Fixture.Create<PurchaseOrderModel>(),
                    Fixture.Create<string>());
                _dibsPaymentProcessorMock.Setup(x => x.ProcessPaymentResult(_paymentResponse, It.IsAny<IIdentity>()))
                    .Returns(_processingResult);
                _expectedModel = CreateReceiptViewModel();
                _receiptViewModelBuilderMock.Setup(b => b.BuildFor(_processingResult)).Returns(_expectedModel);
            }

            [Test]
            public void _then_the_payment_result_is_passed_to_the_payment_complete_processor()
            {
                var resultModel = GetModel();

                resultModel.ShouldEqual(_expectedModel);
            }

            private ReceiptViewModel GetModel()
            {
                var result = (ViewResult) _sut.ProcessPayment(new DibsPaymentPage(), _paymentResponse);
                var resultModel = (ReceiptViewModel) result.Model;
                return resultModel;
            }

            [Test]
            public void _then_the_purchase_is_tracked_via_google_analytics()
            {
                _sut.ProcessPayment(new DibsPaymentPage(), _paymentResponse);

                _googleAnalyticsTracker.Verify(t => t.TrackAfterPayment(_expectedModel));
            }

            private static ReceiptViewModel CreateReceiptViewModel()
            {
                var modelMock = new Mock<ReceiptViewModel>();
                modelMock.Setup(f => f.ThankYouText.ToString()).Returns(Fixture.Create<string>);
                return modelMock.Object;
            }
        }

    }
}
