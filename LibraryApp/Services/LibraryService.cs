

using LibraryApp.Exceptions;
using LibraryApp.Models;

namespace LibraryApp.Services
{
    public class LibraryService
    {
        private readonly IBookService _bookService;
        private readonly IDeliveryService _deliveryService;
        private readonly IPurchaseService _purchaseService;


        public void DoPurchaseCalculation(Book book)
        {

            BookRequestInfo requestsInfo = _bookService.GetBookRequestsInTheLastMonthInfo(book.Id);
            int numOftotalRequests = requestsInfo.NumberOfTotalRequests;

            if (numOftotalRequests == 0)
            {
                throw new NoRequestsForCalculationException("[DoPurchaseCalculation] Book wasn't requested in the last month.");
            }
            double percentOfUnprocessedRequests = requestsInfo.PercentOfUnprocessedRequests;

            DeliveryType deliveryType = _deliveryService.GetDeliveryTypeForBook(book.Id);
            Purchase purchase = new Purchase(book.Id);         

            if (deliveryType.Equals(DeliveryType.Oversea) && percentOfUnprocessedRequests > 70)
            {
                if (book.NumberOfCopies >= 12)
                {
                    purchase.NumberOfCopiesToBePurchased = 25;
                }
                else
                {
                    purchase.NumberOfCopiesToBePurchased = 20;
                }
            }
            else if (deliveryType.Equals(DeliveryType.International))
            {
                if(percentOfUnprocessedRequests >= 40 || numOftotalRequests > 15)
                {
                    purchase.NumberOfCopiesToBePurchased = 20;
                }
                else
                {
                    purchase.NumberOfCopiesToBePurchased = 16;
                }
            }
            else
            {
                purchase.NumberOfCopiesToBePurchased = 15;
            }

            _purchaseService.CreatePurchase(purchase);
        }


        public int GetMemberDiscount(double bookPrice, int numOfPurchasesInTheLastMonth, bool penalty, ActivityFrequency activityFrequency)
        {
            if (activityFrequency.Equals(ActivityFrequency.High))
            {
                if (numOfPurchasesInTheLastMonth > 8 && bookPrice > 20.0)
                {
                    if (penalty)
                        return 25;
                    return 30;
                }
                return 10;
            }
            else if (activityFrequency.Equals(ActivityFrequency.Regular))
            {
                if(numOfPurchasesInTheLastMonth > 10 || bookPrice >= 27.0)
                    return 20;
                return 10;
            }

            return 0;
        }

    }
}
