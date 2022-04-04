using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdrabiamyD
{
    /// <summary>
    /// Wyjątek zgłaszany gdy dzienny limit pobrań został przekroczony
    /// </summary>
    /// <remarks>
    /// Ja nie wiem jaki ten limit jest, mój ukochany... przyjaciel <c>https://github.com/KartoniarzEssa</c>
    /// stwierdził że jest to 60 stron, ale pobierałem ich znacznie więcej w ciągu dnia.
    /// </remarks>
    [Serializable]
    public class DailyLimitExceededException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public DailyLimitExceededException() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public DailyLimitExceededException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public DailyLimitExceededException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DailyLimitExceededException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
