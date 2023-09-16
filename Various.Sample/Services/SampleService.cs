using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Various.Sample.Services.Interfaces;

namespace Various.Sample.Services
{
    public class SampleService : ISampleService
    {
        private ISourceList<SampleItem> _sourceSampleItems;
        private Subject<Unit> _resort;
        private SampleItem _sample1;

        public SampleService()
        {
            _sourceSampleItems = new SourceList<SampleItem>();
            _resort = new Subject<Unit>();

            _sample1 = new SampleItem("1234", "SampleItem1");

            _sourceSampleItems.Add(_sample1);
            _sourceSampleItems.Add(new SampleItem("5678", "SampleItem2"));
            _sourceSampleItems.Add(new SampleItem("9123", "SampleItem3"));
        }

        public IObservable<Unit> Resort => _sourceSampleItems.CountChanged.Select(x => Unit.Default);

        public IObservable<IChangeSet<SampleItem>> Connect() => _sourceSampleItems.Connect();

        public void Update()
        {
            _sourceSampleItems.Edit(innerList =>
            {
                innerList.Remove(_sample1);
                innerList.Add(new SampleItem("5678", "SampleItem2"));
                innerList.Add(new SampleItem("6666", "SampleItem4"));
            });
        }
    }
}
