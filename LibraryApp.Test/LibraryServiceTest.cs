
using LibraryApp.Exceptions;
using LibraryApp.Models;
using LibraryApp.Services;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Diagnostics.Metrics;

namespace LibraryApp.Test
{
    //Guid example: "00000000-0000-0000-0000-000000000001"
    [TestFixture]
    public class LibraryServiceTest
        {
            private IBookService _fakeBookService;
            private IDeliveryService _fakeDeliveryService;
            private IPurchaseService _fakePurchaseService;
            private LibraryService _libraryService;

            // =====================================================================
            // F1: GetMemberDiscount — testovi pokretani pomocu PICT TestCaseSource
            // =====================================================================

            /// <summary>
            /// F1 - Testovi generisani iz PICT Results.txt fajla pomocu PictParser-a.
            /// Svaki test proverava ispravan proracun popusta za clana biblioteke.
            /// </summary>
            [TestCaseSource(typeof(PictParser), nameof(PictParser.GetTestCases))]
            public void GetMemberDiscount_PictTestCases_ReturnsExpectedResult(
                ActivityFrequency activityFrequency, int numOfPurchases, double bookPrice, bool penalty, int expectedResult)
            {
                // Arrange
                _fakeBookService = new IBookService(new BookRequestInfo { NumberOfTotalRequests = 5, PercentOfUnprocessedRequests = 20 });
                _fakeDeliveryService = new IDeliveryService(DeliveryType.Local);
                _fakePurchaseService = new IPurchaseService();
                _libraryService = new LibraryService(_IBookService, _IDeliveryService, _IPurchaseService);

                // Act
                int result = _libraryService.GetMemberDiscount(bookPrice, numOfPurchases, penalty, activityFrequency);

                // Assert
                Assert.That(result, Is.EqualTo(expectedResult));
            }

            // =====================================================================
            // F1: GetMemberDiscount — dodatni direktni testovi
            // =====================================================================

            [SetUp]
            public void Setup()
            {
                _fakeBookService = new IBookService(new BookRequestInfo
                {
                    NumberOfTotalRequests = 10,
                    PercentOfUnprocessedRequests = 30
                });
                _fakeDeliveryService = new IDeliveryService(DeliveryType.Other);
                _fakePurchaseService = new IPurchaseService();
                _libraryService = new LibraryService(_fakeBookService, _fakeDeliveryService, _fakePurchaseService);
            }

            /// <summary>
            /// F1 - Kada je frekvencija High, broj kupovina > 8, cena > 20 i postoji penalty, popust je 25.
            /// </summary>
            [Test]
            public void GetMemberDiscount_HighFrequencyWithPenalty_Returns25Percent()
            {
                // Arrange
                ActivityFrequency activityFrequency = ActivityFrequency.High;
                int numOfPurchasesInTheLastMonth = 9;
                double bookPrice = 25.0;
                bool penalty = true;

                // Act
                int result = _libraryService.GetMemberDiscount(bookPrice, numOfPurchasesInTheLastMonth, penalty, activityFrequency);

                // Assert
                Assert.That(result, Is.EqualTo(25));
            }

            /// <summary>
            /// F1 - Kada je frekvencija High, broj kupovina > 8, cena > 20 i nema penala, popust je 30.
            /// </summary>
            [Test]
            public void GetMemberDiscount_HighFrequencyWithoutPenalty_Returns30Percent()
            {
                // Arrange
                ActivityFrequency activityFrequency = ActivityFrequency.High;
                int numOfPurchasesInTheLastMonth = 10;
                double bookPrice = 21.0;
                bool penalty = false;

                // Act
                int result = _libraryService.GetMemberDiscount(bookPrice, numOfPurchasesInTheLastMonth, penalty, activityFrequency);

                // Assert
                Assert.That(result, Is.EqualTo(30));
            }

            /// <summary>
            /// F1 - Kada je frekvencija Regular i cena knjige je >= 27.0, popust je 20.
            /// </summary>
            [Test]
            public void GetMemberDiscount_RegularFrequencyExpensiveBook_Returns20Percent()
            {
                // Arrange
                ActivityFrequency activityFrequency = ActivityFrequency.Regular;
                int numOfPurchasesInTheLastMonth = 2;
                double bookPrice = 28.5;
                bool penalty = false;

                // Act
                int result = _libraryService.GetMemberDiscount(bookPrice, numOfPurchasesInTheLastMonth, penalty, activityFrequency);

                // Assert
                Assert.That(result, Is.EqualTo(20));
            }

            // =====================================================================
            // F2: DoPurchaseCalculation — testovi uslova i proracuna nabavke kolicina
            // =====================================================================

            /// <summary>
            /// F2 - Kada je dostava Oversea, procenat neobradjenih > 70 i broj kopija < 12, kolicina je 20.
            /// </summary>
            [Test]
            public void DoPurchaseCalculation_OverseaHighUnprocessedLowCopies_Purchases20()
            {
                // Arrange
                var book = new Book { Id = Guid.Parse("96bff8d0-3254-404c-9f50-51a8e135a183"), NumberOfCopies = 10 };
                var requestInfo = new BookRequestInfo { NumberOfTotalRequests = 5, PercentOfUnprocessedRequests = 75.0 };

                _fakeBookService = new IBookService(requestInfo);
                _fakeDeliveryService = new IDeliveryService(DeliveryType.Oversea);
                _libraryService = new LibraryService(_fakeBookService, _fakeDeliveryService, _fakePurchaseService);

                // Act
                _libraryService.DoPurchaseCalculation(book);

                // Assert
                Assert.That(_fakePurchaseService.CreatePurchaseCalled, Is.True);
                Assert.That(_fakePurchaseService.LastCreatedPurchase.NumberOfCopiesToBePurchased, Is.EqualTo(20));
            }

