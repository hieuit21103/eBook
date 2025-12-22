using Domain.Entities;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;

namespace Extensions;

public static class UserExtensions
{
    public static IQueryable<User> ApplyFilters(this IQueryable<User> query, UserFilterParams filterParams)
    {
        if (!string.IsNullOrEmpty(filterParams.Keyword))
        {
            var lowerKeyword = filterParams.Keyword.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(lowerKeyword) ||
                u.Email.ToLower().Contains(lowerKeyword));
        }

        if (!string.IsNullOrEmpty(filterParams.Email))
        {
            query = query.Where(u => u.Email.Contains(filterParams.Email));
        }

        if (!string.IsNullOrEmpty(filterParams.Username))
        {
            query = query.Where(u => u.Username.Contains(filterParams.Username));
        }

        if (filterParams.Role.HasValue)
        {
            query = query.Where(u => u.Role == filterParams.Role.Value);
        }

        if (filterParams.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filterParams.IsActive.Value);
        }

        return query;
    }
}
