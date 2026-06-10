using LibraryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Fakes
{
    internal class IDeliveryService
    {
        public class FakeDeliveryService : IDeliveryService
        {
            private readonly DeliveryType _deliveryType;

            public FakeDeliveryService(DeliveryType deliveryType)
            {
                _deliveryType = deliveryType;
            }

            public DeliveryType GetDeliveryTypeForBook(int bookId)
            {
                return _deliveryType;
            }
        }
    }
}
