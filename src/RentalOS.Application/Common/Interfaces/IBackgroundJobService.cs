using System.Linq.Expressions;

namespace RentalOS.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    string Enqueue<T>(Expression<Action<T>> methodCall);
}
