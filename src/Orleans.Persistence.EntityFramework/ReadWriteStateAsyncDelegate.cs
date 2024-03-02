using System.Threading.Tasks;
using Orleans.Runtime;

namespace Orleans.Persistence.EntityFramework
{
    internal delegate Task ReadWriteStateAsyncDelegate(string grainType, GrainReference grainReference,
        IGrainState grainState, object storageOptions);
}