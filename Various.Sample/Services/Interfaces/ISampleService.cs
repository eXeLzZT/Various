using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Various.Sample.Services.Interfaces
{
    public interface ISampleService
    {
        IObservable<IChangeSet<SampleItem>> Connect();
        IObservable<Unit> Resort { get; }
        void Update();
    }
}
