using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RegionsWasher
{
    internal interface ITinySubscriber
    {
    }

    internal interface ITinySubscriber<T> : ITinySubscriber
    {
        void Receive(T message);
    }

    internal class TinyMessageBroker
    {
        private readonly Dictionary<Type, List<WeakReference<ITinySubscriber>>> subscribersMap;

        static TinyMessageBroker()
        {
            Instance = new TinyMessageBroker();
        }

        private TinyMessageBroker()
        {
            subscribersMap = new Dictionary<Type, List<WeakReference<ITinySubscriber>>>();
        }

        public static TinyMessageBroker Instance { get; }

        public void Publish<T>(T message)
        {
            lock (subscribersMap)
            {
                if (subscribersMap.TryGetValue(typeof(T), out var subscribers))
                {
                    foreach (var subscriber in subscribers.ToArray())
                    {
                        if (subscriber.TryGetTarget(out var tinySubscriber))
                        {
                            try
                            {
                                var typedSubscriber = tinySubscriber as ITinySubscriber<T>;
                                typedSubscriber?.Receive(message);
                            }
                            catch (Exception exception)
                            {
                                // do not care about any exception
                                Debug.WriteLine($"publish exception: {exception}");
                            }
                        }
                        else
                        {
                            subscribers.Remove(subscriber);
                            Debug.WriteLine($"remove vanished subscriber type={typeof(T)} total={subscribers.Count}");
                            if (!subscribers.Any())
                            {
                                subscribersMap.Remove(typeof(T));
                                Debug.WriteLine($"remove empty subscribers list type={typeof(T)}");
                            }
                        }
                    }
                }
            }
        }

        public void Subscribe<T>(ITinySubscriber<T> subscriber)
        {
            lock (subscribersMap)
            {
                if (!subscribersMap.TryGetValue(typeof(T), out var subscribers))
                {
                    subscribers = new List<WeakReference<ITinySubscriber>>();
                    subscribersMap.Add(typeof(T), subscribers);
                }

                var weakSubscriber = new WeakReference<ITinySubscriber>(subscriber);
                subscribers.Add(weakSubscriber);
                Debug.WriteLine($"add subscriber type={typeof(T)} total={subscribers.Count}");
            }
        }

        public void Unsubscribe<T>(ITinySubscriber<T> subscriber)
        {
            lock (subscribersMap)
            {
                if (!subscribersMap.TryGetValue(typeof(T), out var subscribers))
                {
                    return;
                }

                foreach (var weakReference in subscribers.ToArray())
                {
                    if (weakReference.TryGetTarget(out var tinySubscriber))
                    {
                        if (tinySubscriber.Equals(subscriber))
                        {
                            subscribers.Remove(weakReference);
                            Debug.WriteLine($"remove subscriber type={typeof(T)} total={subscribers.Count}");
                        }
                    }
                }

                if (!subscribers.Any())
                {
                    subscribersMap.Remove(typeof(T));
                    Debug.WriteLine($"remove empty subscribers list type={typeof(T)}");
                }
            }
        }
    }
}
