using NUnit.Framework;
using UnityEngine;
using Helios.Settings;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Tests
{
    public class GroupTests
    {
        [Test]
        public void Value_SetFromInt_ReturnsEqualInt()
        {
            Group group = MakeTempGroup();
            string groupName = "IntTest";
            int value = Random.Range(0, int.MaxValue);
            group.Set(groupName, value);

            Assert.AreEqual(value, group.Get<int>(groupName));
        }

        [Test]
        public void Value_SetFromString_ReturnsEqualString()
        {
            Group group = MakeTempGroup();
            string groupName = "StringTest";
            string value = System.Guid.NewGuid().ToString();
            group.Set(groupName, value);

            Assert.AreEqual(value, group.Get<string>(groupName));
        }

        [Test]
        public void Value_SetFromFloat_ReturnsEqualFloat()
        {
            Group group = MakeTempGroup();
            string groupName = "FloatTest";
            float value = Random.Range(0, 1000);
            group.Set(groupName, value);

            Assert.AreEqual(value, group.Get<float>(groupName));
        }

        [Test]
        public void Value_SetFromVector3_ReturnsEqualVector3()
        {
            Group group = MakeTempGroup();
            string groupName = "Vector3Test";
            Vector3 value = Random.onUnitSphere;
            group.Set(groupName, value);

            Assert.AreEqual(value, group.Get<Vector3>(groupName));
        }

        [Test]
        public void StringValue_NotInGroup_ReturnsIncludedValue()
        {
            Group group = MakeTempGroup();
            string value = "SomeString";
            Assert.AreEqual(value, group.Get("NonExistent", value));
        }

        [Test]
        public void IntValue_NotInGroup_ReturnsIncludedValue()
        {
            Group group = MakeTempGroup();
            int value = Random.Range(0, int.MaxValue);
            var returnValue = group.Get("randomName", value);
            Assert.AreEqual(value, returnValue);
        }

        [Test]
        public void FloatValue_NotInGroup_ReturnsIncludedValue()
        {
            Group group = MakeTempGroup();
            float value = Random.value;
            Assert.AreEqual(value, group.Get("NonExistent", value));
        }

        //[Test]
        //public void Serializing_PreservesFieldCount()
        //{
        //    int count = Random.Range(0, 30);
        //    var group = MakeTempGroup(count);
        //    JObject jObject = (JObject)JObject.Parse(group.ToJson()).GetValue("_fields");
        //    Assert.AreEqual(count, jObject.Count);
        //}

        [Test]
        public void GroupName_SetInConstructor_IsApplied()
        {
            var name = GetRandomString();
            var group = Group.Get(name);
            Assert.AreEqual(name, group.Name);
        }

        [Test]
        public void GroupFieldCount_AfterNew_IsZero()
        {
            var group = MakeTempGroup(0).DeleteGroup();
            Assert.AreEqual(0, group.GetFieldCount());
        }

        [Test]
        public void GroupFieldCount_AfterSettingOneGroup_IsOne()
        {
            var group = MakeTempGroup(1);
            Assert.AreEqual(1, group.GetFieldCount());
        }

        [Test]
        public void StringValue_SetOnGroup_IsEqualToOriginal()
        {
            Group group = MakeTempGroup().DeleteGroup();
            string value = "testValue";
            group.Set("testField", value);
            Assert.AreEqual(value, group.Get<string>("testField"));
        }

        [Test]
        public void IntValue_SetAtCreation_IsEqualToOriginal()
        {
            Group group = MakeTempGroup();
            int value = Random.Range(0,int.MaxValue);
            string fieldName = "testField";
            group.Set(fieldName, value);

            var returnValue = group.Get<int>(fieldName);
            Assert.AreEqual(value, returnValue);
        }

        [Test]
        public void IntValue_SetAfterCreation_IsEqualToOriginal()
        {
            Group group = MakeTempGroup();
            int value = Random.Range(0, int.MaxValue);
            string fieldName = "testField";
            group.Set(fieldName, value);

            int value2 = value + 1;
            group.Set(fieldName, value2);
            Assert.AreEqual(value2, group.Get<int>(fieldName));
        }

        [Test]
        public void Getter_InvalidType_ThrowsException()
        {
            Group group = MakeTempGroup();
            int value = Random.Range(0, int.MaxValue);
            string fieldName = "testField";
            group.Set(fieldName, value);

            Assert.Throws<System.InvalidCastException>(() => {
                group.Get<string>(fieldName);
            });
        }

        [Test]
        public void Setter_InvalidType_ThrowsException()
        {
            Group group = MakeTempGroup();
            int value = Random.Range(0, int.MaxValue);
            string fieldName = "testField";
            group.Set(fieldName, value);

            Assert.Throws<System.InvalidCastException>(() => {
                group.Set(fieldName, "newTest");
            });
        }

        [Test]
        public void Delete_Field_ReducesCountByOne()
        {
            Group group = MakeTempGroup();
            string fieldName = GetRandomString();
            group.Set(fieldName, "Test");
            int preRemoveCount = group.GetFieldCount();
            group.DeleteField(fieldName);
            int postRemoveCount = group.GetFieldCount();
            group.DeleteGroup();
            Assert.AreEqual(preRemoveCount - 1, postRemoveCount);
        }

        #region Save and Load
        [Test]
        public void GetPath_ReturnsAString()
        {
            var group = MakeTempGroup();
            Assert.IsNotEmpty(group.Path);
        }

        [Test]
        public void Saving_CreatesAFile()
        {
            var group = Group.Get(GetRandomString());
            bool groupResult = group.Save();
            Assert.True(groupResult);
            FileAssert.Exists(group.Path);
            System.IO.File.Delete(group.Path);
        }

        [Test]
        public void Group_LoadedFromDisk_PreservesFieldCount()
        {
            int count = Random.Range(1, 30);
            var group = MakeTempGroup(count);
            var name = group.Name;
            var path = group.Path;
            group.Save();
            group = null;

            var restoredGroup = Group.Get(name);
            Assert.NotNull(restoredGroup);
            Assert.AreEqual(count, restoredGroup.GetFieldCount());

            File.Delete(path);
        }

        [Test]
        public void StaticGetter_GivenGroupNameOnDisk_ReturnsGroupWithMatchingFieldCount()
        {
            var count = Random.Range(0, 100);
            var group = MakeTempGroup(count);
            var name = group.Name;
            group.Save();

            var restoredGroup = Group.Get(name).DeleteGroup();
            Assert.AreEqual(count, restoredGroup.GetFieldCount());
        }

        [Test]
        public void StaticGetter_AfterFirstGetCall_ReturnsCachedGroup()
        {
            // Create group, save to disk, nullify
            var group = MakeTempGroup(5);
            var name = group.Name;
            group.Save();

            // Get group first time, should load from disk
            Group.Get(name).DeleteGroup();

            // Get it again, should not be null
            var secondLoad = Group.Get(name);
            Assert.NotNull(secondLoad);
        }
        #endregion

        #region Convenience Methods

        private Group MakeTempGroup(int fieldCount = 0)
        {
            Group group = Group.Get(GetRandomString()).DeleteGroup();
            for (int i = 0; i < fieldCount; i++)
            {
                group.Set(GetRandomString(), Random.value);
            }
            return group;
        }

        private string GetRandomString()
        {
            return System.Guid.NewGuid().ToString();
        }
        #endregion

    }
}
