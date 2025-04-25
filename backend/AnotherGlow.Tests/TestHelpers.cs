using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace appBackend.Tests
{
    /// <summary>
    /// Helper class to mock DbSet<T> for DbContext testing.
    /// </summary>
    public static class MockDbSetHelper
    {
        public static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            // Set up IQueryable properties and methods
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // Set up asynchronous enumeration
            dbSetMock.As<IAsyncEnumerable<T>>().Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            // Mock Add/AddRange
            dbSetMock.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(sourceList.Add);
            dbSetMock.Setup(d => d.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(sourceList.AddRange);
            dbSetMock.Setup(d => d.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                     .Callback<T, CancellationToken>((entity, ct) => sourceList.Add(entity))
                     .ReturnsAsync((T entity, CancellationToken ct) => null!); // Return type doesn't matter much here

            dbSetMock.Setup(d => d.AddRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
                     .Callback<IEnumerable<T>, CancellationToken>((entities, ct) => sourceList.AddRange(entities))
                     .Returns(Task.CompletedTask);


            // Mock Remove/RemoveRange
            dbSetMock.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>(t => sourceList.Remove(t));
            dbSetMock.Setup(d => d.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(ts =>
            {
                foreach (var t in ts.ToList()) { sourceList.Remove(t); }
            });

            // Mock FindAsync (needs explicit setup, especially for composite keys if applicable)
            // This basic version works for single primary key of type object[]
            dbSetMock.Setup(d => d.FindAsync(It.IsAny<object[]>()))
                .Returns<object[]>(async keyValues =>
                {
                    // This basic FindAsync mock assumes the key is the first property.
                    // Adjust if your primary key is different or composite in a complex way.
                    var keyType = typeof(T).GetProperties().First(p =>
                        System.ComponentModel.DataAnnotations.KeyAttribute.GetCustomAttribute(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)) != null
                        || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) // Common conventions
                        || p.Name.Equals(typeof(T).Name + "Id", StringComparison.OrdinalIgnoreCase)
                        || p.Name.Equals(keyValues[0].GetType().Name + "Id", StringComparison.OrdinalIgnoreCase) // Heuristic
                        )?.PropertyType;

                    if (keyType == null || keyValues.Length == 0)
                        return await Task.FromResult<T?>(null); // Cannot determine key

                    // Attempt to convert the first key value to the key property type
                    object? targetKey = null;
                    try
                    {
                        targetKey = Convert.ChangeType(keyValues[0], keyType);
                    }
                    catch
                    {
                        return await Task.FromResult<T?>(null); // Conversion failed
                    }

                    if (targetKey == null)
                        return await Task.FromResult<T?>(null);


                    foreach (var entity in sourceList)
                    {
                        var keyProp = entity.GetType().GetProperty(keyType.Name + "Id") // Try common convention first
                                       ?? entity.GetType().GetProperties().First(p => p.PropertyType == keyType &&
                                           (System.ComponentModel.DataAnnotations.KeyAttribute.GetCustomAttribute(p, typeof(System.ComponentModel.DataAnnotations.KeyAttribute)) != null
                                            || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)
                                            || p.Name.Equals(typeof(T).Name + "Id", StringComparison.OrdinalIgnoreCase)));

                        if (keyProp != null && targetKey.Equals(keyProp.GetValue(entity)))
                        {
                            return await Task.FromResult<T?>(entity);
                        }
                    }
                    return await Task.FromResult<T?>(null);
                });


            return dbSetMock;
        }
    }

    // Helper classes for async query provider (needed for EF Core async operations on IQueryable)
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            // You might need more sophisticated logic here depending on what async operations you use.
            // For simple ToListAsync, FirstOrDefaultAsync etc., Task.FromResult often works.
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                                     .GetMethod(
                                          name: nameof(IQueryProvider.Execute),
                                          genericParameterCount: 1,
                                          types: new[] { typeof(Expression) })
                                     ?.MakeGenericMethod(resultType)
                                     .Invoke(this, new[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                                        ?.MakeGenericMethod(resultType)
                                        .Invoke(null, new[] { executionResult })!;

            // Simpler version if the above reflection is too complex or fails:
            // return Task.FromResult(Execute<TResult>(expression));
        }

        //IAsyncEnumerable<TResult> IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression)
        //{
        //    return new TestAsyncEnumerable<TResult>(expression);
        //}
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose() => _inner.Dispose(); // Synchronous Dispose is fine
        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask; // Simple implementation
        }


        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(_inner.MoveNext());
        }
    }
}