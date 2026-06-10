using LibraryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Fakes
{
    internal class IBookService
    {

        public class FakeBookService : IBookService
        {
            private readonly BookRequestInfo _bookRequestInfo;

            public FakeBookService(BookRequestInfo bookRequestInfo)
            {
                _bookRequestInfo = bookRequestInfo;
            }

            public BookRequestInfo GetBookRequestsInTheLastMonthInfo(int bookId)
            {
                return _bookRequestInfo;
            }
        }
    }
}
    