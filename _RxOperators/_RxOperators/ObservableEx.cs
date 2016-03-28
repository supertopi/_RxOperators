using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;

namespace System.Linq
{
    public static partial class ObservableEx
    {

        /// <summary>
        /// Acts like a Buffer but is limited with a silence threshold with Throttle
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="maxAmount"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static IObservable<IList<TSource>> BufferWithThrottle<TSource>
                                                  (this IObservable<TSource> source,
                                                   int maxAmount, TimeSpan threshold)
        {
            return Observable.Create<IList<TSource>>((obs) =>
            {
                return source.GroupByUntil(_ => true,
                                           g => g.Throttle(threshold).Select(_ => Unit.Default)
                                                 .Merge(g.Buffer(maxAmount).Select(_ => Unit.Default)))
                             .SelectMany(i => i.ToList())
                             .Subscribe(obs);
            });
        }

        /// <summary>
        /// Buffers elements for a timespan when they become available, 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static IObservable<IList<TSource>> BufferWhenAvailable<TSource>(this IObservable<TSource> source, TimeSpan threshold)
        {
            return source.Publish(sp =>
                            sp.GroupByUntil(_ => true, _ => Observable.Timer(threshold))
                              .SelectMany(i => i.ToList()));

        }


        /// <summary>
        /// Monitors for matches in given Observables
        /// </summary>
        ///  <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private static IObservable<string> MonitorMatches<TSource>(this IObservable<TSource> source,
                                                                    IObservable<TSource> other,
                                                                    TimeSpan threshold)
        {
            return Observable.Create<string>((obs) =>
            {
                other = other.Publish().RefCount();

                return source.Subscribe(i =>
                {
                    Observable.Merge(Observable.Timer(threshold).Select(_ => $"Timeout for A{i}"),
                                     other.Where(j => j.Equals(i)).Select(_ => $"Got matching B for A{i}"))
                              .Take(1)
                              .Subscribe(obs);
                });
            });
        }




    }
}