using System;
using NUnit.Framework;

namespace ORM
{
    [TestFixture]
    public class PostgreSQL
    {
        [Test]
        public void TestTake()
        {
            var db = new PostgreSQLConnection();

            var results = db.Take("test").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Marla", results[1][0]);
        }

        [Test]
        public void TestWhere()
        {
            var db = new PostgreSQLConnection();

            var results = db.Take("test").Where(Pairing.Of("name", "Susan"), Pairing.Of("id", 2)).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);
        }

        [Test]
        public void TestOrderBy()
        {
            var db = new PostgreSQLConnection();

            // Test default orderby
            var results = db.Take("test").OrderBy("name").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(5, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);

            // Test orderby ascending
            results = db.Take("test").OrderBy("name", "asc").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(5, results[0].Count);
            Assert.AreEqual("Jenna", results[1][0]);
        }

        [Test]
        public void TestLimit()
        {
            var db = new PostgreSQLConnection();

            var results = db.Take("test").OrderBy("name").Limit(1).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);
        }

        [Test]
        public void TestInsert()
        {
            var db = new PostgreSQLConnection();

            db.Insert("test", Pairing.Of("name", "Graydon")).Execute();

            var results = db.Take("test").Where(Pairing.Of("name", "Graydon")).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Graydon", results[1][0]);
            Assert.AreEqual(1, results[0].Count);

            db.Delete("test").Where(Pairing.Of("name", "Graydon")).Execute();
        }

        [Test]
        public void TestUpdate()
        {
            var db = new PostgreSQLConnection();

            db.Insert("test", Pairing.Of("name", "Graydon")).Execute();
            db.Update("test", Pairing.Of("name", "Graydon Update")).Where(Pairing.Of("name", "Graydon")).Execute();

            var results = db.Take("test").Where(Pairing.Of("name", "Graydon Update")).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Graydon Update", results[1][0]);
            Assert.AreEqual(1, results[0].Count);

            db.Delete("test").Where(Pairing.Of("name", "Graydon Update")).Execute();
        }

        [Test]
        public void TestDelete()
        {
            var db = new PostgreSQLConnection();

            db.Insert("test", Pairing.Of("name", "Graydon")).Execute();
            db.Delete("test").Where(Pairing.Of("name", "Graydon")).Execute();

            var results = db.Take("test").Where(Pairing.Of("name", "Graydon")).Execute();

            Assert.AreEqual(0, results[0].Count);
        }

        [Test]
        public void TestError()
        {
            var db = new PostgreSQLConnection();

            Assert.Throws<Exception>(() => { db.Limit(1).Execute(); }, "Something went wrong with your sql statement, please try again.");
        }
    }
}
