using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdrabiamyD.Models
{
    /// <summary>
    /// An object modeling a book
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Id of a book
        /// </summary>
        public int Id { get; init; }
        /// <summary>
        /// A <c>Page[]</c> representing the pages a book contains
        /// </summary>
        public Page[] Pages { get; init; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pages"></param>
        public Book(int id, Page[] pages)
        {
            Id = id;
            Pages = pages;
        }

    }
}
