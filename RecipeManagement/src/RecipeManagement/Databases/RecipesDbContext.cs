namespace RecipeManagement.Databases;

using System.Data.Common;
using RecipeManagement.Domain;
using RecipeManagement.Databases.EntityConfigurations;
using RecipeManagement.Services;
using Configurations;
using MediatR;
using RecipeManagement.Domain.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql;
using Resources;

public interface IJobContext
{
    DbSet<Job> Jobs { get; set; }
}

public sealed class RecipesDbContext : DbContext, IJobContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RecipesDbContext(
        DbContextOptions<RecipesDbContext> options, ICurrentUserService currentUserService, IMediator mediator, IDateTimeProvider dateTimeProvider) : base(options)
    {
        _currentUserService = currentUserService;
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
    }

    #region DbSet Region - Do Not Delete
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Job> Jobs { get; set; }
    #endregion DbSet Region - Do Not Delete

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.FilterSoftDeletedRecords();
        /* any query filters added after this will override soft delete 
                https://docs.microsoft.com/en-us/ef/core/querying/filters
                https://github.com/dotnet/efcore/issues/10275
        */

        #region Entity Database Config Region - Only delete if you don't want to automatically add configurations
        modelBuilder.ApplyConfiguration(new RecipeConfiguration());
        #endregion Entity Database Config Region - Only delete if you don't want to automatically add configurations
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        var result = base.SaveChanges();
        _dispatchDomainEvents().GetAwaiter().GetResult();
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        UpdateAuditFields();
        var result = await base.SaveChangesAsync(cancellationToken);
        await _dispatchDomainEvents();
        return result;
    }
    
    private async Task _dispatchDomainEvents()
    {
        var domainEventEntities = ChangeTracker.Entries<BaseEntity>()
            .Select(po => po.Entity)
            .Where(po => po.DomainEvents.Any())
            .ToArray();

        foreach (var entity in domainEventEntities)
        {
            var events = entity.DomainEvents.ToArray();
            entity.DomainEvents.Clear();
            foreach (var entityDomainEvent in events)
                await _mediator.Publish(entityDomainEvent);
        }
    }
        
    private void UpdateAuditFields()
    {
        var now = _dateTimeProvider.DateTimeUtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.UpdateCreationProperties(now, _currentUserService?.UserId);
                    entry.Entity.UpdateModifiedProperties(now, _currentUserService?.UserId);
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdateModifiedProperties(now, _currentUserService?.UserId);
                    break;
                
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.UpdateModifiedProperties(now, _currentUserService?.UserId);
                    entry.Entity.UpdateIsDeleted(true);
                    break;
            }
        }
    }
}

public static class Extensions
{
    public static void FilterSoftDeletedRecords(this ModelBuilder modelBuilder)
    {
        Expression<Func<BaseEntity, bool>> filterExpr = e => !e.IsDeleted;
        foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes()
            .Where(m => m.ClrType.IsAssignableTo(typeof(BaseEntity))))
        {
            // modify expression to handle correct child type
            var parameter = Expression.Parameter(mutableEntityType.ClrType);
            var body = ReplacingExpressionVisitor
                .Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
            var lambdaExpression = Expression.Lambda(body, parameter);

            // set filter
            mutableEntityType.SetQueryFilter(lambdaExpression);
        }
    }
    
    public static async Task ListenForNotificationsAsync(
        this DbContext context, 
        string channelName, 
        Action<NpgsqlNotificationEventArgs> onNotificationReceived,
        CancellationToken cancellationToken = default)
    {
        var npgsqlConnection = context.Database.GetDbConnection() as NpgsqlConnection;

        if (npgsqlConnection == null)
        {
            throw new InvalidOperationException("The database connection is not a NpgsqlConnection.");
        }

        // Open the connection if it's not already open
        if (npgsqlConnection.State != System.Data.ConnectionState.Open)
        {
            await npgsqlConnection.OpenAsync(cancellationToken);
        }

        // Set up the notification event handling
        npgsqlConnection.Notification += (o, e) => onNotificationReceived(e);

        // Start listening to the specified channel
        await using (var command = new NpgsqlCommand($"LISTEN {channelName}", npgsqlConnection))
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        // Keep the connection alive, listening for notifications
        while (!cancellationToken.IsCancellationRequested)
        {
            await npgsqlConnection.WaitAsync(cancellationToken);
        }
    }
    
    public static async Task NotifyAsync(
        this DbContext context, 
        string channelName, 
        string payload,
        CancellationToken cancellationToken = default)
    {
        var npgsqlConnection = context.Database.GetDbConnection() as NpgsqlConnection;

        if (npgsqlConnection == null)
        {
            throw new InvalidOperationException("The database connection is not a NpgsqlConnection.");
        }

        // Open the connection if it's not already open
        if (npgsqlConnection.State != System.Data.ConnectionState.Open)
        {
            await npgsqlConnection.OpenAsync(cancellationToken);
        }

        // Use raw SQL to send a NOTIFY command with the payload
        await using var command = new NpgsqlCommand($"NOTIFY {channelName}, '{payload}'", npgsqlConnection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public class ForUpdateSkipLockedInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        if (command.CommandText.Contains(Consts.RowLockTag))
        {
            command.CommandText += " FOR UPDATE SKIP LOCKED";
        }

        return base.ReaderExecuting(command, eventData, result);
    }
}



