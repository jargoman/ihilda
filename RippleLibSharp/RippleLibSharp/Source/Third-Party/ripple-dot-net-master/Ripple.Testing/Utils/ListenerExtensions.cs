using System;
using System.Reflection;

namespace Ripple.Testing.Utils
{
    public static class ListenerExtensions
    {
        public static void ListenOnce<TEventArgs>(this IEventEmitter eventSource,
            EventInfo eventInfo,
            Action<TEventArgs> handler)
        {
            Action<TEventArgs> internalHandler = null;
            internalHandler = (args) => {
                handler(args);
                eventInfo.RemoveEventHandler(eventSource, internalHandler);
            };
            eventInfo.AddEventHandler(eventSource, internalHandler);
        }
    }
}