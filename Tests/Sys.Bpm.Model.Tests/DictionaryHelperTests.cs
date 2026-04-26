using Sys.Workflow;
using System.Collections.Generic;
using Xunit;

namespace Sys.Bpm.Model.Tests
{
    public class DictionaryHelperTests
    {
        [Fact]
        public void PutAll_MergesSourceIntoTarget()
        {
            var target = new Dictionary<string, int>
            {
                ["a"] = 1,
                ["b"] = 2
            };
            var source = new Dictionary<string, int>
            {
                ["c"] = 3,
                ["d"] = 4
            };

            target.PutAll(source);

            Assert.Equal(4, target.Count);
            Assert.Equal(1, target["a"]);
            Assert.Equal(2, target["b"]);
            Assert.Equal(3, target["c"]);
            Assert.Equal(4, target["d"]);
        }

        [Fact]
        public void PutAll_KeyConflict_PreservesOriginalValue()
        {
            var target = new Dictionary<string, int>
            {
                ["a"] = 1,
                ["b"] = 2
            };
            var source = new Dictionary<string, int>
            {
                ["b"] = 20,
                ["c"] = 3
            };

            target.PutAll(source);

            Assert.Equal(3, target.Count);
            Assert.Equal(1, target["a"]);
            Assert.Equal(2, target["b"]); // 原值保留，不被覆盖
            Assert.Equal(3, target["c"]);
        }

        [Fact]
        public void PutAll_NullSource_DoesNotThrow()
        {
            var target = new Dictionary<string, int>
            {
                ["a"] = 1
            };

            target.PutAll(null);

            Assert.Single(target);
            Assert.Equal(1, target["a"]);
        }

        [Fact]
        public void PutAll_EmptySource_DoesNotModifyTarget()
        {
            var target = new Dictionary<string, int>
            {
                ["a"] = 1
            };
            var source = new Dictionary<string, int>();

            target.PutAll(source);

            Assert.Single(target);
        }

        [Fact]
        public void PutAll_ReturnsTargetDictionary()
        {
            var target = new Dictionary<string, int>();
            var source = new Dictionary<string, int> { ["a"] = 1 };

            IDictionary<string, int> result = target.PutAll(source);

            Assert.Same(target, result);
        }
    }
}