            /// <summary>
            /// F2 - Kada je dostava Oversea, procenat neobradjenih > 70 i broj kopija >= 12, kolicina je 25.
            /// </summary>
            [Test]
            public void DoPurchaseCalculation_OverseaHighUnprocessedHighCopies_Purchases25()
            {
                // Arrange
                var book = new Book { Id = Guid.Parse("96bff8d0-3254-404c-9f50-51a8e135a183"), NumberOfCopies = 12 };
                var requestInfo = new BookRequestInfo { NumberOfTotalRequests = 5, PercentOfUnprocessedRequests = 71.0 };

                _fakeBookService = new IBookService(requestInfo);
                _fakeDeliveryService = new IDeliveryService(DeliveryType.Oversea);
                _libraryService = new LibraryService(_fakeBookService, _fakeDeliveryService, _fakePurchaseService);

                // Act
                _libraryService.DoPurchaseCalculation(book);

                // Assert
                Assert.That(_fakePurchaseService.LastCreatedPurchase.NumberOfCopiesToBePurchased, Is.EqualTo(25));
            }

            /// <summary>
            /// F2 - Kada je dostava International i procenat neobradjenih >= 40 (granicna vrednost), kolicina je 20.
            /// </summary>
            [Test]
            public void DoPurchaseCalculation_InternationalBoundaryUnprocessedPercent_Purchases20()
            {
                // Arrange
                var book = new Book { Id = Guid.Parse("96bff8d0-3254-404c-9f50-51a8e135a183"), NumberOfCopies = 5 };
                var requestInfo = new BookRequestInfo { NumberOfTotalRequests = 10, PercentOfUnprocessedRequests = 40.0 };

                _fakeBookService = new IBookService(requestInfo);
                _fakeDeliveryService = new IDeliveryService(DeliveryType.International);
                _libraryService = new LibraryService(_fakeBookService, _fakeDeliveryService, _fakePurchaseService);

                // Act
                _libraryService.DoPurchaseCalculation(book);

                // Assert
                Assert.That(_fakePurchaseService.LastCreatedPurchase.NumberOfCopiesToBePurchased, Is.EqualTo(20));
            }

            /// <summary>
            /// F2 - Kada je dostava International, ali uslovi za vecu kolicinu nisu ispunjeni, kolicina je 16.
            /// </summary>
            [Test]
            public void DoPurchaseCalculation_InternationalNoSpecialConditions_Purchases16()
            {
                // Arrange
                var book = new Book { Id = Guid.Parse("96bff8d0-3254-404c-9f50-51a8e135a183"), NumberOfCopies = 5 };
                var requestInfo = new BookRequestInfo { NumberOfTotalRequests = 15, PercentOfUnprocessedRequests = 39.9 };

                _fakeBookService = new IBookService(requestInfo);
                _fakeDeliveryService = new IDeliveryService(DeliveryType.International);
                _libraryService = new LibraryService(_fakeBookService, _fakeDeliveryService, _fakePurchaseService);

                // Act
                _libraryService.DoPurchaseCalculation(book);

                // Assert
                Assert.That(_fakePurchaseService.LastCreatedPurchase.NumberOfCopiesToBePurchased, Is.EqualTo(16));
            }

            /// <summary>
            /// F2 - Za sve ostale tipove dostave (podrazumevani else), kolicina je uvek 15.
            /// </summary>
            [Test]
            public void DoPurchaseCalculation_OtherDeliveryType_Purchases15()
            {
                // Arrange
                var book = new Book { Id = Guid.Parse("96bff8d0-3254-404c-9f50-51a8e135a183"), NumberOfCopies = 2 };
                var requestInfo = new BookRequestInfo { NumberOfTotalRequests = 5, PercentOfUnprocessedRequests = 10.0 };

                _fakeBookService = new IBookService(requestInfo);
                _fakeDeliveryService = new IDeliveryService(DeliveryType.Other);
                _libraryService = new LibraryService(_fakeBookService, _fakeDeliveryService, _fakePurchaseService);

                // Act
                _libraryService.DoPurchaseCalculation(book);

                // Assert
                Assert.That(_fakePurchaseService.LastCreatedPurchase.NumberOfCopiesToBePurchased, Is.EqualTo(15));
            }

            // =====================================================================
            // F3: DoPurchaseCalculation — testiranje izuzetaka (Exceptions)
            // =====================================================================

            /// <summary>
            /// F3 - Kada je broj ukupnih zahteva jednak 0, mora se baciti NoRequestsForCalculationException.
            /// </summary>
            [Test]
            public void DoPurchaseCalculation_TotalRequestsZero_ThrowsNoRequestsForCalculationException()
            {
                // Arrange
                var book = new Book { Id = Guid.Parse("96bff8d0-3254-404c-9f50-51a8e135a183"), NumberOfCopies = 5 };
                var requestInfo = new BookRequestInfo { NumberOfTotalRequests = 0, PercentOfUnprocessedRequests = 0.0 };

                _fakeBookService = new IBookService(requestInfo);
                _fakeDeliveryService = new IDeliveryService(DeliveryType.Local);
                _libraryService = new LibraryService(_fakeBookService, _fakeDeliveryService, _fakePurchaseService);

                // Act & Assert
                Assert.That(
                    (Action)(() => _libraryService.DoPurchaseCalculation(book)),
                    Throws.TypeOf<NoRequestsForCalculationException>());
            }
        }
    }
