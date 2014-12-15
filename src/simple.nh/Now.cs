using System;

namespace Simple.NH
{
    public interface INow
    {
        DateTime GetNow();

        DateTime GetUtcNow();

        DateTimeOffset GetOffsetNow();
    }

    public class Now : INow
    {
        static Now()
        {
            Current = new Now();
        }

        public static INow Current { get; private set; }

        public DateTime GetNow()
        {
            return DateTime.Now;
        }

        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        public DateTimeOffset GetOffsetNow()
        {
            return new DateTimeOffset(this.GetNow());
        }

        public class NowGuard : IDisposable
        {
            private readonly INow _now;

            public NowGuard()
            {
                _now = Current;
            }

            public NowGuard(INow provider)
            {
                _now = Current;
                Current = provider;
            }

            public void Dispose()
            {
                Current = _now;
            }
        }
    }
}
