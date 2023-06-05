using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Agro.LibCore
{
    public interface ICancelTracker
    {
        //void Reset();
        bool Cancel();
    }
    public class NotCancelTracker : ICancelTracker
    {
        public bool Cancel()
        {
            return false;
        }
        public static NotCancelTracker Instance = new();
    }
    public class TokenCancelTracker : ICancelTracker
    {
        private CancellationToken _token;
        public TokenCancelTracker(CancellationToken token)
        {
            _token = token;
        }
        public bool Cancel()
        {
            return _token.IsCancellationRequested;
        }
    }
}
