using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdrabiamyD.Models
{
    /// <summary>
    /// An object that models a page
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Page number
        /// </summary>
        public int Number { get; init; }
        /// <summary>
        /// The content of a page
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
