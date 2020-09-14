using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Helios.Settings;
using Random = UnityEngine.Random;

namespace Tests
{
    public class FieldTests
    {
        [Test]
        public void Value_WhenUpdated_EmitsEvent()
        {
            string randomString = System.Guid.NewGuid().ToString();
            Field<string> field = new Field<string>();

            bool received = false;
            field.OnValueChanged += newValue => { received = true; };
            field.Set(randomString);

            Assert.True(received);
        }

        [Test]
        public void StringValue_FromGet_IsCorrect()
        {
            string value = System.Guid.NewGuid().ToString();
            Field<string> field = new Field<string>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }

        [Test]
        public void IntValue_FromGet_IsCorrect()
        {
            int value = Random.Range(int.MinValue, int.MaxValue);
            Field<int> field = new Field<int>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }

        [Test]
        public void FloatValue_FromGet_IsCorrect()
        {
            float value = Random.Range(float.MinValue, float.MaxValue);
            Field<float> field = new Field<float>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }

        [Test]
        public void BoolValue_FromGet_IsCorrect()
        {
            bool value = Random.value > 0.5f;
            Field<bool> field = new Field<bool>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }

        [Test]
        public void ByteValue_FromGet_IsCorrect()
        {
            byte[] bytes = new byte[1];
            new System.Random().NextBytes(bytes);
            byte value = bytes[0];
            Field<byte> field = new Field<byte>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }

        [Test]
        public void BytesValue_FromGet_IsCorrect()
        {
            byte[] value = new byte[5];
            new System.Random().NextBytes(value);
            Field<byte[]> field = new Field<byte[]>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }

        [Test]
        public void Vector2Value_FromGet_IsCorrect()
        {
            Vector2 value = new Vector2(
                Random.Range(float.MinValue, float.MaxValue),
                Random.Range(float.MinValue, float.MaxValue));
            Field<Vector2> field = new Field<Vector2>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }

        [Test]
        public void Vector3Value_FromGet_IsCorrect()
        {
            Vector3 value = new Vector3(
                Random.Range(float.MinValue, float.MaxValue),
                Random.Range(float.MinValue, float.MaxValue),
                Random.Range(float.MinValue, float.MaxValue));
            Field<Vector3> field = new Field<Vector3>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }

        [Test]
        public void DateTimeValue_FromGet_IsCorrect()
        {
            DateTime value = DateTime.UtcNow;
            Field<DateTime> field = new Field<DateTime>();
            field.Set(value);

            Assert.AreEqual(value, field.Get());
        }
    }
}
