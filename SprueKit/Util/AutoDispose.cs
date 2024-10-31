using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprueKit.Util
{
    /// <summary>
    /// Automatically disposes of contents when exchanged
    /// </summary>
    /// <typeparam name="T">IDisposable object to be wrapped</typeparam>
    public class AutoDispose<T> : IDisposable where T : class, IDisposable
    {
        public T Value { get; private set; }

        public T Get() { return Value; }

        public bool Set(T val)
        {
            if (Value != null)
            {
                Value.Dispose();
                Value = val;
                return true;
            }
            Value = val;
            return false;
        }

        public void Dispose()
        {
            if (Value != null)
                Value.Dispose();
            Value = null;
        }
    }

    public class RefDispose<T> : IDisposable where T : class, IDisposable
    {
        public class Counter<T> : IDisposable where T : class, IDisposable
        {
            public T Value { get; private set; }

            int refCount_ = 0;

            public Counter(T value)
            {
                refCount_ = 1;
                Value = value;
            }

            public Counter<T> Increment()
            {
                lock (this)
                {
                    refCount_ += 1;
                    return this;
                }
            }

            public void Decrement()
            {
                lock (this)
                {
                    refCount_ -= 1;
                    if (refCount_ == 0)
                        Dispose();
                }
            }

            ~Counter()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (Value != null)
                {
                    Value.Dispose();
                    Value = null;
                    refCount_ = 0;
                }
            }
        }

        Counter<T> counter_;

        public T Value {
            get
            {
                if (counter_ != null)
                {
                    lock (counter_)
                    {
                        return counter_.Value;
                    }
                }
                return null;
            }
            set
            {
                if (counter_ == null)
                    counter_ = new Counter<T>(value);
            }
        }

        private RefDispose() { }
        public RefDispose(T val)
        {
            counter_ = new Counter<T>(val);
        }

        public RefDispose<T> GetHandle()
        {
            return new Util.RefDispose<T> { counter_ = counter_.Increment() };
        }

        public void SetFrom(RefDispose<T> rhs)
        {
            if (rhs.counter_ != null)
                counter_ = rhs.counter_.Increment();
        }

        public void Dispose()
        {
            if (counter_ != null)
            {
                counter_.Decrement();
                counter_ = null;
            }
        }
    }
}
