using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.NH
{
    public static class Pipeline
    {
        [ThreadStatic]
        private static List<Delegate> _actions;

        /// <summary>
        /// Registers an action for the event on the current thread. This method should always be 
        /// called in the context of a using statement to automatically unregister the action.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <param name="action">Action that will be called when an event is raised.</param>
        /// <returns>
        /// IDisposable object for use within a 'using' statement.
        /// </returns>
        public static IDisposable Register<T>(Action<T> action)
        {
            action.CheckArg("action");

            if (_actions == null)
                _actions = new List<Delegate>();

            _actions.Add(action);

            return new PipelineRegistration<T>(action);
        }


        /// <summary>
        /// Publishes an event on the event bus and will invoke 0 or many listeners in the order in which they were registered.
        /// Listeners are discovered both from explicit registration and via the container.  The container will be inspected and invoked 
        /// first followed by explicit registrations.  The invocation of these listeners happens synchronously and a count of the number 
        /// of invoked listeners is returned from the method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static int Raise<T>(T arg)
        {
            int count = 0;

            if (_actions != null)
            {
                foreach (var action in _actions.OfType<Action<T>>())
                {
                    action(arg);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Unregisters a callback for the event on the current thread.
        /// Internal to force use of Register IDisposable pattern
        /// </summary>
        internal static void UnRegister<T>(Action<T> action)
        {
            if (_actions == null)
                return;

            _actions.Remove(action);
        }

        /// <summary>
        /// A guard pattern over the EventBus's Register/Unregister methods.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        sealed class PipelineRegistration<T> : IDisposable
        {
            private readonly Action<T> _action;
            private bool _disposed;

            public PipelineRegistration(Action<T> action)
            {
                _action = action;
            }

            /// <summary>
            /// Dispose pattern implemented over the guard.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Dispose pattern implemented over the guard.
            /// </summary>
            private void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        Pipeline.UnRegister(_action);
                    }

                    _disposed = true;
                }
            }

            /// <summary>
            /// The finalizer for the object which will attempt to dispose
            /// </summary>
            ~PipelineRegistration()
            {
                Dispose(false);
            }
        }
    }

    /// <summary>
    /// Top level interface for modeling an event listener.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPipelineHandler<in T>
    {
        /// <summary>
        /// The method invoked by the EventBus when an event raiser is matched to the listener.
        /// </summary>
        /// <param name="event">The instance of the event.</param>
        void Handle(T @event);
    }
}
