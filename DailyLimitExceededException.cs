using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdrabiamyD
{
    /// <summary>
    /// Exception thrown when the daily limit for premium downloads has been exceeded
    /// </summary>
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
