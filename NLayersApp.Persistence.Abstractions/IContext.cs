using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NLayersApp.Persistence.Abstractions
{
    public interface IContext: IDisposable
    {
        DbSet<T> Set<T>() where T: class;
        Task SaveChangesAsync(CancellationToken cancellationToken);
        IMutableModel ExternalModel { get; set; }
        IModel Model { get; }
    }
}