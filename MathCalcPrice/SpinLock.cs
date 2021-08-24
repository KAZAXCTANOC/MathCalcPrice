using System.Threading;

namespace MathCalcPrice
{
    class SpinLock
    {
        private int _locked;

        public void Lock()
        {
            while (Interlocked.CompareExchange(ref _locked, 1, 0) != 0)
                continue; // spin
        }

        public void Unlock()
        {
            _locked = 0;
        }
    }
}
