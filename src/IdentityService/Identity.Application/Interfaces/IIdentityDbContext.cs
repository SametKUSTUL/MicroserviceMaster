using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Interfaces;

public interface IIdentityDbContext
{
    DbSet<UserCredential> UserCredentials { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
