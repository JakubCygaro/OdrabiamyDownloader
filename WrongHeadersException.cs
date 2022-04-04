using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdrabiamyD
{
    /// <summary>
    /// Wyjątek zgłaszany gdy headery są żle ustawione
    /// </summary>
    [Serializable]
    public class WrongHeadersException : Exception
    {
        /// <summary>
        /// Header jaki był ustawiony w momencie zgłoszenia wyjątku
        /// </summary>
        public readonly Headers header;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="header">Header który wywłował wyjątek</param>
        public WrongHeadersException(Headers header) { this.header = header; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Header który wywłował wyjątek</param>
        /// <param name="header"></param>
        public WrongHeadersException(string message, Headers header) : base(message) 
        { 
            this.header = header; 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Header który wywłował wyjątek</param>
        /// <param name="header"></param>
        /// <param name="inner"></param>
        public WrongHeadersException(string message, Headers header,
            Exception inner) : base(message, inner) 
        {
            this.header = header;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected WrongHeadersException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
