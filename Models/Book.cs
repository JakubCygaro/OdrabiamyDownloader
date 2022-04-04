using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdrabiamyD.Models
{
    /// <summary>
    /// Obiekt modelujący Cionszkę pobieraną z Odrabiamy.pl
    /// </summary>
    public class Book
    {
        /// <summary>
        /// ID Cionszki
        /// </summary>
        public int Id { get; init; }
        /// <summary>
        /// Tablica <c>Page[]</c> reprezentująca strony pobranej książki
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
