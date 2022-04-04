using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdrabiamyD.Models
{
    /// <summary>
    /// Obiekt modelujący stronę książki pobraną z Odrabiamy.pl
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Numer strony
        /// </summary>
        public int Number { get; init; }
        /// <summary>
        /// Zawartość strony - kod HTML pobrany z Odrabiamy.pl
        /// </summary>
        public string Content { get; init; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="content"></param>
        public Page(int number, string content)
        {
            Number = number;
            Content = content;
        }
    }
}
