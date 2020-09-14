using NUnit.Framework;
using Helios.Settings;
using UnityEngine;
using System.IO;

namespace Tests
{
    public class SuperGroupTests
    {

        #region Save and Load
        [Test]
        public void GetPath_ReturnsAString()
        {
            var group = MakeTempSuperGroup();
            Assert.IsNotEmpty(group.Path);
        }

        [Test]
        public void Saving_CreatesAFile()
        {
            var superGroup = SuperGroup.Get(GetRandomString());
            bool groupResult = superGroup.Save();
            Assert.True(groupResult);
            FileAssert.Exists(superGroup.Path);
            File.Delete(superGroup.Path);
        }

        [Test]
        public void SuperGroup_LoadedFromDisk_PreservesGroupCount()
        {
            int count = Random.Range(1, 30);
            var superGroup = MakeTempSuperGroup(count);
            var name = superGroup.Name;
            var path = superGroup.Path;
            superGroup.Save();
            superGroup = null;

            var restoredSuperGroup = SuperGroup.Get(name);
            Assert.NotNull(restoredSuperGroup);
            Assert.AreEqual(count, restoredSuperGroup.GetGroupCount());

            File.Delete(path);
        }

        [Test]
        public void StaticGetter_AfterFirstGetCall_ReturnsCachedGroup()
        {
            // Create group, save to disk, nullify
            var superGroup = MakeTempSuperGroup(5);
            var name = superGroup.Name;
            superGroup.Save();

            // Get group first time, should load from disk
            SuperGroup.Get(name).Delete();

            // Get it again, should not be null
            var secondLoad = SuperGroup.Get(name);
            Assert.NotNull(secondLoad);
        }
        #endregion

        #region Convenience Methods

        private SuperGroup MakeTempSuperGroup(int groupCount = 0)
        {
            SuperGroup superGroup = SuperGroup.Get(GetRandomString()).Delete();
            for (int i = 0; i < groupCount; i++)
            {
                superGroup.Set(GetRandomString());
            }
            return superGroup;
        }

        private string GetRandomString()
        {
            return System.Guid.NewGuid().ToString();
        }
        #endregion

    }
}
