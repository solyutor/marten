using System;
using System.Collections.Concurrent;
using System.Linq;
using Marten.Linq;

namespace Marten
{
    internal readonly struct CompiledQueryKey: IEquatable<CompiledQueryKey>
    {
        private static readonly ConcurrentDictionary<Type, bool> SupportsEquatableCache = new ConcurrentDictionary<Type, bool>();

        private readonly Type _queryType;
        private readonly object _query;

        private CompiledQueryKey(Type queryType, object query = null)
        {
            _queryType = queryType ?? throw new ArgumentNullException(nameof(queryType));
            _query = query;
        }

        public bool Equals(CompiledQueryKey other)
        {
            return _queryType == other._queryType && Equals(_query, other._query);
        }

        public override bool Equals(object obj)
        {
            return obj is CompiledQueryKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_queryType.GetHashCode() * 397) ^ (_query?.GetHashCode() ?? 0);
            }
        }

        public static CompiledQueryKey ForEquatable<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query)
        {
            var queryType = query.GetType();

            return SupportsEquatableCache.GetOrAdd(queryType, type => type.GetInterfaces().Contains(typeof(IEquatable<>).MakeGenericType(type)))
                ? new CompiledQueryKey(queryType, query)
                : new CompiledQueryKey(queryType, null);
        }

        public static CompiledQueryKey ByType<TDoc, TOut>(ICompiledQuery<TDoc, TOut> query)
        {
            return new CompiledQueryKey(query.GetType());   
        }
    }
}
