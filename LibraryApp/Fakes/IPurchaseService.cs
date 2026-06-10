using LibraryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Fakes
{
    internal class IPurchaseService
    {
        public class FakePurchaseService : IPurchaseService
        {
            public bool CreatePurchaseCalled { get; set; } = false;

            public void CreatePurchase(Purchase purchase)
            {
                CreatePurchaseCalled = true;
            }
        }
    }
}
